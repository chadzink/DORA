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

        public override IQueryable<ResourceAccess> FindBy(Func<ResourceAccess, bool>[] criteria)
        {
            IQueryable<ResourceAccess> query = FindAll();

            return base.FindBy(query, criteria);
        }

        public override ResourceAccess Create(ResourceAccess entity)
        {
            if (!entity.Id.HasValue)
                entity.Id = Guid.NewGuid();

            dbContext.ResourceAccesses.Add(entity);
            dbContext.SaveChanges();

            return entity;
        }

        public override ResourceAccess Update(ResourceAccess current, ResourceAccess previous)
        {
            bool hasAccess = (from s in this.FindAll() where s.Id == previous.Id select s).FirstOrDefault() != null;

            if (hasAccess)
            {
                current.ResourceId = previous.ResourceId;
                current.KeyCode = previous.KeyCode;

                dbContext.SaveChanges();
            }

            return current;
        }

        public override ResourceAccess SaveChanges(ResourceAccess entity)
        {
            if (entity.Id.HasValue)
            {
                bool hasAccess = (from s in this.FindAll() where s.Id == entity.Id select s).FirstOrDefault() != null;

                if (hasAccess)
                {
                    dbContext.ResourceAccesses.Attach(entity);
                    dbContext.SaveChanges();

                    return entity;
                }
            }

            return null;
        }

        public override ResourceAccess Delete(ResourceAccess entity)
        {
            ResourceAccess dbEntity = this.Find(entity.Id.Value);

            if (dbEntity != null)
            {
                dbEntity.ArchivedStamp = DateTime.Now;
                dbContext.SaveChanges();
            }

            return dbEntity;
        }

        public override ResourceAccess Restore(Guid id)
        {
            ResourceAccess entity = (from s in dbContext.ResourceAccesses where s.Id == id select s).First();


            if (entity != null)
            {
                entity.ArchivedStamp = null;
                dbContext.SaveChanges();
            }

            return entity;
        }
    }
}