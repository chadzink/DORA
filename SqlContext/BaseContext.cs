using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DORA.SqlContext
{
    public class BaseContext : DbContext
    {
        private DbContextOptions<BaseContext> _options;
        public DbContextOptions<BaseContext> options { get { return this._options; } }

        public BaseContext(DbContextOptions<BaseContext> options)
            : base(options)
        {
            this._options = options;
        }

        internal static BaseContext CreateContext()
        {
            // Get DbContext from DI system
            var resolver = new DependencyResolver
            {
                CurrentDirectory = Directory.GetCurrentDirectory()
            };
            return resolver.ServiceProvider.GetService(typeof(BaseContext)) as BaseContext;
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
