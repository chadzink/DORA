using System;
using System.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using DORA.DotAPI.Context;
using DORA.DotAPI.Context.Entities;
using DORA.DotAPI.Context.Repositories;
using DORA.DotAPI.Helpers;

namespace DORA.DotAPI.Services
{
    public interface IUserService
    {
        User Authenticate(string username, string password);
        User NewUser(string username, string password, string email, string firstname = null, string lastname = null, string phonenumber = null);

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
            RoleResourceAccessRepository roleResourceAccessRepository,
            IConfiguration configuration
        )
        {
            _dbContext = dbContext;
            _dataRepository = dataRepository;
            _jwtRefreshTokenRepository = jwtRefreshTokenRepository;
            _configuration = configuration;
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
                _configuration["AppSettings:Secret"],
                _configuration["AppSettings:JwtIssuer"],
                _configuration["AppSettings:JwtAudience"],
                int.Parse(_configuration["AppSettings:JwtExpiresMinutes"]),
                int.Parse(_configuration["AppSettings:JwtRefreshExpiresDays"]),
                _dbContext
            );
        }

        public User NewUser(string username, string password, string email, string firstname = null, string lastname = null, string phonenumber = null)
        {
            User user = _dataRepository.FindOneBy(r => r.UserName == username);

            // user must not exist with new username provided
            if (user == null)
            {
                // set user passowrd
                PasswordHasher passwordHasher = new PasswordHasher();

                User newUser = new User {
                    Id = Guid.NewGuid(),
                    UserName = username,
                    DisplayName = username.ToUpper(),
                    enabled = 1,
                    Email = email,
                    FirstName = firstname,
                    LastName = lastname,
                    Phone = phonenumber,
                };

                UserPassword newUserPassword = new UserPassword {
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

            return null;
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

            int jwtRefreshExpiresDays = int.Parse(_configuration["AppSettings:JwtRefreshExpiresDays"]);

            byte[] randomNumber = new byte[32];
            System.Security.Cryptography.RandomNumberGenerator rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);

            string refreshTokenString = Convert.ToBase64String(randomNumber);

            _jwtRefreshTokenRepository.Create(new JwtRefreshToken
            {
                RefreshToken = refreshTokenString,
                UserName = username,
                ValidUntil = System.DateTime.UtcNow.AddDays(jwtRefreshExpiresDays),
            });

            return _jwtRefreshTokenRepository.FindOneBy(r => r.RefreshToken == refreshTokenString && r.UserName == username);
        }

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            string secret = _configuration["AppSettings:Secret"];

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
