using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.SqlServer;
using Volo.Abp.Modularity;

namespace Sample.Ordering.EntityFrameworkCore;

[DependsOn(
    typeof(OrderingDomainModule),
    typeof(AbpEntityFrameworkCoreSqlServerModule),
    typeof(AbpBackgroundJobsEntityFrameworkCoreModule)
    )]
public class OrderingEntityFrameworkCoreModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        OrderingEfCoreEntityExtensionMappings.Configure();
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<OrderingDbContext>(options =>
        {
                /* Remove "includeAllEntities: true" to create
                 * default repositories only for aggregate roots */
            options.AddDefaultRepositories(includeAllEntities: true);
        });

        Configure<AbpDbContextOptions>(options =>
        {
                /* The main point to change your DBMS.
                 * See also OrderingMigrationsDbContextFactory for EF Core tooling. */
            options.UseSqlServer();
        });
    }
}
