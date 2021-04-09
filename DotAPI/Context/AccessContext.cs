using Microsoft.EntityFrameworkCore;
using DORA.DotAPI.Context.Entities;
using Microsoft.Extensions.Configuration;
using DORA.DotAPI.Helpers;
using System.IO;

namespace DORA.DotAPI.Context
{
    public class AccessContext : DbContext
    {
        public AccessContext(DbContextOptions<AccessContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            PasswordHasher passwordHasher = new PasswordHasher();

            modelBuilder.Entity<UserRole>(entity => {
                entity.HasKey(ur => new { ur.UserId, ur.RoleId });

                entity.HasOne(ur => ur.User)
                    .WithMany(u => u.UserRoles)
                    .HasForeignKey(ur => ur.UserId);

                entity.HasOne(ur => ur.Role)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(ur => ur.RoleId);
            });

            modelBuilder.Entity<ResourceAccess>(entity => {

                entity.HasOne(rr => rr.Resource)
                    .WithMany(r => r.ResourceAccesses)
                    .HasForeignKey(rr => rr.ResourceId);

                entity.HasMany(ra => ra.RoleResourceAccesses)
                    .WithOne(rra => rra.ResourceAccess)
                    .HasForeignKey(ra => ra.ResourceAccessId);
            });

            modelBuilder.Entity<RoleResourceAccess>(entity => {
                entity.HasKey(rr => new { rr.ResourceId, rr.RoleId, rr.ResourceAccessId });

                entity.HasOne(rra => rra.Role)
                    .WithMany(r => r.RoleResourcesAccess)
                    .HasForeignKey(ur => ur.RoleId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(rra => rra.Resource)
                    .WithMany(rs => rs.RoleResourceAccesses)
                    .HasForeignKey(ur => ur.ResourceId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(rra => rra.ResourceAccess)
                    .WithMany(ra => ra.RoleResourceAccesses)
                    .HasForeignKey(ur => ur.ResourceAccessId)
                    .OnDelete(DeleteBehavior.NoAction);
            });
        }

        internal static AccessContext CreateContext()
        {

            // Get DbContext from DI system
            var resolver = new DependencyResolver
            {
                CurrentDirectory = Directory.GetCurrentDirectory()
            };
            return resolver.ServiceProvider.GetService(typeof(AccessContext)) as AccessContext;
        }

        public IConfiguration Configuration { get; }

        public DbSet<User> Users { get; set; }
        public DbSet<UserPassword> UserPasswords { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Resource> Resources { get; set; }
        public DbSet<RoleResourceAccess> RoleResourcesAccesses { get; set; }
        public DbSet<ResourceAccess> ResourceAccesses { get; set; }
        public DbSet<JwtRefreshToken> JwtRefreshTokens { get; set; }
    }
}
