using System;
using System.Linq;
using DORA.Navigation.Context.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using DORA.Access.Common;
using DORA.Access.Context.Entities;

namespace DORA.Navigation.Context.Repositories
{
    public class NavGroupRepository : Repository<NavigationContext, NavGroup>
    {
        public NavGroupRepository(NavigationContext context, IConfiguration config)
            : base(context, config)
        {
        }

        public override IQueryable<NavGroup> FindAll()
        {
            User currentUser = this.CurrentUser();

            if (currentUser != null)
                return from g in dbContext.NavGroups
                       join rg in dbContext.RoleNavGroups on g.Id equals rg.NavGroupId
                       join ur in currentUser.UserRoles on rg.RoleId equals ur.RoleId
                       where ur.UserId == currentUser.Id
                       select g;
            else
                return null;
        }

        public override IQueryable<NavGroup> FindAllWithIncludes(string[] collectionNames)
        {
            IQueryable<NavGroup> query = this.FindAll();

            foreach(string collectionName in collectionNames)
                query = query.Include(collectionName);
                
            return query;
        }

        public override NavGroup Find(Guid id)
        {
            return (from e in this.FindAll()
                    where e.Id == id
                    select e).FirstOrDefault();
        }

        public override NavGroup CopyEntity(NavGroup current, NavGroup updates)
        {
            current.GroupType = updates.GroupType;
            current.Label = updates.Label;
            current.Key = updates.Key;
            current.DefaultNavItemId = updates.DefaultNavItemId;

            return current;
        }

        public override NavGroup[] JoinAllAndSort(NavGroup[] entities)
        {
            return (
                from e in this.FindAll().ToList()
                join c in entities on e.Id equals c.Id
                select c
            ).OrderBy(c => c.Id).ToArray();
        }
    }
}