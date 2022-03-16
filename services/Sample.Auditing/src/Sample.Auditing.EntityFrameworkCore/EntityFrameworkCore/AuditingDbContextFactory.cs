using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Sample.Auditing.EntityFrameworkCore;

/* This class is needed for EF Core console commands
 * (like Add-Migration and Update-Database commands) */
public class AuditingDbContextFactory : IDesignTimeDbContextFactory<AuditingDbContext>
{
    public AuditingDbContext CreateDbContext(string[] args)
    {
        AuditingEfCoreEntityExtensionMappings.Configure();

        var configuration = BuildConfiguration();

        var builder = new DbContextOptionsBuilder<AuditingDbContext>()
            .UseSqlServer(configuration.GetConnectionString("Default"));

        return new AuditingDbContext(builder.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../Sample.Auditing.DbMigrator/"))
            .AddJsonFile("appsettings.json", optional: false);

        return builder.Build();
    }
}
