using System;
using System.Linq;
using System.Linq.Expressions;
using DORA.Navigation.Context.Entities;
using DORA.Access.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;

namespace DORA.Navigation.Context.Repositories
{
    public class RoleNavGroupRepository : Repository<NavigationContext, RoleNavGroup>
    {
        public RoleNavGroupRepository(NavigationContext context, IConfiguration config)
            : base(context, config)
        {
        }

        public override IQueryable<RoleNavGroup> FindAll()
        {
            return from s in dbContext.RoleNavGroups select s;
        }

        public override IQueryable<RoleNavGroup> FindAllWithIncludes(string[] collectionNames)
        {
            IQueryable<RoleNavGroup> query = this.FindAll();

            foreach(string collectionName in collectionNames)
                query = query.Include(collectionName);
                
            return query;
        }

        public override RoleNavGroup Find(Guid id)
        {
            return this.FindOneBy(e => e.Id == id);
        }

        public override RoleNavGroup CopyEntity(RoleNavGroup current, RoleNavGroup updates)
        {
            current.RoleId = updates.RoleId;
            current.NavGroupId = updates.NavGroupId;

            return current;
        }

        public override RoleNavGroup[] JoinAllAndSort(RoleNavGroup[] entities)
        {
            return (
                from e in this.FindAll().ToList()
                join c in entities on e.Id equals c.Id
                select c
            ).OrderBy(c => c.Id).ToArray();
        }
    }
}