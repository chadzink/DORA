using System;
using System.Linq;
using System.Linq.Expressions;
using DORA.DotAPI.Context.Entities;
using Microsoft.Extensions.Configuration;

namespace DORA.DotAPI.Context.Repositories
{
    public class ResourceAccessRepository : Repository<AccessContext, ResourceAccess>
    {
        public ResourceAccessRepository(AccessContext context, IConfiguration configuration)
            : base(context, context, configuration)
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

        public override ResourceAccess Update(ResourceAccess current, ResourceAccess entity)
        {
            current.ResourceId = entity.ResourceId;
            current.KeyCode = entity.KeyCode;

            dbContext.SaveChanges();

            return current;
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