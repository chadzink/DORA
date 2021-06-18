using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DORA.SqlContext
{
    public class DependencyResolver
    {
        public IServiceProvider ServiceProvider { get; }
        public string CurrentDirectory { get; set; }
        public string TargetAssembly { get; set; }
        public string ConnectionStringsKey { get; set; }

        public DependencyResolver()
        {
            // Set up Dependency Injection
            IServiceCollection services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Register env and config services
            services.AddTransient<IEnvironmentService, EnvironmentService>();
            services.AddTransient<IConfigurationService, ConfigurationService>
                (provider => new ConfigurationService(provider.GetService<IEnvironmentService>())
                {
                    CurrentDirectory = CurrentDirectory
                });

            // Register DbContext class
            services.AddTransient(provider =>
            {
                IConfigurationService configService = provider.GetService<IConfigurationService>();
                IConfiguration configuration = configService.GetConfiguration();

                string connectionString = configuration.GetConnectionString(ConnectionStringsKey);
                DbContextOptionsBuilder<BaseContext> optionsBuilder = new DbContextOptionsBuilder<BaseContext>();

                optionsBuilder.UseSqlServer(
                    connectionString,
                    builder => builder.MigrationsAssembly(TargetAssembly)
                );

                return new BaseContext(optionsBuilder.Options);
            });
        }
    }
}
