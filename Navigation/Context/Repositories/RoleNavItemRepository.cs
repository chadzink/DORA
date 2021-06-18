using System;
using System.Linq;
using DORA.Navigation.Context.Entities;
using DORA.Access.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;

namespace DORA.Navigation.Context.Repositories
{
    public class RoleNavItemRepository : Repository<NavigationContext, RoleNavItem>
    {
        public RoleNavItemRepository(NavigationContext context, IConfiguration config)
            : base(context, config)
        {
        }

        public override IQueryable<RoleNavItem> FindAll()
        {
            return from s in dbContext.RoleNavItems select s;
        }

        public override IQueryable<RoleNavItem> FindAllWithIncludes(string[] collectionNames)
        {
            IQueryable<RoleNavItem> query = this.FindAll();

            foreach (string collectionName in collectionNames)
                query = query.Include(collectionName);

            return query;
        }

        public override RoleNavItem Find(Guid id)
        {
            return this.FindOneBy(e => e.Id == id);
        }

        public override RoleNavItem CopyEntity(RoleNavItem current, RoleNavItem updates)
        {
            current.RoleId = updates.RoleId;
            current.NavItemId = updates.NavItemId;

            return current;
        }

        public override RoleNavItem[] JoinAllAndSort(RoleNavItem[] entities)
        {
            return (
                from e in this.FindAll().ToList()
                join c in entities on e.Id equals c.Id
                select c
            ).OrderBy(c => c.Id).ToArray();
        }
    }
}