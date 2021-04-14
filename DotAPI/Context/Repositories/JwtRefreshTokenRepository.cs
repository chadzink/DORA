using System;
using System.Linq;
using System.Linq.Expressions;
using DORA.DotAPI.Context.Entities;
using DORA.DotAPI.Common;
using Microsoft.Extensions.Configuration;

namespace DORA.DotAPI.Context.Repositories
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

        public override JwtRefreshToken Update(JwtRefreshToken current, JwtRefreshToken previous)
        {
            current.RefreshToken = previous.RefreshToken;
            current.UserName = previous.UserName;
            current.ValidUntil = previous.ValidUntil;

            dbContext.SaveChanges();

            return current;
        }

        public override JwtRefreshToken SaveChanges(JwtRefreshToken entity)
        {
            dbContext.JwtRefreshTokens.Attach(entity);
            dbContext.SaveChanges();

            return entity;
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