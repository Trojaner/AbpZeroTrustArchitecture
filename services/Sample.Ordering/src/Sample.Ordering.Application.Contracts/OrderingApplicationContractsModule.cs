using Volo.Abp.Account;
using Volo.Abp.Identity;
using Volo.Abp.Modularity;
using Volo.Abp.ObjectExtending;
using Volo.Abp.PermissionManagement;
using Volo.Abp.TenantManagement;

namespace Sample.Ordering;

[DependsOn(
    typeof(OrderingDomainSharedModule),
    typeof(AbpAccountApplicationContractsModule),
    typeof(AbpIdentityApplicationContractsModule),
    typeof(AbpPermissionManagementApplicationContractsModule),
    typeof(AbpTenantManagementApplicationContractsModule),
    typeof(AbpObjectExtendingModule)
)]
public class OrderingApplicationContractsModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        OrderingDtoExtensions.Configure();
    }
}
