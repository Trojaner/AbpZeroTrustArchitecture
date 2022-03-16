using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.SqlServer;
using Volo.Abp.Modularity;


namespace Sample.Products.EntityFrameworkCore;

[DependsOn(
    typeof(ProductsDomainModule),
    typeof(AbpEntityFrameworkCoreSqlServerModule),
    typeof(AbpBackgroundJobsEntityFrameworkCoreModule)
    )]
public class ProductsEntityFrameworkCoreModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        ProductsEfCoreEntityExtensionMappings.Configure();
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<ProductsDbContext>(options =>
        {
                /* Remove "includeAllEntities: true" to create
                 * default repositories only for aggregate roots */
            options.AddDefaultRepositories(includeAllEntities: true);
        });

        Configure<AbpDbContextOptions>(options =>
        {
                /* The main point to change your DBMS.
                 * See also ProductsMigrationsDbContextFactory for EF Core tooling. */
            options.UseSqlServer();
        });
    }
}
