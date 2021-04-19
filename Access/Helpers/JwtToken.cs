using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Linq;
using DORA.Access.Context;
using DORA.Access.Context.Entities;

namespace DORA.Access.Helpers
{
    public static class JwtToken
    {
        public static User AddTokensToUser(
            User user,
            string secret,
            string JwtIssuer,
            string JwtAudience,
            int jwtExpiresMinutes,
            int jwtRefreshExpiresDays,
            AccessContext dbContext
        )
        {
            // authentication successful so generate jwt token
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

            System.DateTime ExpiredDateTime = System.DateTime.UtcNow.AddMinutes(jwtExpiresMinutes);

            byte[] key = System.Text.Encoding.ASCII.GetBytes(secret);

            List<Claim> subjectClaims = new List<Claim> {
                new Claim(ClaimTypes.GivenName, string.Format("{0} {1}", user.FirstName, user.LastName)),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email.ToString()),
                new Claim(ClaimTypes.PrimarySid, user.Id.ToString()),
                new Claim(ClaimTypes.Role, "user"),
            };

            // set all roles that apply to user
            List<Role> roles = JwtToken.GetRolesForUser(dbContext, user);
            foreach (Role role in roles)
            {
                subjectClaims.Add(new Claim(ClaimTypes.Role, role.KeyCode));
            }

            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = JwtIssuer,
                Audience = JwtAudience,
                Subject = new ClaimsIdentity(subjectClaims),
                Expires = ExpiredDateTime,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
            user.JwtToken = tokenHandler.WriteToken(token);
            user.JwtTokenExpiresOn = ExpiredDateTime;

            JwtRefreshToken refreshToken = JwtToken.MakeRefreshToken(user.UserName, jwtRefreshExpiresDays, dbContext);
            user.RefreshJwtToken = refreshToken.RefreshToken;
            user.RefreshJwtTokenExpiresOn = refreshToken.ValidUntil;

            return user;
        }

        public static JwtRefreshToken MakeRefreshToken(
            string username,
            //IJwtRefreshTokenRepository _jwtRefreshTokenRepository,
            int jwtRefreshExpiresDays,
            AccessContext dbContext
        )
        {
            // cleanup old refresh tokens for user that have expired
            IQueryable<JwtRefreshToken> expiredRefreshTokens = from rt in dbContext.JwtRefreshTokens
                                                               where rt.UserName == username && rt.ValidUntil < DateTime.Now
                                                               select rt;

            foreach (JwtRefreshToken expiredRefreshToken in expiredRefreshTokens)
            {
                dbContext.JwtRefreshTokens.Remove(expiredRefreshToken);
            }
            dbContext.SaveChanges();

            // check for existing token
            JwtRefreshToken existing = (from rt in dbContext.JwtRefreshTokens
                                        where rt.UserName == username && rt.ValidUntil >= DateTime.Now
                                        select rt).FirstOrDefault();

            if (existing == null)
            {
                // make new

                byte[] randomNumber = new byte[32];
                System.Security.Cryptography.RandomNumberGenerator rng = System.Security.Cryptography.RandomNumberGenerator.Create();
                rng.GetBytes(randomNumber);

                string refreshTokenString = Convert.ToBase64String(randomNumber);

                JwtRefreshToken newJwtRefreshToken = new JwtRefreshToken
                {
                    RefreshToken = refreshTokenString,
                    UserName = username,
                    ValidUntil = DateTime.UtcNow.AddDays(jwtRefreshExpiresDays),
                };

                dbContext.JwtRefreshTokens.Add(newJwtRefreshToken);
                dbContext.SaveChanges();

                return newJwtRefreshToken;
            }
            else
            {
                // update existing
                existing.ValidUntil = DateTime.UtcNow.AddDays(jwtRefreshExpiresDays);
                dbContext.SaveChanges();
                return existing;
            }
        }

        public static List<Role> GetRolesForUser(AccessContext dbContext, User user)
        {
            return (from ur in dbContext.UserRoles
                    join r in dbContext.Roles on ur.RoleId equals r.Id
                    where ur.UserId == user.Id
                    select r).ToList();
        }
    }
}
