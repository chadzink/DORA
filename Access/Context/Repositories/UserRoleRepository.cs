using System;
using System.Linq;
using System.Linq.Expressions;
using DORA.Access.Context.Entities;
using DORA.Access.Common;
using Microsoft.Extensions.Configuration;

namespace DORA.Access.Context.Repositories
{
    public interface IUserRoleRepository : IRepository<UserRole>
    {
        IQueryable<Role> GetRolesForUser(User user);
    }

    public class UserRoleRepository : Repository<AccessContext, UserRole>, IUserRoleRepository
    {
        public UserRoleRepository(AccessContext context, IConfiguration config)
            : base(context, config)
        {
        }

        public IQueryable<Role> GetRolesForUser(User user)
        {
            return (from ur in dbContext.UserRoles
                    join r in dbContext.Roles on ur.RoleId equals r.Id
                    where ur.UserId == user.Id
                    select r);
        }

        public override IQueryable<UserRole> FindAll()
        {
            return from s in dbContext.UserRoles
                   where s.ArchivedStamp == null
                   select s;
        }

        public override UserRole Find(Guid id)
        {
            return this.FindOneBy(e => e.Id == id);
        }

        public override UserRole FindOneBy(Expression<Func<UserRole, bool>> criteria)
        {
            return dbContext.UserRoles.FirstOrDefault(criteria);
        }

        public override IQueryable<UserRole> FindBy(Expression<Func<UserRole, bool>> criteria)
        {
            IQueryable<UserRole> query = FindAll();

            return base.FindBy(query, criteria);
        }

        public override UserRole[] Create(UserRole[] entity)
        {
            foreach (UserRole e in entity)
            {
                if (!e.Id.HasValue)
                    e.Id = Guid.NewGuid();
            }

            dbContext.UserRoles.AddRange(entity);
            dbContext.SaveChanges();

            return entity;
        }

        public override UserRole[] Update(UserRole[] current, UserRole[] previous)
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
                current[e].RoleId = previous[e].RoleId;
                current[e].UserId = previous[e].UserId;
            }

            dbContext.UserRoles.AttachRange(current);
            dbContext.SaveChanges();

            return current;
        }

        public override UserRole[] SaveChanges(UserRole[] entity)
        {
            entity = (
                from e in this.FindAll().ToList()
                join p in entity on e.Id equals p.Id.Value
                select p
            ).ToArray();

            dbContext.UserRoles.AttachRange(entity);
            dbContext.SaveChanges();

            return entity;
        }

        public override UserRole[] Delete(UserRole[] entity)
        {
            entity = (
                from e in this.FindAll().ToList()
                join p in entity on e.Id equals p.Id.Value
                select p
            ).ToArray();

            foreach (UserRole dbEntity in entity)
            {
                dbEntity.ArchivedStamp = DateTime.Now;
            }

            dbContext.UserRoles.AttachRange(entity);
            dbContext.SaveChanges();

            return entity;
        }

        public override UserRole[] Restore(Guid[] id)
        {
            UserRole[] entity = (
                from e in dbContext.UserRoles
                where id.Contains(e.Id.Value)
                select e
            ).ToArray();

            foreach (UserRole e in entity)
            {
                e.ArchivedStamp = null;
            }

            dbContext.SaveChanges();

            return entity;
        }
    }
}