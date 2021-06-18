using System;
using System.Linq;
using DORA.Access.Context.Entities;
using DORA.Access.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;

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

        public override IQueryable<RoleResourceAccess> FindAllWithIncludes(string[] collectionNames)
        {
            IQueryable<RoleResourceAccess> query = this.FindAll();

            foreach(string collectionName in collectionNames)
                query = query.Include(collectionName);
                
            return query;
        }

        public override RoleResourceAccess CopyEntity(RoleResourceAccess current, RoleResourceAccess update)
        {
            current.RoleId = update.RoleId;
            current.ResourceId = update.ResourceId;

            return current;
        }

        public override RoleResourceAccess[] JoinAllAndSort(RoleResourceAccess[] entities)
        {
            return (
                from e in this.FindAll().ToList()
                join c in entities on e.Id equals c.Id
                select c
            ).OrderBy(c => c.Id).ToArray();
        }

        public override RoleResourceAccess[] Delete(RoleResourceAccess[] entity)
        {
            entity = this.JoinAllAndSort(entity);

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