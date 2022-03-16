using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Sample.Identity.EntityFrameworkCore;

/* This class is needed for EF Core console commands
 * (like Add-Migration and Update-Database commands) */
public class IdentityDbContextFactory : IDesignTimeDbContextFactory<IdentityDbContext>
{
    public IdentityDbContext CreateDbContext(string[] args)
    {
        IdentityEfCoreEntityExtensionMappings.Configure();

        var configuration = BuildConfiguration();

        var builder = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseSqlServer(configuration.GetConnectionString("Default"));

        return new IdentityDbContext(builder.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../Sample.Identity.DbMigrator/"))
            .AddJsonFile("appsettings.json", optional: false);

        return builder.Build();
    }
}
