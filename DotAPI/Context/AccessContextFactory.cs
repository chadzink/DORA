using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DORA.DotAPI.Context
{
    public class AccessContextFactory :
        IDesignTimeDbContextFactory<AccessContext>
    {
        public AccessContext CreateDbContext(string[] args)
        {
            AccessContext dbContext = AccessContext.CreateContext();
            dbContext.Database.Migrate();
            return dbContext;
        }
    }
}