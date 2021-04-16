using System;
using System.Linq;
using System.Linq.Expressions;
using DORA.Access.Context.Entities;
using DORA.Access.Common;
using Microsoft.Extensions.Configuration;

namespace DORA.Access.Context.Repositories
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

        public override IQueryable<RoleResourceAccess> FindBy(Expression<Func<RoleResourceAccess, bool>> criteria)
        {
            IQueryable<RoleResourceAccess> query = FindAll();

            return base.FindBy(query, criteria);
        }

        public override RoleResourceAccess[] Create(RoleResourceAccess[] entity)
        {
            foreach (RoleResourceAccess e in entity)
            {
                if (!e.Id.HasValue)
                    e.Id = Guid.NewGuid();
            }

            dbContext.RoleResourcesAccesses.AddRange(entity);
            dbContext.SaveChanges();

            return entity;
        }

        public override RoleResourceAccess[] Update(RoleResourceAccess[] current, RoleResourceAccess[] previous)
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
                current[e].ResourceId = previous[e].ResourceId;
            }

            dbContext.RoleResourcesAccesses.AttachRange(current);
            dbContext.SaveChanges();

            return current;
        }

        public override RoleResourceAccess[] SaveChanges(RoleResourceAccess[] entity)
        {
            entity = (
                from e in this.FindAll().ToList()
                join p in entity on e.Id equals p.Id.Value
                select p
            ).ToArray();

            dbContext.RoleResourcesAccesses.AttachRange(entity);
            dbContext.SaveChanges();

            return entity;
        }

        public override RoleResourceAccess[] Delete(RoleResourceAccess[] entity)
        {
            entity = (
                from e in this.FindAll().ToList()
                join p in entity on e.Id equals p.Id.Value
                select p
            ).ToArray();

            foreach (RoleResourceAccess dbEntity in entity)
            {
                dbEntity.ArchivedStamp = DateTime.Now;
            }

            dbContext.RoleResourcesAccesses.AttachRange(entity);
            dbContext.SaveChanges();

            return entity;
        }

        public override RoleResourceAccess[] Restore(Guid[] id)
        {
            RoleResourceAccess[] entity = (
                from e in dbContext.RoleResourcesAccesses
                where id.Contains(e.Id.Value)
                select e
            ).ToArray();

            foreach (RoleResourceAccess e in entity)
            {
                e.ArchivedStamp = null;
            }

            dbContext.SaveChanges();

            return entity;
        }
    }
}