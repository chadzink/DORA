using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DORA.SqlContext
{
    public class SqlContext : DbContext
    {
        public SqlContext(DbContextOptions<SqlContext> options)
            : base(options)
        {
        }

        internal static SqlContext CreateContext()
        {
            // Get DbContext from DI system
            var resolver = new DependencyResolver
            {
                CurrentDirectory = Directory.GetCurrentDirectory()
            };
            return resolver.ServiceProvider.GetService(typeof(SqlContext)) as SqlContext;
        }

        public static IConfiguration GetConfiguration()
        {
            // Get IConfiguration from DI system
            var resolver = new DependencyResolver
            {
                CurrentDirectory = Directory.GetCurrentDirectory()
            };

            IConfigurationService configService = resolver.ServiceProvider
                .GetService(typeof(IConfigurationService)) as IConfigurationService;

            return configService.GetConfiguration();
        }
    }
}
