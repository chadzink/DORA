using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DORA.Navigation.Context
{
    public class NavigationContextFactory :
        IDesignTimeDbContextFactory<NavigationContext>
    {
        public NavigationContext CreateDbContext(string[] args)
        {
            NavigationContext dbContext = NavigationContext.CreateContext();
            dbContext.Database.Migrate();
            return dbContext;
        }
    }
}