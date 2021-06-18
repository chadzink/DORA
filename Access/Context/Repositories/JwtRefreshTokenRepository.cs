using System;
using System.Linq;
using DORA.Access.Context.Entities;
using DORA.Access.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;

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

        // need to add this to complete interface -- does nothing
        public override JwtRefreshToken Find(Guid id) { return null; }

        public override IQueryable<JwtRefreshToken> FindAllWithIncludes(string[] collectionNames)
        {
            IQueryable<JwtRefreshToken> query = this.FindAll();

            foreach(string collectionName in collectionNames)
                query = query.Include(collectionName);
                
            return query;
        }

        public override JwtRefreshToken CopyEntity(JwtRefreshToken current, JwtRefreshToken updates)
        {
            current.RefreshToken = updates.RefreshToken;
            current.UserName = updates.UserName;
            current.ValidUntil = updates.ValidUntil;

            return current;
        }

        public override JwtRefreshToken[] JoinAllAndSort(JwtRefreshToken[] entities)
        {
            return (
                from e in this.FindAll().ToList()
                join c in entities on e.Id equals c.Id
                select c
            ).OrderBy(c => c.Id).ToArray();
        }
    }
}