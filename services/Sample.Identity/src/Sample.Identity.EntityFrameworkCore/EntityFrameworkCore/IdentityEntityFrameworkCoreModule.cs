using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.SqlServer;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.IdentityServer.EntityFrameworkCore;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.TenantManagement.EntityFrameworkCore;

namespace Sample.Identity.EntityFrameworkCore;

[DependsOn(
    typeof(IdentityDomainModule),
    typeof(AbpIdentityEntityFrameworkCoreModule),
    typeof(AbpIdentityServerEntityFrameworkCoreModule),
    typeof(AbpPermissionManagementEntityFrameworkCoreModule),
    typeof(AbpEntityFrameworkCoreSqlServerModule),
    typeof(AbpBackgroundJobsEntityFrameworkCoreModule),
    typeof(AbpTenantManagementEntityFrameworkCoreModule)
    )]
public class IdentityEntityFrameworkCoreModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        IdentityEfCoreEntityExtensionMappings.Configure();
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<IdentityDbContext>(options =>
        {
                /* Remove "includeAllEntities: true" to create
                 * default repositories only for aggregate roots */
            options.AddDefaultRepositories(includeAllEntities: true);
        });

        Configure<AbpDbContextOptions>(options =>
        {
                /* The main point to change your DBMS.
                 * See also IdentityMigrationsDbContextFactory for EF Core tooling. */
            options.UseSqlServer();
        });
    }
}
