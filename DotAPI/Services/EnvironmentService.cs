using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace DORA.DotAPI.Services
{
    public interface IEnvironmentService
    {
        string EnvironmentName { get; set; }
    }
    
    public interface IConfigurationService
    {
        IConfiguration GetConfiguration();
    }

    public class EnvironmentService : IEnvironmentService
    {
        public EnvironmentService()
        {
            EnvironmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                ?? "Production";
        }

        public string EnvironmentName { get; set; }
    }

    public class ConfigurationService : IConfigurationService
    {
        public IEnvironmentService EnvService { get; }
        public string CurrentDirectory { get; set; }

        public ConfigurationService(IEnvironmentService envService)
        {
            EnvService = envService;
        }

        public IConfiguration GetConfiguration()
        {
            CurrentDirectory = CurrentDirectory ?? Directory.GetCurrentDirectory();
            return new ConfigurationBuilder()
                .SetBasePath(CurrentDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{EnvService.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();
        }
    }
}