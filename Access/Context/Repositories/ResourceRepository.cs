using System;
using System.Linq;
using System.Linq.Expressions;
using DORA.Access.Context.Entities;
using DORA.Access.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;

namespace DORA.Access.Context.Repositories
{
    public class ResourceRepository : Repository<AccessContext, Resource>
    {
        public string[] DEFAULT_RESOURCE_ACCESS_KEYCODES = new string[] {
            "CREATE",
            "READ",
            "UPDATE",
            "DELETE"
        };

        private string ADMIN_ROLE_NAME_CANONICAL = "ADMIN";

        public ResourceRepository(AccessContext context, IConfiguration config)
            : base(context, config)
        {
        }

        public override IQueryable<Resource> FindAll()
        {
            return from s in dbContext.Resources
                   where s.ArchivedStamp == null
                   select s;
        }

        public override IQueryable<Resource> FindAllWithIncludes(string[] collectionNames)
        {
            IQueryable<Resource> query = this.FindAll();

            foreach(string collectionName in collectionNames)
                query = query.Include(collectionName);
                
            return query;
        }

        public override Resource Find(Guid id)
        {
            return this.FindOneBy(e => e.Id == id);
        }

        public override Resource FindOneBy(Expression<Func<Resource, bool>> criteria)
        {
            return dbContext.Resources.FirstOrDefault(criteria);
        }

        public override IQueryable<Resource> FindBy(Expression<Func<Resource, bool>> criteria)
        {
            IQueryable<Resource> query = FindAll();

            return base.FindBy(query, criteria);
        }

        public override Resource[] Create(Resource[] entity)
        {

            foreach (Resource e in entity)
            {
                if (!e.Id.HasValue)
                    e.Id = Guid.NewGuid();
            }

            dbContext.Resources.AddRange(entity);
            dbContext.SaveChanges();

            // add the default Resource Access records
            foreach (Resource e in entity)
            {
                foreach (string keyCode in this.DEFAULT_RESOURCE_ACCESS_KEYCODES)
                {
                    ResourceAccess newRA = new ResourceAccess()
                    {
                        Id = Guid.NewGuid(),
                        ResourceId = e.Id.Value,
                        KeyCode = keyCode,
                    };

                    dbContext.ResourceAccesses.Add(newRA);

                    // add the new resource to the admin role (check if exist too)
                    Role adminRole = dbContext.Roles.Where(r => r.KeyCode == ADMIN_ROLE_NAME_CANONICAL).FirstOrDefault();

                    if (adminRole != null)
                    {
                        RoleResourceAccess newRRA = new RoleResourceAccess()
                        {
                            Id = Guid.NewGuid(),
                            RoleId = adminRole.Id.Value,
                            ResourceId = e.Id.Value,
                            ResourceAccessId = newRA.Id.Value,
                        };

                        dbContext.RoleResourcesAccesses.Add(newRRA);
                    }
                }
            }

            dbContext.SaveChanges();

            return entity;
        }

        public bool AddResourceToRoleNames(
            Resource entity,
            string[] RoleNamesCanonical,
            string[] WithAccessKeys
        )
        {
            IQueryable<ResourceAccess> accessResources = dbContext.ResourceAccesses
                .Where(r => WithAccessKeys.Contains(r.KeyCode));

            foreach(ResourceAccess resourceAccess in accessResources)
            {
                IQueryable<Role> addRoles = dbContext.Roles
                    .Where(r => RoleNamesCanonical.Contains(r.KeyCode));

                foreach(Role role in addRoles)
                {
                    RoleResourceAccess newRRA = new RoleResourceAccess() {
                        Id = Guid.NewGuid(),
                        RoleId = role.Id.Value,
                        ResourceId = entity.Id.Value,
                        ResourceAccessId = resourceAccess.Id.Value,
                    };

                    dbContext.RoleResourcesAccesses.Add(newRRA);
                }
            }
            return true;
        }

        public override Resource[] Update(Resource[] current, Resource[] previous)
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
                current[e].KeyCode = previous[e].KeyCode;
            }

            dbContext.Resources.AttachRange(current);
            dbContext.SaveChanges();

            return current;
        }

        public override Resource[] SaveChanges(Resource[] entity)
        {
            entity = (
                from e in this.FindAll().ToList()
                join p in entity on e.Id equals p.Id.Value
                select p
            ).ToArray();

            dbContext.Resources.AttachRange(entity);
            dbContext.SaveChanges();

            return entity;
        }

        public override Resource[] Delete(Resource[] entity)
        {
            entity = (
                from e in this.FindAll().ToList()
                join p in entity on e.Id equals p.Id.Value
                select p
            ).ToArray();

            foreach (Resource dbEntity in entity)
            {
                dbEntity.ArchivedStamp = DateTime.Now;

                // achive the ResourceAccess & RoleResourceAccess entities linked to this resource
                foreach (ResourceAccess dbRA in dbContext.ResourceAccesses.Where(ra => ra.ResourceId == dbEntity.Id.Value))
                {
                    dbRA.ArchivedStamp = DateTime.Now;

                    foreach (RoleResourceAccess dbRRA in dbContext.RoleResourcesAccesses.Where(rra => rra.ResourceAccessId == dbRA.Id.Value))
                    {
                        dbRRA.ArchivedStamp = DateTime.Now;
                    }
                }
            }

            dbContext.Resources.AttachRange(entity);
            dbContext.SaveChanges();

            return entity;
        }

        public override Resource[] Restore(Guid[] id)
        {
            Resource[] entity = (
                from e in dbContext.Resources
                where id.Contains(e.Id.Value)
                select e
            ).ToArray();

            foreach (Resource e in entity)
            {
                e.ArchivedStamp = null;

                // remove achive the ResourceAccess & RoleResourceAccess entities linked to this resource
                foreach (ResourceAccess dbRA in dbContext.ResourceAccesses.Where(ra => ra.ResourceId == e.Id.Value))
                {
                    dbRA.ArchivedStamp = null;

                    foreach (RoleResourceAccess dbRRA in dbContext.RoleResourcesAccesses.Where(rra => rra.ResourceAccessId == dbRA.Id.Value))
                    {
                        dbRRA.ArchivedStamp = null;
                    }
                }
            }

            dbContext.SaveChanges();

            return entity;
        }
    }
}