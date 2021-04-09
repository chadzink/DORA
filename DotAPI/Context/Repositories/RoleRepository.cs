using System;
using System.Linq;
using System.Linq.Expressions;
using DORA.DotAPI.Context.Entities;
using Microsoft.Extensions.Configuration;
using DORA.DotAPI.Common;

namespace DORA.DotAPI.Context.Repositories
{
    public class RoleRepository : Repository<AccessContext, Role>
    {
        public RoleRepository(AccessContext context, IConfiguration configuration) : base(context, configuration)
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

        public override IQueryable<Role> FindBy(Func<Role, bool>[] criteria)
        {
            IQueryable<Role> query = FindAll();

            return base.FindBy(query, criteria);
        }

        public override Role Create(Role entity)
        {
            if (!entity.Id.HasValue)
                entity.Id = Guid.NewGuid();

            dbContext.Roles.Add(entity);

            // Add site to current user, if exist
            User currentUser = this.CurrentUser();

            if (currentUser != null)
            {
                dbContext.UserRoles.Add(new UserRole
                {
                    Id = Guid.NewGuid(),
                    UserId = currentUser.Id.Value,
                    RoleId = entity.Id.Value,
                });
            }

            dbContext.SaveChanges();

            return entity;
        }

        public override Role Update(Role current, Role entity)
        {
            bool hasAccess = (from s in this.FindAll()
                              where s.Id == entity.Id
                              select s).FirstOrDefault() != null;

            if (hasAccess)
            {
                current.Label = entity.Label;
                current.NameCanonical = entity.NameCanonical;
                dbContext.SaveChanges();
            }

            return current;
        }

        public override Role Delete(Role entity)
        {
            bool hasAccess = (from s in this.FindAll()
                              where s.Id == entity.Id
                              select s).FirstOrDefault() != null;

            if (hasAccess)
            {
                Role dbEntity = this.Find(entity.Id.Value);

                if (dbEntity != null)
                {
                    dbEntity.ArchivedStamp = DateTime.Now;
                    dbContext.SaveChanges();
                }

                return dbEntity;
            }

            return entity;
        }

        public override Role Restore(Guid id)
        {
            Role entity = (from s in dbContext.Roles where s.Id == id select s).First();

            if (entity != null)
            {
                entity.ArchivedStamp = null;
                dbContext.SaveChanges();
            }

            return entity;
        }
    }
}