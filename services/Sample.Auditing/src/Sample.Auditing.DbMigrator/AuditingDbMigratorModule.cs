using Sample.Auditing.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Modularity;

namespace Sample.Auditing.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(AuditingEntityFrameworkCoreModule),
    typeof(AuditingApplicationContractsModule)
    )]
public class AuditingDbMigratorModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpBackgroundJobOptions>(options => options.IsJobExecutionEnabled = false);
    }
}
