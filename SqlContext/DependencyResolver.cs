using System;
using DORA.DotAPI.Context;
using DORA.DotAPI.Context.Repositories;
using DORA.DotAPI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DORA.DotAPI.Helpers
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

                string connectionString = configuration.GetConnectionString("AccessConnection");
                DbContextOptionsBuilder<AccessContext> optionsBuilder = new DbContextOptionsBuilder<AccessContext>();

                string targetAssembly = "DotAPI";
                if (configuration["Migrations:AccessTargetMigrationsAssembly"] != null)
                    targetAssembly = configuration.GetValue<string>("Migrations:AccessTargetMigrationsAssembly");

                optionsBuilder.UseSqlServer(
                    connectionString,
                    builder => builder.MigrationsAssembly(targetAssembly)
                );

                return new AccessContext(optionsBuilder.Options);
            });
        }
    }
}
