using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace AutoStack.Infrastructure.Persistence;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var currentPath = Directory.GetCurrentDirectory();
        var presentationPath = Path.Combine(currentPath, "AutoStack.Presentation");

        if (!Directory.Exists(presentationPath))
        {
            presentationPath = Path.Combine(currentPath, "..", "AutoStack.Presentation");
        }

        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                          ?? "Development";

        var configBuilder = new ConfigurationBuilder()
            .SetBasePath(presentationPath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true);

        if (environment == "Development")
        {
            var userSecretsId = GetUserSecretsId(presentationPath);
            if (!string.IsNullOrEmpty(userSecretsId))
            {
                configBuilder.AddUserSecrets(userSecretsId);
            }
        }

        var configuration = configBuilder.Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new ApplicationDbContext(optionsBuilder.Options);
    }

    private static string? GetUserSecretsId(string projectPath)
    {
        var csprojFiles = Directory.GetFiles(projectPath, "*.csproj");
        if (csprojFiles.Length == 0) return null;

        var csprojPath = csprojFiles[0];
        var doc = XDocument.Load(csprojPath);
        var userSecretsId = doc.Descendants("UserSecretsId").FirstOrDefault()?.Value;

        return userSecretsId;
    }
}
