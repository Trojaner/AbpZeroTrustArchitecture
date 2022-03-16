using Volo.Abp.Account;
using Volo.Abp.AutoMapper;
using Volo.Abp.Identity;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.TenantManagement;

namespace Sample.Ordering;

[DependsOn(
    typeof(OrderingDomainModule),
    typeof(AbpAccountApplicationModule),
    typeof(OrderingApplicationContractsModule),
    typeof(AbpIdentityApplicationModule),
    typeof(AbpPermissionManagementApplicationModule),
    typeof(AbpTenantManagementApplicationModule)
    )]
public class OrderingApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<OrderingApplicationModule>();
        });
    }
}
