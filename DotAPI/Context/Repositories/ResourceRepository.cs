using System;
using System.Linq;
using System.Linq.Expressions;
using DORA.DotAPI.Context.Entities;
using Microsoft.Extensions.Configuration;

namespace DORA.DotAPI.Context.Repositories
{
    public class ResourceRepository : Repository<AccessContext, Resource>
    {
        private string[] DEFAULT_RESOURCE_ACCESS_KEYCODES = new string[] {
            "CREATE",
            "READ",
            "UPDATE",
            "DELETE"
        };

        private string ADMIN_ROLE_NAME_CANONICAL = "ADMIN";

        public ResourceRepository(AccessContext context, IConfiguration configuration)
            : base(context, context, configuration)
        {
        }

        public override IQueryable<Resource> FindAll()
        {
            return from s in dbContext.Resources
                   where s.ArchivedStamp == null
                   select s;
        }

        public override Resource Find(Guid id)
        {
            return this.FindOneBy(e => e.Id == id);
        }

        public override Resource FindOneBy(Expression<Func<Resource, bool>> criteria)
        {
            return dbContext.Resources.FirstOrDefault(criteria);
        }

        public override IQueryable<Resource> FindBy(Func<Resource, bool>[] criteria)
        {
            IQueryable<Resource> query = FindAll();

            return base.FindBy(query, criteria);
        }

        public override Resource Create(Resource entity)
        {
            if (!entity.Id.HasValue)
                entity.Id = Guid.NewGuid();

            dbContext.Resources.Add(entity);

            // add the default Resource Access records
            foreach(string keyCode in this.DEFAULT_RESOURCE_ACCESS_KEYCODES)
            {
                ResourceAccess newRA = new ResourceAccess() {
                    Id = Guid.NewGuid(),
                    ResourceId = entity.Id.Value,
                    KeyCode = keyCode,
                };

                dbContext.ResourceAccesses.Add(newRA);

                // add the new resource to the admin role (check if exist too)
                Role adminRole = dbContext.Roles.Where(r => r.NameCanonical == ADMIN_ROLE_NAME_CANONICAL).FirstOrDefault();

                if (adminRole != null)
                {
                    RoleResourceAccess newRRA = new RoleResourceAccess() {
                        Id = Guid.NewGuid(),
                        RoleId = adminRole.Id.Value,
                        ResourceId = entity.Id.Value,
                        ResourceAccessId = newRA.Id.Value,
                    };

                    dbContext.RoleResourcesAccesses.Add(newRRA);
                }
            }

            dbContext.SaveChanges();

            return entity;
        }

        public override Resource Update(Resource current, Resource entity)
        {
            current.KeyCode = entity.KeyCode;

            dbContext.SaveChanges();

            return current;
        }

        public override Resource Delete(Resource entity)
        {
            Resource dbEntity = this.Find(entity.Id.Value);

            if (dbEntity != null)
            {
                dbEntity.ArchivedStamp = DateTime.Now;
                dbContext.SaveChanges();

                // achive the ResourceAccess & RoleResourceAccess entities linked to this resource
                foreach(ResourceAccess dbRA in dbContext.ResourceAccesses.Where(ra => ra.ResourceId == dbEntity.Id.Value))
                {
                    dbRA.ArchivedStamp = DateTime.Now;
                    dbContext.SaveChanges();

                    foreach(RoleResourceAccess dbRRA in dbContext.RoleResourcesAccesses.Where(rra => rra.ResourceAccessId == dbRA.Id.Value))
                    {
                        dbRRA.ArchivedStamp = DateTime.Now;
                        dbContext.SaveChanges();
                    }
                }
                
            }

            return dbEntity;
        }

        public override Resource Restore(Guid id)
        {
            Resource entity = (from s in dbContext.Resources where s.Id == id select s).First();


            if (entity != null)
            {
                entity.ArchivedStamp = null;
                dbContext.SaveChanges();

                // remove achive the ResourceAccess & RoleResourceAccess entities linked to this resource
                foreach (ResourceAccess dbRA in dbContext.ResourceAccesses.Where(ra => ra.ResourceId == entity.Id.Value))
                {
                    dbRA.ArchivedStamp = null;
                    dbContext.SaveChanges();

                    foreach (RoleResourceAccess dbRRA in dbContext.RoleResourcesAccesses.Where(rra => rra.ResourceAccessId == dbRA.Id.Value))
                    {
                        dbRRA.ArchivedStamp = null;
                        dbContext.SaveChanges();
                    }
                }
            }

            return entity;
        }
    }
}