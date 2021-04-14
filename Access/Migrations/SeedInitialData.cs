
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
        public static void Up()
        {
            using (AccessContext dbContext = AccessContext.CreateContext())
            {
                // there is a admin user already then the process was completed before
                if ((from u in dbContext.Users where u.UserName == "admin" select u.Id.Value).Count() > 0)
                    return;

                // Not using the repositories because there is no user context
                // security controls should be bypassed here for direct data entry

                // Add the Access Module Resources (does not include JwtRefreshTokens because it is a system resource)
                string[] DefaultResourceKeyCodes = new string[] {
                    "RESOURCE", "RESOURCE-ACCESS", "ROLE",
                    "ROLE-RESOURCE-ACCESS", "USER", "USER-ROLE"
                };

                string[] DefaultResourceAccessKeyCodes = new string[] {
                    "CREATE", "READ", "UPDATE", "DELETE"
                };

                foreach(string resourceKey in DefaultResourceKeyCodes)
                {
                    Resource resource = new Resource() { Id = Guid.NewGuid(), KeyCode = resourceKey };
                    dbContext.Resources.Add(resource);

                    // Add the resource Acces for each resource added
                    foreach(string accessKey in DefaultResourceAccessKeyCodes)
                    {
                        ResourceAccess access = new ResourceAccess() {
                            Id = Guid.NewGuid(),
                            ResourceId = resource.Id.Value,
                            KeyCode = accessKey
                        };
                        dbContext.ResourceAccesses.Add(access);
                    }
                }

                dbContext.SaveChanges();

                // Seed the default roles ADMIN and USER
                dbContext.Roles.AddRange(
                    new List<Role>() {
                        new Role() { Id = Guid.NewGuid(), Label = "Administrator", NameCanonical = "ADMIN" },
                        new Role() { Id = Guid.NewGuid(), Label = "User", NameCanonical = "USER" }
                    }
                );

                dbContext.SaveChanges();

                // Create the default role resource access data

                // First handle the admin who has access to all the default defined above
                Role adminRole = dbContext.Roles
                    .Where(r => r.NameCanonical == "ADMIN")
                    .FirstOrDefault();

                IQueryable<RoleResourceAccess> adminAccessQuery = (
                    from ra in dbContext.ResourceAccesses
                    join r in dbContext.Resources
                        on ra.ResourceId equals r.Id
                    where
                        DefaultResourceKeyCodes.Contains(r.KeyCode)
                        && DefaultResourceAccessKeyCodes.Contains(ra.KeyCode)
                    select new RoleResourceAccess() {
                        Id = Guid.NewGuid(),
                        RoleId = adminRole.Id.Value,
                        ResourceId = ra.ResourceId,
                        ResourceAccessId = ra.Id.Value
                    }
                );

                dbContext.RoleResourcesAccesses.AddRange(adminAccessQuery);

                // Next add the default ser access, should be read for access
                Role userRole = dbContext.Roles
                    .Where(r => r.NameCanonical == "USER")
                    .FirstOrDefault();

                IQueryable<RoleResourceAccess> userAccessQuery = (
                    from ra in dbContext.ResourceAccesses
                    join r in dbContext.Resources
                        on ra.ResourceId equals r.Id
                    where
                        DefaultResourceKeyCodes.Contains(r.KeyCode)
                        && ra.KeyCode == "READ"
                    select new RoleResourceAccess() {
                        Id = Guid.NewGuid(),
                        RoleId = userRole.Id.Value,
                        ResourceId = ra.ResourceId,
                        ResourceAccessId = ra.Id.Value
                    }
                );

                dbContext.RoleResourcesAccesses.AddRange(userAccessQuery);

                dbContext.SaveChanges();

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
                    RoleId = adminRole.Id.Value
                };

                dbContext.Users.Add(adminUser);
                dbContext.UserPasswords.Add(newUserPassword);
                dbContext.UserRoles.Add(adminUserRole);

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

                string[] DefaultResourceKeyCodes = new string[] {
                    "RESOURCE", "RESOURCE-ACCESS", "ROLE",
                    "ROLE-RESOURCE-ACCESS", "USER", "USER-ROLE"
                };

                // remove all the role access to the resources defined even if other migrations or processed add different role for the resources
                dbContext.RoleResourcesAccesses.RemoveRange(
                    from rra in dbContext.RoleResourcesAccesses
                    join ra in dbContext.ResourceAccesses
                        on rra.ResourceAccessId equals ra.Id.Value
                    join r in dbContext.Resources
                        on ra.ResourceId equals r.Id.Value
                    where
                        DefaultResourceKeyCodes.Contains(r.KeyCode)
                    select rra
                );

                dbContext.Roles.RemoveRange(
                    from r in dbContext.Roles
                    where r.NameCanonical == "ADMIN" || r.NameCanonical == "USER"
                    select r
                );

                dbContext.ResourceAccesses.RemoveRange(
                    from ra in dbContext.ResourceAccesses
                    join r in dbContext.Resources
                        on ra.ResourceId equals r.Id.Value
                    where
                        DefaultResourceKeyCodes.Contains(r.KeyCode)
                    select ra
                );

                dbContext.Resources.RemoveRange(
                    from r in dbContext.Resources
                    where
                        DefaultResourceKeyCodes.Contains(r.KeyCode)
                    select r
                );
            }
        }
    }
}