using System;
using System.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using DORA.Access.Context;
using DORA.Access.Context.Entities;
using DORA.Access.Context.Repositories;
using DORA.Access.Helpers;

namespace DORA.Access.Services
{
    public interface IUserService
    {
        User Authenticate(string username, string password);
        User NewUser(string username, string password, string email, string firstname = null, string lastname = null, string phonenumber = null);
        User ChangePassword(string username, string oldPassword, string newPassword);
        User CompletePasswordReset(User user, Guid token, string newPassword);
        User SerResetRequested(User user);

        User UserWithToken(User user);
        JwtRefreshToken MakeRefreshToken(string username);
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }

    public class UserService : IUserService
    {
        private readonly AccessContext _dbContext;
        private readonly UserRepository _dataRepository;
        private readonly JwtRefreshTokenRepository _jwtRefreshTokenRepository;
        readonly IConfiguration _configuration;

        public UserService(
            AccessContext dbContext,
            UserRepository dataRepository,
            JwtRefreshTokenRepository jwtRefreshTokenRepository,
            IConfiguration configuration
        )
        {
            _dbContext = dbContext;
            _dataRepository = dataRepository;
            _jwtRefreshTokenRepository = jwtRefreshTokenRepository;
            _configuration = configuration;
        }

        public IConfiguration Config
        {
            get
            {
                return this._configuration;
            }
        }

        public User Authenticate(string username, string password)
        {
            PasswordHasher passwordHasher = new PasswordHasher();

            User user = _dataRepository.FindOneBy(r => r.UserName == username && r.enabled != 0);

            if (user == null)
                return null;

            string userPasswordHash = _dataRepository.UserPasswordHash(user);

            if (userPasswordHash == null)
                return null;

            PasswordVerificationResult passwordCheck = passwordHasher.VerifyHashedPassword(userPasswordHash, password);

            // return null if user not found
            if (password == "_not_used_" || user == null || passwordCheck == PasswordVerificationResult.Failed)
                return null;

            return this.UserWithToken(user);
        }

        public User UserWithToken(User user)
        {
            return JwtToken.AddTokensToUser(
                user,
                Config["AppSettings:Secret"],
                Config["AppSettings:JwtIssuer"],
                Config["AppSettings:JwtAudience"],
                int.Parse(Config["AppSettings:JwtExpiresMinutes"]),
                int.Parse(Config["AppSettings:JwtRefreshExpiresDays"]),
                _dbContext
            );
        }

        public User NewUser(string username, string password, string email, string firstname = null, string lastname = null, string phonenumber = null)
        {
            User user = _dataRepository.FindOneBy(r => r.UserName == username);

            // user must not exist with new username provided
            if (user == null && !string.IsNullOrEmpty(email))
            {
                // check if email already exist
                user = _dataRepository.FindOneBy(r => r.Email == email);

                if (user == null)
                {
                    PasswordHasher passwordHasher = new PasswordHasher();

                    User newUser = new User
                    {
                        Id = Guid.NewGuid(),
                        UserName = username,
                        DisplayName = username.ToUpper(),
                        enabled = 1,
                        Email = email,
                        FirstName = firstname,
                        LastName = lastname,
                        Phone = phonenumber,
                    };

                    UserPassword newUserPassword = new UserPassword
                    {
                        Id = Guid.NewGuid(),
                        UserId = newUser.Id.Value,
                        Password = passwordHasher.HashPassword(password),
                        CreatedStamp = DateTime.Now
                    };

                    newUser.CurrentUserPasswordId = newUserPassword.Id.Value;

                    _dataRepository.Create(newUser);
                    _dataRepository.AssignUserPassword(newUser, newUserPassword);

                    return this.UserWithToken(newUser);
                }
            }

            return null;
        }

        public User ChangePassword(string username, string oldPassword, string newPassword)
        {
            User userWithToken = this.Authenticate(username, oldPassword);

            if (userWithToken == null)
                throw new Exception("Failed Authentication");

            // we know the user passed authentication with old password

            PasswordHasher passwordHasher = new PasswordHasher();

            UserPassword newUserPassword = new UserPassword
            {
                Id = Guid.NewGuid(),
                UserId = userWithToken.Id.Value,
                Password = passwordHasher.HashPassword(newPassword),
                CreatedStamp = DateTime.Now
            };

            string assignedNewPassword = _dataRepository.AssignUserPassword(userWithToken, newUserPassword);

            if (assignedNewPassword != "Success")
                throw new Exception(assignedNewPassword);

            return this.UserWithToken(userWithToken);
        }

        public User CompletePasswordReset(User user, Guid token, string newPassword)
        {
            // check the user status for reset password
            if (user.PasswordResetToken != token
                && user.RequestedPasswordReset < DateTime.Now.AddDays(-3)
            )
            {
                return null;
            }

            PasswordHasher passwordHasher = new PasswordHasher();

            UserPassword newUserPassword = new UserPassword
            {
                Id = Guid.NewGuid(),
                UserId = user.Id.Value,
                Password = passwordHasher.HashPassword(newPassword),
                CreatedStamp = DateTime.Now
            };

            string assignedNewPassword = _dataRepository.AssignUserPassword(user, newUserPassword);

            user.RequestedPasswordReset = null;
            user.PasswordResetToken = null;

            _dataRepository.SaveChanges(user);

            if (assignedNewPassword != "Success")
                return null;

            return this.UserWithToken(user);
        }

        public User SerResetRequested(User user)
        {
            user.RequestedPasswordReset = DateTime.Now;
            user.PasswordResetToken = Guid.NewGuid();

            return _dataRepository.SaveChanges(user);
        }

        public JwtRefreshToken MakeRefreshToken(string username)
        {
            //https://code-maze.com/using-refresh-tokens-in-asp-net-core-authentication/

            // cleanup old refresh tokens for user that have expired
            IQueryable<JwtRefreshToken> expiredRefreshTokens = _jwtRefreshTokenRepository.FindBy(new Func<JwtRefreshToken, bool>[2] {
                (r => r.UserName == username),
                (r => r.ValidUntil < DateTime.Now)
            });

            foreach (JwtRefreshToken expiredRefreshToken in expiredRefreshTokens)
            {
                _jwtRefreshTokenRepository.Delete(expiredRefreshToken);
            }

            int jwtRefreshExpiresDays = int.Parse(Config["AppSettings:JwtRefreshExpiresDays"]);

            byte[] randomNumber = new byte[32];
            System.Security.Cryptography.RandomNumberGenerator rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);

            string refreshTokenString = Convert.ToBase64String(randomNumber);

            _jwtRefreshTokenRepository.Create(new JwtRefreshToken
            {
                RefreshToken = refreshTokenString,
                UserName = username,
                ValidUntil = DateTime.UtcNow.AddDays(jwtRefreshExpiresDays),
            });

            return _jwtRefreshTokenRepository.FindOneBy(r => r.RefreshToken == refreshTokenString && r.UserName == username);
        }

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            string secret = Config["AppSettings:Secret"];

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false, //you might want to validate the audience and issuer depending on your use case
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                ValidateLifetime = false //here we are saying that we don't care about the token's expiration date
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");
            return principal;
        }
    }
}
