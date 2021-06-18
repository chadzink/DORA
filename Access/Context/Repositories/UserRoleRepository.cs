using System;
using System.Linq;
using DORA.Access.Context.Entities;
using DORA.Access.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;

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

        public override IQueryable<UserRole> FindAllWithIncludes(string[] collectionNames)
        {
            IQueryable<UserRole> query = this.FindAll();

            foreach(string collectionName in collectionNames)
                query = query.Include(collectionName);
                
            return query;
        }

        public override UserRole CopyEntity(UserRole current, UserRole updates)
        {
            current.RoleId = updates.RoleId;
            current.UserId = updates.UserId;

            return current;
        }

        public override UserRole[] JoinAllAndSort(UserRole[] entities)
        {
            return (
                from e in this.FindAll().ToList()
                join c in entities on e.Id equals c.Id
                select c
            ).OrderBy(c => c.Id).ToArray();
        }

        public override UserRole[] Delete(UserRole[] entity)
        {
            entity = this.JoinAllAndSort(entity);

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