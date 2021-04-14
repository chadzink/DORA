using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DORA.Access.Context
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