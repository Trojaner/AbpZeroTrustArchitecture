using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Sample.Ordering.EntityFrameworkCore;

/* This class is needed for EF Core console commands
 * (like Add-Migration and Update-Database commands) */
public class OrderingDbContextFactory : IDesignTimeDbContextFactory<OrderingDbContext>
{
    public OrderingDbContext CreateDbContext(string[] args)
    {
        OrderingEfCoreEntityExtensionMappings.Configure();

        var configuration = BuildConfiguration();

        var builder = new DbContextOptionsBuilder<OrderingDbContext>()
            .UseSqlServer(configuration.GetConnectionString("Default"));

        return new OrderingDbContext(builder.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../Sample.Ordering.DbMigrator/"))
            .AddJsonFile("appsettings.json", optional: false);

        return builder.Build();
    }
}
