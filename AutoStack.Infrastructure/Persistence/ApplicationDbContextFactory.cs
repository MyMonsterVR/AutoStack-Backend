using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace AutoStack.Infrastructure.Persistence;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        // Find the Presentation project folder
        var currentPath = Directory.GetCurrentDirectory();
        var presentationPath = Path.Combine(currentPath, "AutoStack.Presentation");

        // If we're in a subdirectory (like Infrastructure), go up one level
        if (!Directory.Exists(presentationPath))
        {
            presentationPath = Path.Combine(currentPath, "..", "AutoStack.Presentation");
        }

        var configuration = new ConfigurationBuilder()
            .SetBasePath(presentationPath)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        // Ensure the database is created in the Presentation folder
        var absoluteConnectionString = connectionString?.Replace("Data Source=", $"Data Source={presentationPath}\\");

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlite(absoluteConnectionString);

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
