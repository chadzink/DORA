using System;
using System.Linq;
using System.Linq.Expressions;
using DORA.DotAPI.Context.Entities;
using DORA.DotAPI.Common;
using Microsoft.Extensions.Configuration;

namespace DORA.DotAPI.Context.Repositories
{
    public interface IRoleResourceAccessRepository : IRepository<RoleResourceAccess>
    {
        IQueryable<Resource> GetResourceAccessesForRole(Role role);
    }

    public class RoleResourceAccessRepository : Repository<AccessContext, RoleResourceAccess>, IRoleResourceAccessRepository
    {
        public RoleResourceAccessRepository(AccessContext context, IConfiguration config)
            : base(context, config)
        {
        }

        public IQueryable<Resource> GetResourceAccessesForRole(Role role)
        {
            return (from rra in dbContext.RoleResourcesAccesses
                    join r in dbContext.Resources on rra.ResourceId equals r.Id
                    where rra.RoleId == role.Id
                    select r);
        }

        public override IQueryable<RoleResourceAccess> FindAll()
        {
            return from s in dbContext.RoleResourcesAccesses
                   where s.ArchivedStamp == null
                   select s;
        }

        public override RoleResourceAccess Find(Guid id)
        {
            return this.FindOneBy(e => e.Id == id);
        }

        public override RoleResourceAccess FindOneBy(Expression<Func<RoleResourceAccess, bool>> criteria)
        {
            return dbContext.RoleResourcesAccesses.FirstOrDefault(criteria);
        }

        public override IQueryable<RoleResourceAccess> FindBy(Func<RoleResourceAccess, bool>[] criteria)
        {
            IQueryable<RoleResourceAccess> query = FindAll();

            return base.FindBy(query, criteria);
        }

        public override RoleResourceAccess Create(RoleResourceAccess entity)
        {
            if (!entity.Id.HasValue)
                entity.Id = Guid.NewGuid();

            dbContext.RoleResourcesAccesses.Add(entity);
            dbContext.SaveChanges();

            return entity;
        }

        public override RoleResourceAccess Update(RoleResourceAccess current, RoleResourceAccess previous)
        {
            bool hasAccess = (from s in this.FindAll() where s.Id == previous.Id select s).FirstOrDefault() != null;

            if (hasAccess)
            {
                current.RoleId = previous.RoleId;
                current.ResourceId = previous.ResourceId;
                dbContext.SaveChanges();
            }

            return current;
        }

        public override RoleResourceAccess SaveChanges(RoleResourceAccess entity)
        {
            if (entity.Id.HasValue)
            {
                bool hasAccess = (from s in this.FindAll() where s.Id == entity.Id select s).FirstOrDefault() != null;

                if (hasAccess)
                {
                    dbContext.RoleResourcesAccesses.Attach(entity);
                    dbContext.SaveChanges();

                    return entity;
                }
            }

            return null;
        }

        public override RoleResourceAccess Delete(RoleResourceAccess entity)
        {
            RoleResourceAccess dbEntity = this.Find(entity.Id.Value);

            if (dbEntity != null)
            {
                dbEntity.ArchivedStamp = DateTime.Now;
                dbContext.SaveChanges();

                return dbEntity;
            }

            return entity;
        }

        public override RoleResourceAccess Restore(Guid id)
        {
            RoleResourceAccess entity = (from s in dbContext.RoleResourcesAccesses where s.Id == id select s).First();

            if (entity != null)
            {
                entity.ArchivedStamp = null;
                dbContext.SaveChanges();
            }

            return entity;
        }
    }
}