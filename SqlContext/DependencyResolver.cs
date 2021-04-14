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

                string connectionString = configuration.GetConnectionString("BaseConnection");
                DbContextOptionsBuilder<BaseContext> optionsBuilder = new DbContextOptionsBuilder<BaseContext>();

                string targetAssembly = "SqlContext";
                if (configuration["Migrations:SqlTargetMigrationsAssembly"] != null)
                    targetAssembly = configuration["Migrations:SqlTargetMigrationsAssembly"];

                optionsBuilder.UseSqlServer(
                    connectionString,
                    builder => builder.MigrationsAssembly(targetAssembly)
                );

                return new BaseContext(optionsBuilder.Options);
            });
        }
    }
}
