using System;
using System.Linq;
using System.Linq.Expressions;
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

        public override IQueryable<IncludedResource> FindAllWithIncludes(string[] collectionNames)
        {
            IQueryable<IncludedResource> query = this.FindAll();

            foreach(string collectionName in collectionNames)
                query = query.Include(collectionName);
                
            return query;
        }

        public override IncludedResource Find(Guid id)
        {
            return this.FindOneBy(e => e.Id == id);
        }

        public override IncludedResource FindOneBy(Expression<Func<IncludedResource, bool>> criteria)
        {
            return dbContext.IncludedResources.FirstOrDefault(criteria);
        }

        public override IQueryable<IncludedResource> FindBy(Expression<Func<IncludedResource, bool>> criteria)
        {
            IQueryable<IncludedResource> query = FindAll();

            return base.FindBy(query, criteria);
        }

        public override IncludedResource[] Create(IncludedResource[] entity)
        {
            foreach (IncludedResource e in entity)
            {
                if (!e.Id.HasValue)
                    e.Id = Guid.NewGuid();
            }

            dbContext.IncludedResources.AddRange(entity);
            dbContext.SaveChanges();

            return entity;
        }

        public override IncludedResource[] Update(IncludedResource[] current, IncludedResource[] previous)
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
                current[e].IncludedRecourceId = previous[e].IncludedRecourceId;
                current[e].CollectionName = previous[e].CollectionName;
                current[e].Description = previous[e].Description;
            }

            dbContext.IncludedResources.AttachRange(current);
            dbContext.SaveChanges();

            return current;
        }

        public override IncludedResource[] SaveChanges(IncludedResource[] entity)
        {
            entity = (
                from e in this.FindAll().ToList()
                join p in entity on e.Id equals p.Id.Value
                select p
            ).ToArray();

            dbContext.IncludedResources.AttachRange(entity);
            dbContext.SaveChanges();

            return entity;
        }

        public override IncludedResource[] Delete(IncludedResource[] entity)
        {
            entity = (
                from e in this.FindAll().ToList()
                join p in entity on e.Id equals p.Id.Value
                select p
            ).ToArray();

            dbContext.IncludedResources.RemoveRange(entity);
            dbContext.SaveChanges();

            return entity;
        }

        public override IncludedResource[] Restore(Guid[] id)
        {
            return null;
        }
    }
}