using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using DORA.Navigation.Context.Entities;
using DORA.SqlContext;
using System.IO;

namespace DORA.Navigation.Context
{
    public class NavigationContext : BaseContext
    {
        public NavigationContext(DbContextOptions options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<NavGroup>(entity => {
                entity.HasMany(g => g.NavItems)
                    .WithOne(i => i.NavGroup)
                    .HasForeignKey(ur => ur.NavGroupId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<NavItem>(entity => {
                entity.HasOne(i => i.NavGroup)
                    .WithMany(g => g.NavItems)
                    .HasForeignKey(i => i.NavGroupId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(i => i.ParentNavItem)
                    .WithMany(p => p.ChildNavItems)
                    .HasForeignKey(i => i.ParentNavItemId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<RoleNavGroup>(entity => {
                entity.HasKey(rng => new { rng.NavGroupId, rng.RoleId });

                entity.HasOne(rng => rng.Role);
                entity.HasOne(rng => rng.NavGroup);
            });

            modelBuilder.Entity<RoleNavItem>(entity => {
                entity.HasKey(rng => new { rng.NavItemId, rng.RoleId });

                entity.HasOne(rng => rng.Role);
                entity.HasOne(rng => rng.NavItem);
            });
        }

        public static NavigationContext CreateContext()
        {
            DependencyResolver resolver = new DependencyResolver
            {
                CurrentDirectory = Directory.GetCurrentDirectory(),
                TargetAssembly = "Navigation",
                ConnectionStringsKey = "BaseConnection",
            };
            BaseContext baseCtx = resolver.ServiceProvider.GetService(typeof(BaseContext)) as BaseContext;

            return new NavigationContext(baseCtx.options);
        }

        public IConfiguration Configuration { get; }

        public DbSet<NavGroup> NavGroups { get; set; }
        public DbSet<NavItem> NavItems { get; set; }
        public DbSet<RoleNavGroup> RoleNavGroups { get; set; }
        public DbSet<RoleNavItem> RoleNavItems { get; set; }
    }
}
