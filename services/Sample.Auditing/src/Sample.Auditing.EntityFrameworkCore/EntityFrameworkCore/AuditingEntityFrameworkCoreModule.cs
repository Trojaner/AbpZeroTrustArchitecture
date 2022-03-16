using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.SqlServer;
using Volo.Abp.Modularity;

namespace Sample.Auditing.EntityFrameworkCore;

[DependsOn(
    typeof(AuditingDomainModule),
    typeof(AbpEntityFrameworkCoreSqlServerModule),
    typeof(AbpAuditLoggingEntityFrameworkCoreModule)
    )]
public class AuditingEntityFrameworkCoreModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        AuditingEfCoreEntityExtensionMappings.Configure();
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<AuditingDbContext>(options =>
        {
                /* Remove "includeAllEntities: true" to create
                 * default repositories only for aggregate roots */
            options.AddDefaultRepositories(includeAllEntities: true);
        });

        Configure<AbpDbContextOptions>(options =>
        {
                /* The main point to change your DBMS.
                 * See also AuditingMigrationsDbContextFactory for EF Core tooling. */
            options.UseSqlServer();
        });
    }
}
