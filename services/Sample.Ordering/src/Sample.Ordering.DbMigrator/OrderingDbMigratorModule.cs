using Sample.Ordering.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Modularity;

namespace Sample.Ordering.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(OrderingEntityFrameworkCoreModule),
    typeof(OrderingApplicationContractsModule)
    )]
public class OrderingDbMigratorModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpBackgroundJobOptions>(options => options.IsJobExecutionEnabled = false);
    }
}
