using System;
using System.Linq;
using System.Linq.Expressions;
using DORA.DotAPI.Context.Entities;
using DORA.DotAPI.Common;
using Microsoft.Extensions.Configuration;

namespace DORA.DotAPI.Context.Repositories
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

        public override IQueryable<UserRole> FindBy(Func<UserRole, bool>[] criteria)
        {
            IQueryable<UserRole> query = FindAll();

            return base.FindBy(query, criteria);
        }

        public override UserRole Create(UserRole entity)
        {
            if (!entity.Id.HasValue)
                entity.Id = Guid.NewGuid();

            dbContext.UserRoles.Add(entity);
            dbContext.SaveChanges();

            return entity;
        }

        public override UserRole Update(UserRole current, UserRole previous)
        {
            bool hasAccess = (from s in this.FindAll() where s.Id == previous.Id select s).FirstOrDefault() != null;

            if (hasAccess)
            {
                current.RoleId = previous.RoleId;
                current.UserId = previous.UserId;

                dbContext.SaveChanges();
            }

            return current;
        }

        public override UserRole SaveChanges(UserRole entity)
        {
            if (entity.Id.HasValue)
            {
                bool hasAccess = (from s in this.FindAll() where s.Id == entity.Id select s).FirstOrDefault() != null;

                if (hasAccess)
                {
                    dbContext.UserRoles.Attach(entity);
                    dbContext.SaveChanges();

                    return entity;
                }
            }

            return null;
        }

        public override UserRole Delete(UserRole entity)
        {
            UserRole dbEntity = this.Find(entity.Id.Value);

            if (dbEntity != null)
            {
                dbEntity.ArchivedStamp = DateTime.Now;
                dbContext.SaveChanges();

                return dbEntity;
            }

            return entity;
        }

        public override UserRole Restore(Guid id)
        {
            UserRole entity = (from s in dbContext.UserRoles where s.Id == id select s).First();

            if (entity != null)
            {
                entity.ArchivedStamp = null;
                dbContext.SaveChanges();
            }

            return entity;
        }
    }
}