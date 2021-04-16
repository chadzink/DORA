using System;
using System.Linq;
using System.Linq.Expressions;
using DORA.Access.Context.Entities;
using DORA.Access.Common;
using Microsoft.Extensions.Configuration;

namespace DORA.Access.Context.Repositories
{
    public class JwtRefreshTokenRepository : Repository<AccessContext, JwtRefreshToken>
    {
        public JwtRefreshTokenRepository(AccessContext context, IConfiguration config)
            : base(context, config)
        {
        }

        public override IQueryable<JwtRefreshToken> FindAll()
        {
            return from s in dbContext.JwtRefreshTokens select s;
        }

        public JwtRefreshToken Find(int id)
        {
            return this.FindOneBy(e => e.Id == id);
        }

        public override JwtRefreshToken Find(Guid id) { return null; }

        public override JwtRefreshToken FindOneBy(Expression<Func<JwtRefreshToken, bool>> criteria)
        {
            return dbContext.JwtRefreshTokens.FirstOrDefault(criteria);
        }

        public override IQueryable<JwtRefreshToken> FindBy(Expression<Func<JwtRefreshToken, bool>> criteria)
        {
            // bool isAdmin = this.CurrentUser().IsAdmin.HasValue && this.CurrentUser().IsAdmin.Value;

            IQueryable<JwtRefreshToken> query = FindAll();

            return base.FindBy(query, criteria);
        }

        public override JwtRefreshToken[] Create(JwtRefreshToken[] entity)
        {
            dbContext.JwtRefreshTokens.AddRange(entity);
            dbContext.SaveChanges();

            return entity;
        }

        public override JwtRefreshToken[] Update(JwtRefreshToken[] current, JwtRefreshToken[] previous)
        {
            if (current.Length != previous.Length)
                return null;

            dbContext.JwtRefreshTokens.AttachRange(current);

            for (int e = 0; e < current.Length; e++)
            {
                current[e].RefreshToken = previous[e].RefreshToken;
                current[e].UserName = previous[e].UserName;
                current[e].ValidUntil = previous[e].ValidUntil;
            }

            dbContext.SaveChanges();

            return current;
        }

        public override JwtRefreshToken[] SaveChanges(JwtRefreshToken[] entity)
        {
            dbContext.JwtRefreshTokens.AttachRange(entity);
            dbContext.SaveChanges();

            return entity;
        }

        public override JwtRefreshToken[] Delete(JwtRefreshToken[] entity)
        {
            dbContext.JwtRefreshTokens.RemoveRange(entity);
            dbContext.SaveChanges();

            return entity;
        }

        public override JwtRefreshToken[] Restore(Guid[] id) { return null; }
    }
}