using Volo.Abp.Account;
using Volo.Abp.AutoMapper;
using Volo.Abp.Identity;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.TenantManagement;

namespace Sample.Products;

[DependsOn(
    typeof(ProductsDomainModule),
    typeof(AbpAccountApplicationModule),
    typeof(ProductsApplicationContractsModule),
    typeof(AbpIdentityApplicationModule),
    typeof(AbpPermissionManagementApplicationModule),
    typeof(AbpTenantManagementApplicationModule)
    )]
public class ProductsApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<ProductsApplicationModule>();
        });
    }
}
