
using System;
using System.Collections.Generic;
using System.Linq;
using DORA.Access.Context;
using DORA.Access.Context.Entities;
using DORA.Access.Helpers;

namespace DORA.Navigation.Migrations
{
    public static class SeedInitialData
    {
        // Define Default data to load
        static Guid NAV_GROUP_RESOURCE_ID = Guid.NewGuid();
        static Guid NAV_ITEM_RESOURCE_ID = Guid.NewGuid();

        static Resource[] NEW_RESOURCES = new Resource[] {
            new Resource { Id = NAV_GROUP_RESOURCE_ID, KeyCode = "NAV-GROUP", SqlObjectName = "navigation_group"},
            new Resource { Id = NAV_ITEM_RESOURCE_ID, KeyCode = "NAV-ITEM", SqlObjectName = "navigation_item"},
        };

        static IncludedResource[] NEW_INCLUDED_RESOURCES = new IncludedResource[] {
            // NAV-GROUP - NavItems
            new IncludedResource { 
                Id = Guid.NewGuid(),
                ResourceId = NAV_GROUP_RESOURCE_ID,
                IncludedRecourceId = NAV_ITEM_RESOURCE_ID,
                CollectionName = "NavItems",
                Description = "Navigation Items in the Navigation Group"
             },
             // NAV-ITEM - NavGroup, ParentNavItem, ChildNavItems[]
            new IncludedResource { 
                Id = Guid.NewGuid(),
                ResourceId = NAV_ITEM_RESOURCE_ID,
                IncludedRecourceId = NAV_GROUP_RESOURCE_ID,
                CollectionName = "NavGroup",
                Description = "The Navigation group that this item exist in"
             },
            new IncludedResource {
                Id = Guid.NewGuid(),
                ResourceId = NAV_ITEM_RESOURCE_ID,
                IncludedRecourceId = NAV_ITEM_RESOURCE_ID,
                CollectionName = "ChildNavItems",
                Description = "The child navigations item for this item"
             },
        };

        // Default role keys
        static string ADMIN_ROLE_KEY = "ADMIN";
        static string USER_ROLE_KEY = "USER";

        public static void Up()
        {
            using (AccessContext dbContext = AccessContext.CreateContext())
            {
                // there is a already a nav group resource then this has already been executed
                if ((from u in dbContext.Resources where u.KeyCode == "NAV-GROUP" select u.Id.Value).Count() > 0)
                    return;

                // Add the Access Module Resources (does not include JwtRefreshTokens because it is a hiden system resource)
                string[] DefaultResourceAccessKeyCodes = new string[] {
                    "CREATE", "READ", "UPDATE", "DELETE"
                };

                Role adminRole = dbContext.Roles.Where(u => u.KeyCode == ADMIN_ROLE_KEY).FirstOrDefault();
                Role userRole = dbContext.Roles.Where(u => u.KeyCode == USER_ROLE_KEY).FirstOrDefault();

                Role[] DEFAULT_ROLES = new Role[] { adminRole, userRole };

                foreach (Resource defaultResource in NEW_RESOURCES)
                {
                    dbContext.Resources.Add(defaultResource);

                    // Add the resource Acces for each resource added
                    foreach(string accessKey in DefaultResourceAccessKeyCodes)
                    {
                        ResourceAccess access = new ResourceAccess() {
                            Id = Guid.NewGuid(),
                            ResourceId = defaultResource.Id.Value,
                            KeyCode = accessKey
                        };

                        dbContext.ResourceAccesses.Add(access);

                        foreach(Role role in DEFAULT_ROLES)
                        {
                            if (role.KeyCode == "ADMIN")
                                dbContext.RoleResourcesAccesses.Add(new RoleResourceAccess
                                {
                                    Id = Guid.NewGuid(),
                                    RoleId = role.Id.Value,
                                    ResourceId = defaultResource.Id.Value,
                                    ResourceAccessId = access.Id.Value
                                });
                            else if (accessKey == "READ")
                                dbContext.RoleResourcesAccesses.Add(new RoleResourceAccess
                                {
                                    Id = Guid.NewGuid(),
                                    RoleId = role.Id.Value,
                                    ResourceId = defaultResource.Id.Value,
                                    ResourceAccessId = access.Id.Value
                                });
                        }
                    }
                }
                
                dbContext.IncludedResources.AddRange(NEW_INCLUDED_RESOURCES);

                // SAVE ALL TO DB
                dbContext.SaveChanges();
            }  
        }

        public static void Down()
        {
            using (AccessContext dbContext = AccessContext.CreateContext())
            {
                // there is no nav group resource then data does not need to be removed
                if ((from u in dbContext.Resources where u.KeyCode == "NAV-GROUP" select u.Id.Value).Count() == 0)
                    return;

                // remove the default data created

                // remove all the role access to the resources defined even if other migrations or processed add different role for the resources
                string[] DefaultResourceKeys = NEW_RESOURCES.Select(r => r.KeyCode).ToArray();

                dbContext.RoleResourcesAccesses.RemoveRange(
                    from rra in dbContext.RoleResourcesAccesses
                    join ra in dbContext.ResourceAccesses
                        on rra.ResourceAccessId equals ra.Id.Value
                    join r in dbContext.Resources
                        on ra.ResourceId equals r.Id.Value
                    where
                        DefaultResourceKeys.Contains(r.KeyCode)
                    select rra
                );

                dbContext.ResourceAccesses.RemoveRange(
                    from ra in dbContext.ResourceAccesses
                    join r in dbContext.Resources
                        on ra.ResourceId equals r.Id.Value
                    where
                        DefaultResourceKeys.Contains(r.KeyCode)
                    select ra
                );

                dbContext.IncludedResources.RemoveRange(
                    from ir in dbContext.IncludedResources
                    join r in dbContext.Resources
                        on ir.ResourceId equals r.Id.Value
                    where
                        DefaultResourceKeys.Contains(r.KeyCode)
                    select ir
                );

                // SAVE ALL TO DB
                dbContext.SaveChanges();
            }
        }
    }
}
