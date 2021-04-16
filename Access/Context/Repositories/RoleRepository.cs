using System;
using System.Linq;
using System.Linq.Expressions;
using DORA.Access.Context.Entities;
using DORA.Access.Common;
using Microsoft.Extensions.Configuration;

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

        public override Role FindOneBy(Expression<Func<Role, bool>> criteria)
        {
            User currentUser = this.CurrentUser();

            if (currentUser != null)
                return (from j in dbContext.UserRoles
                        join r in dbContext.Roles
                        on j.RoleId equals r.Id
                        where j.UserId == currentUser.Id
                        select r).FirstOrDefault(criteria);
            else
                return null;
        }

        public override IQueryable<Role> FindBy(Expression<Func<Role, bool>> criteria)
        {
            IQueryable<Role> query = FindAll();

            return base.FindBy(query, criteria);
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

        public override Role[] Update(Role[] current, Role[] previous)
        {
            if (current.Length != previous.Length)
                return null;

            // filter & sort out entities that the user doe not have access to
            current = (
                from e in this.FindAll().ToList()
                join c in current on e.Id equals c.Id.Value
                select c
            ).OrderBy(c => c.Id).ToArray();

            // filter and sort
            previous = (
                from c in current
                join p in previous on c.Id.Value equals p.Id.Value
                select p
            ).OrderBy(c => c.Id).ToArray();


            for (int e = 0; e < current.Length; e++)
            {
                current[e].Label = previous[e].Label;
                current[e].NameCanonical = previous[e].NameCanonical;
            }

            dbContext.Roles.AttachRange(current);
            dbContext.SaveChanges();

            return current;
        }

        public override Role[] SaveChanges(Role[] entity)
        {
            entity = (
                from e in this.FindAll().ToList()
                join p in entity on e.Id equals p.Id.Value
                select p
            ).ToArray();

            dbContext.Roles.AttachRange(entity);
            dbContext.SaveChanges();

            return entity;
        }

        public override Role[] Delete(Role[] entity)
        {
            entity = (
                from e in this.FindAll().ToList()
                join p in entity on e.Id equals p.Id.Value
                select p
            ).ToArray();

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