using System;
using System.Linq;
using DORA.Access.Context.Entities;
using DORA.Access.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;

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

        public override IQueryable<ResourceAccess> FindAllWithIncludes(string[] collectionNames)
        {
            IQueryable<ResourceAccess> query = this.FindAll();

            foreach(string collectionName in collectionNames)
                query = query.Include(collectionName);
                
            return query;
        }

        public override ResourceAccess CopyEntity(ResourceAccess current, ResourceAccess update)
        {
            current.ResourceId = update.ResourceId;
            current.KeyCode = update.KeyCode;

            return current;
        }

        public override ResourceAccess[] JoinAllAndSort(ResourceAccess[] entities)
        {
            return (
                from e in this.FindAll().ToList()
                join c in entities on e.Id equals c.Id
                select c
            ).OrderBy(c => c.Id).ToArray();
        }

        public override ResourceAccess[] Delete(ResourceAccess[] entity)
        {
            entity = this.JoinAllAndSort(entity);

            foreach (ResourceAccess dbEntity in entity)
            {
                dbEntity.ArchivedStamp = DateTime.Now;
            }
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