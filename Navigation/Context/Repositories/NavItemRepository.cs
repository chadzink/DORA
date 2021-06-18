using System;
using System.Linq;
using DORA.Navigation.Context.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using DORA.Access.Common;
using DORA.Access.Context.Entities;

namespace DORA.Navigation.Context.Repositories
{
    public class NavItemRepository : Repository<NavigationContext, NavItem>
    {
        public NavItemRepository(NavigationContext context, IConfiguration config)
            : base(context, config)
        {
        }

        public override IQueryable<NavItem> FindAll()
        {
            User currentUser = this.CurrentUser();

            if (currentUser != null)
                return from i in dbContext.NavItems
                       join rg in dbContext.RoleNavItems on i.Id equals rg.NavItemId
                       join ur in currentUser.UserRoles on rg.RoleId equals ur.RoleId
                       where ur.UserId == currentUser.Id
                       select i;
            else
                return null;
        }

        public override IQueryable<NavItem> FindAllWithIncludes(string[] collectionNames)
        {
            IQueryable<NavItem> query = this.FindAll();

            foreach(string collectionName in collectionNames)
                query = query.Include(collectionName);
                
            return query;
        }

        public override NavItem CopyEntity(NavItem current, NavItem updates)
        {
            current.NavGroupId = updates.NavGroupId;
            current.ParentNavItemId = updates.ParentNavItemId;
            current.Label = updates.Label;
            current.Key = updates.Key;
            current.Url = updates.Url;
            current.UrlTarget = updates.UrlTarget;
            current.onClickJsHandler = updates.onClickJsHandler;

            return current;
        }

        public override NavItem Find(Guid id)
        {
            return (from e in this.FindAll()
                    where e.Id == id
                    select e).FirstOrDefault();
        }

        public override NavItem[] JoinAllAndSort(NavItem[] entities)
        {
            return (
                from e in this.FindAll().ToList()
                join c in entities on e.Id equals c.Id
                select c
            ).OrderBy(c => c.Id).ToArray();
        }
    }
}