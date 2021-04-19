
using System;
using System.Collections.Generic;
using System.Linq;
using DORA.Access.Context;
using DORA.Access.Context.Entities;
using DORA.Access.Helpers;

namespace Access.Migrations
{
    public static class SeedInitialData
    {
        // Define Default data to load
        static Guid USER_RESOURCE_ID = Guid.NewGuid();
        static Guid ROLE_RESOURCE_ID = Guid.NewGuid();
        static Guid USER_ROLE_RESOURCE_ID = Guid.NewGuid();
        static Guid RESOURCE_RESOURCE_ID = Guid.NewGuid();
        static Guid RESOURCE_ACCESS_RESOURCE_ID = Guid.NewGuid();
        static Guid ROLE_RESOURCE_ACCESS_RESOURCE_ID = Guid.NewGuid();
        static Guid INCLUDED_RESOURCE_RESOURCE_ID = Guid.NewGuid();

        static Resource[] DEFAULT_RESOURCES = new Resource[] {
            new Resource { Id = USER_RESOURCE_ID, KeyCode = "USER", SqlObjectName = "users"},
            new Resource { Id = ROLE_RESOURCE_ID, KeyCode = "ROLE", SqlObjectName = "roles"},
            new Resource { Id = USER_ROLE_RESOURCE_ID, KeyCode = "USER-ROLE", SqlObjectName = "user_roles"},
            new Resource { Id = RESOURCE_RESOURCE_ID, KeyCode = "RESOURCE", SqlObjectName = "resources"},
            new Resource { Id = RESOURCE_ACCESS_RESOURCE_ID, KeyCode = "RESOURCE-ACCESS", SqlObjectName = "resource_accesses"},
            new Resource { Id = ROLE_RESOURCE_ACCESS_RESOURCE_ID, KeyCode = "ROLE-RESOURCE-ACCESS", SqlObjectName = "role_resource_accesses"},
            new Resource { Id = INCLUDED_RESOURCE_RESOURCE_ID, KeyCode = "INCLUDED-RESOURCE", SqlObjectName = "included_resources"}
        };

        static IncludedResource[] DEFAULT_INCLUDED_RESOURCES = new IncludedResource[] {
            // USER - UserRoles
            new IncludedResource { 
                Id = Guid.NewGuid(),
                ResourceId = USER_RESOURCE_ID,
                IncludedRecourceId = USER_ROLE_RESOURCE_ID,
                CollectionName = "UserRoles.Role",
                Description = "Roles for User from UserRoles"
             },
             // ROLE - UserRoles, RoleResourcesAccess[]
            new IncludedResource { 
                Id = Guid.NewGuid(),
                ResourceId = ROLE_RESOURCE_ID,
                IncludedRecourceId = USER_RESOURCE_ID,
                CollectionName = "UserRoles.User",
                Description = "Users Assigned to Role from UserRoles"
             },
            new IncludedResource { 
                Id = Guid.NewGuid(),
                ResourceId = ROLE_RESOURCE_ID,
                IncludedRecourceId = ROLE_RESOURCE_ACCESS_RESOURCE_ID,
                CollectionName = "RoleResourcesAccess",
                Description = "Access Keys Assigned to Role for Resources"
             },
             //USER-ROLE - NONE
             //RESOURCE - ResourceAccesses, RoleResourceAccesses
            new IncludedResource { 
                Id = Guid.NewGuid(),
                ResourceId = RESOURCE_RESOURCE_ID,
                IncludedRecourceId = RESOURCE_ACCESS_RESOURCE_ID,
                CollectionName = "ResourceAccesses",
                Description = "Access Keys Assigned to Resource"
             },
            new IncludedResource { 
                Id = Guid.NewGuid(),
                ResourceId = RESOURCE_RESOURCE_ID,
                IncludedRecourceId = ROLE_RESOURCE_ACCESS_RESOURCE_ID,
                CollectionName = "RoleResourceAccesses",
                Description = "Roles With Access Keys to Resource"
             },
             //RESOURCE-ACCESS - Resources, RoleResourceAccesses
            new IncludedResource { 
                Id = Guid.NewGuid(),
                ResourceId = RESOURCE_ACCESS_RESOURCE_ID,
                IncludedRecourceId = RESOURCE_RESOURCE_ID,
                CollectionName = "Resources",
                Description = "Resources Assigned Access Key"
             },
            new IncludedResource { 
                Id = Guid.NewGuid(),
                ResourceId = RESOURCE_ACCESS_RESOURCE_ID,
                IncludedRecourceId = ROLE_RESOURCE_ACCESS_RESOURCE_ID,
                CollectionName = "RoleResourcesAccess",
                Description = "Roles and Resources Assigned Access Key"
             },
             //ROLE-RESOURCE-ACCESS - Roles, Resources, ResourceAccesses
            new IncludedResource { 
                Id = Guid.NewGuid(),
                ResourceId = ROLE_RESOURCE_ACCESS_RESOURCE_ID,
                IncludedRecourceId = ROLE_RESOURCE_ID,
                CollectionName = "Roles",
                Description = "Roles Assigned to Role Resource Access"
             },
            new IncludedResource { 
                Id = Guid.NewGuid(),
                ResourceId = ROLE_RESOURCE_ACCESS_RESOURCE_ID,
                IncludedRecourceId = RESOURCE_RESOURCE_ID,
                CollectionName = "Resources",
                Description = "Resource Assigned to Role Resource Access"
             },
            new IncludedResource { 
                Id = Guid.NewGuid(),
                ResourceId = ROLE_RESOURCE_ACCESS_RESOURCE_ID,
                IncludedRecourceId = RESOURCE_ACCESS_RESOURCE_ID,
                CollectionName = "ResourceAccesses",
                Description = "Resource Access Assigned to Role Resource Access"
             },
             //INCLUDED-RESOURCE - NONE
        };

        // Default roles
        static Guid ADMIN_ROLE_ID = Guid.NewGuid();
        static Guid USER_ROLE_ID = Guid.NewGuid();
        static Role[] DEFAULT_ROLES = new Role[] {
            new Role() { Id = ADMIN_ROLE_ID, Label = "Administrator", KeyCode = "ADMIN" },
            new Role() { Id = USER_ROLE_ID, Label = "User", KeyCode = "USER" }
        };

        public static void Up()
        {
            using (AccessContext dbContext = AccessContext.CreateContext())
            {
                // there is a admin user already then the process was completed before
                if ((from u in dbContext.Users where u.UserName == "admin" select u.Id.Value).Count() > 0)
                    return;

                // Not using the repositories because there is no user context
                // security controls should be bypassed here for direct data entry

                // Add the Access Module Resources (does not include JwtRefreshTokens because it is a hiden system resource)
                string[] DefaultResourceAccessKeyCodes = new string[] {
                    "CREATE", "READ", "UPDATE", "DELETE"
                };

                // Seed the default roles ADMIN and USER
                dbContext.Roles.AddRange(DEFAULT_ROLES);

                foreach(Resource defaultResource in DEFAULT_RESOURCES)
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
                
                dbContext.IncludedResources.AddRange(DEFAULT_INCLUDED_RESOURCES);

                // Lastly add a default system admin user
                PasswordHasher passwordHasher = new PasswordHasher();

                User adminUser = new User {
                    Id = Guid.NewGuid(),
                    UserName = "admin",
                    DisplayName = "Administrator",
                    Email = "admin@system.com",
                    FirstName = "Administrator",
                    LastName = "System",
                    Phone = "509-867-5309",
                    enabled = 1,
                    NeedsPasswordChange = true,
                    LastUpdatedStamp = DateTime.Now,
                    CreatedStamp = DateTime.Now
                };

                UserPassword newUserPassword = new UserPassword {
                    Id = Guid.NewGuid(),
                    UserId = adminUser.Id.Value,
                    Password = passwordHasher.HashPassword("@+temp-sys_pass"),
                    CreatedStamp = DateTime.Now
                };

                adminUser.CurrentUserPasswordId = newUserPassword.Id.Value;

                // Assign the new admin user the admin role
                UserRole adminUserRole = new UserRole {
                    Id = Guid.NewGuid(),
                    UserId = adminUser.Id.Value,
                    RoleId = ADMIN_ROLE_ID
                };

                dbContext.Users.Add(adminUser);
                dbContext.UserPasswords.Add(newUserPassword);
                dbContext.UserRoles.Add(adminUserRole);

                // SAVE ALL TO DB
                dbContext.SaveChanges();
            }  
        }

        public static void Down()
        {
            using (AccessContext dbContext = AccessContext.CreateContext())
            {
                // there is no admin user then data does not need to be removed
                if ((from u in dbContext.Users where u.UserName == "admin" select u.Id.Value).Count() == 0)
                    return;

                // remove the default data created

                // remove the user role for the admin user
                dbContext.UserRoles.RemoveRange(
                    from ur in dbContext.UserRoles
                    join u in dbContext.Users
                        on ur.UserId equals u.Id.Value
                    where u.UserName == "admin"
                    select ur
                );

                // remove the admin user
                dbContext.Users.RemoveRange(
                    from u in dbContext.Users
                    where u.UserName == "admin"
                    select u
                );

                // remove all the role access to the resources defined even if other migrations or processed add different role for the resources
                string[] DefaultResourceKeys = DEFAULT_RESOURCES.Select(r => r.KeyCode).ToArray();

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

                dbContext.Roles.RemoveRange(
                    from r in dbContext.Roles
                    where r.KeyCode == "ADMIN" || r.KeyCode == "USER"
                    select r
                );

                dbContext.Resources.RemoveRange(
                    from r in dbContext.Resources
                    where
                        DefaultResourceKeys.Contains(r.KeyCode)
                    select r
                );

                // SAVE ALL TO DB
                dbContext.SaveChanges();
            }
        }
    }
}