using Volo.Abp.Account;
using Volo.Abp.Identity;
using Volo.Abp.Modularity;
using Volo.Abp.ObjectExtending;
using Volo.Abp.PermissionManagement;
using Volo.Abp.TenantManagement;

namespace Sample.Auditing;

[DependsOn(
    typeof(AuditingDomainSharedModule),
    typeof(AbpAccountApplicationContractsModule),
    typeof(AbpIdentityApplicationContractsModule),
    typeof(AbpPermissionManagementApplicationContractsModule),
    typeof(AbpTenantManagementApplicationContractsModule),
    typeof(AbpObjectExtendingModule)
)]
public class AuditingApplicationContractsModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        AuditingDtoExtensions.Configure();
    }
}
