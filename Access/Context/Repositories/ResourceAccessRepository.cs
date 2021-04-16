using System;
using System.Linq;
using System.Linq.Expressions;
using DORA.Access.Context.Entities;
using DORA.Access.Common;
using Microsoft.Extensions.Configuration;

namespace DORA.Access.Context.Repositories
{
    public class ResourceAccessRepository : Repository<AccessContext, ResourceAccess>
    {
        public ResourceAccessRepository(AccessContext context, IConfiguration config)
            : base(context, config)
        {
        }

        public override IQueryable<ResourceAccess> FindAll()
        {
            return from s in dbContext.ResourceAccesses
                   where s.ArchivedStamp == null
                   select s;
        }

        public override ResourceAccess Find(Guid id)
        {
            return this.FindOneBy(e => e.Id == id);
        }

        public override ResourceAccess FindOneBy(Expression<Func<ResourceAccess, bool>> criteria)
        {
            return dbContext.ResourceAccesses.FirstOrDefault(criteria);
        }

        public override IQueryable<ResourceAccess> FindBy(Expression<Func<ResourceAccess, bool>> criteria)
        {
            IQueryable<ResourceAccess> query = FindAll();

            return base.FindBy(query, criteria);
        }

        public override ResourceAccess[] Create(ResourceAccess[] entity)
        {
            foreach (ResourceAccess e in entity)
            {
                if (!e.Id.HasValue)
                    e.Id = Guid.NewGuid();
            }

            dbContext.ResourceAccesses.AddRange(entity);
            dbContext.SaveChanges();

            return entity;
        }

        public override ResourceAccess[] Update(ResourceAccess[] current, ResourceAccess[] previous)
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
                current[e].ResourceId = previous[e].ResourceId;
                current[e].KeyCode = previous[e].KeyCode;    
            }

            dbContext.ResourceAccesses.AttachRange(current);
            dbContext.SaveChanges();

            return current;
        }

        public override ResourceAccess[] SaveChanges(ResourceAccess[] entity)
        {
            entity = (
                from e in this.FindAll().ToList()
                join p in entity on e.Id equals p.Id.Value
                select p
            ).ToArray();

            dbContext.ResourceAccesses.AttachRange(entity);
            dbContext.SaveChanges();

            return entity;
        }

        public override ResourceAccess[] Delete(ResourceAccess[] entity)
        {
            entity = (
                from e in this.FindAll().ToList()
                join p in entity on e.Id equals p.Id.Value
                select p
            ).ToArray();

            foreach (ResourceAccess dbEntity in entity)
            {
                dbEntity.ArchivedStamp = DateTime.Now;
            }

            dbContext.ResourceAccesses.AttachRange(entity);
            dbContext.SaveChanges();

            return entity;
        }

        public override ResourceAccess[] Restore(Guid[] id)
        {
            ResourceAccess[] entity = (
                from e in dbContext.ResourceAccesses
                where id.Contains(e.Id.Value)
                select e
            ).ToArray();

            foreach(ResourceAccess e in entity)
            {
                e.ArchivedStamp = null;
            }

            dbContext.SaveChanges();

            return entity;
        }
    }
}