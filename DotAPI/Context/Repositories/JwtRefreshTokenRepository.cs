using System;
using System.Linq;
using System.Linq.Expressions;
using DORA.DotAPI.Context.Entities;
using Microsoft.Extensions.Configuration;

namespace DORA.DotAPI.Context.Repositories
{
    public class JwtRefreshTokenRepository : Repository<AccessContext, JwtRefreshToken>
    {
        public JwtRefreshTokenRepository(AccessContext context, IConfiguration configuration)
            : base(context, context, configuration)
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

        public override IQueryable<JwtRefreshToken> FindBy(Func<JwtRefreshToken, bool>[] criteria)
        {
            // bool isAdmin = this.CurrentUser().IsAdmin.HasValue && this.CurrentUser().IsAdmin.Value;

            IQueryable<JwtRefreshToken> query = FindAll();

            return base.FindBy(query, criteria);
        }

        public override JwtRefreshToken Create(JwtRefreshToken entity)
        {
            dbContext.JwtRefreshTokens.Add(entity);
            dbContext.SaveChanges();

            return entity;
        }

        public override JwtRefreshToken Update(JwtRefreshToken current, JwtRefreshToken entity)
        {
            current.RefreshToken = entity.RefreshToken;
            current.UserName = entity.UserName;
            current.ValidUntil = entity.ValidUntil;

            dbContext.SaveChanges();

            return current;
        }

        public override JwtRefreshToken Delete(JwtRefreshToken entity)
        {
            dbContext.JwtRefreshTokens.Remove(entity);
            dbContext.SaveChanges();

            return entity;
        }

        public override JwtRefreshToken Restore(Guid id) { return null; }
    }
}