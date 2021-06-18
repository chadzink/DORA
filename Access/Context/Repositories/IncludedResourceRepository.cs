using System;
using System.Linq;
using DORA.Access.Context.Entities;
using DORA.Access.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;

namespace DORA.Access.Context.Repositories
{
    public class IncludedResourceRepository : Repository<AccessContext, IncludedResource>
    {
        public IncludedResourceRepository(AccessContext context, IConfiguration config)
            : base(context, config)
        {
        }

        public override IQueryable<IncludedResource> FindAll()
        {
            return from s in dbContext.IncludedResources select s;
        }

        public override IncludedResource Find(Guid id)
        {
            return this.FindOneBy(e => e.Id == id);
        }

        public override IQueryable<IncludedResource> FindAllWithIncludes(string[] collectionNames)
        {
            IQueryable<IncludedResource> query = this.FindAll();

            foreach(string collectionName in collectionNames)
                query = this.FindAll().Include(collectionName);
                
            return query;
        }

        public override IncludedResource CopyEntity(IncludedResource current, IncludedResource update)
        {
            current.ResourceId = update.ResourceId;
            current.IncludedRecourceId = update.IncludedRecourceId;
            current.CollectionName = update.CollectionName;
            current.Description = update.Description;

            return current;
        }

        public override IncludedResource[] JoinAllAndSort(IncludedResource[] entities)
        {
            return (
                from e in this.FindAll().ToList()
                join c in entities on e.Id equals c.Id
                select c
            ).OrderBy(c => c.Id).ToArray();
        }
    }
}