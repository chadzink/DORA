using System;
using System.Linq;
using DORA.Access.Context.Entities;
using DORA.Access.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;

namespace DORA.Access.Context.Repositories
{
    public class RoleRepository : Repository<AccessContext, Role>
    {
        public RoleRepository(AccessContext context, IConfiguration config)
            : base(context, config)
        {
        }

        public override IQueryable<Role> FindAll()
        {
            User currentUser = this.CurrentUser();

            if (currentUser != null)
                return from ur in dbContext.UserRoles
                       join r in dbContext.Roles
                       on ur.RoleId equals r.Id
                       where ur.UserId == currentUser.Id && r.ArchivedStamp == null
                       select r;
            else
                return null;
        }

        public override Role Find(Guid id)
        {
            return (from e in this.FindAll()
                    where e.Id == id
                    select e).FirstOrDefault();
        }

        public override IQueryable<Role> FindAllWithIncludes(string[] collectionNames)
        {
            IQueryable<Role> query = this.FindAll();

            foreach(string collectionName in collectionNames)
                query = query.Include(collectionName);
                
            return query;
        }

        public override Role CopyEntity(Role current, Role update)
        {
            current.Label = update.Label;
            current.KeyCode = update.KeyCode;

            return current;
        }

        public override Role[] JoinAllAndSort(Role[] entities)
        {
            return (
                from e in this.FindAll().ToList()
                join c in entities on e.Id equals c.Id
                select c
            ).OrderBy(c => c.Id).ToArray();
        }

        public override Role[] Create(Role[] entity)
        {
            foreach (Role e in entity)
            {
                if (!e.Id.HasValue)
                    e.Id = Guid.NewGuid();
            }

            dbContext.Roles.AddRange(entity);
            dbContext.SaveChanges();

            // Add site to current user, if exist
            User currentUser = this.CurrentUser();

            foreach (Role e in entity)
            {
                dbContext.UserRoles.Add(new UserRole
                {
                    Id = Guid.NewGuid(),
                    UserId = currentUser.Id.Value,
                    RoleId = e.Id.Value,
                });
            }

            dbContext.SaveChanges();

            return entity;
        }

        public override Role[] Delete(Role[] entity)
        {
            entity = this.JoinAllAndSort(entity);

            foreach (Role dbEntity in entity)
            {
                dbEntity.ArchivedStamp = DateTime.Now;
            }
            dbContext.SaveChanges();

            return entity;
        }

        public override Role[] Restore(Guid[] id)
        {
            Role[] entity = (
                from e in dbContext.Roles
                where id.Contains(e.Id.Value)
                select e
            ).ToArray();

            foreach (Role e in entity)
            {
                e.ArchivedStamp = null;
            }

            dbContext.SaveChanges();

            return entity;
        }
    }
}