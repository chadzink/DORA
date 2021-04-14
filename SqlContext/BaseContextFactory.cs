using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DORA.SqlContext
{
    public class BaseContextFactory :
        IDesignTimeDbContextFactory<BaseContext>
    {
        public BaseContext CreateDbContext(string[] args)
        {
            BaseContext dbContext = BaseContext.CreateContext();
            dbContext.Database.Migrate();
            return dbContext;
        }
    }
}