using Sample.Products.Localization;
using Volo.Abp.AuditLogging;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Identity;
using Volo.Abp.IdentityServer;
using Volo.Abp.Localization;
using Volo.Abp.Localization.ExceptionHandling;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.TenantManagement;
using Volo.Abp.Validation.Localization;
using Volo.Abp.VirtualFileSystem;

namespace Sample.Products;

[DependsOn(
    typeof(AbpAuditLoggingDomainSharedModule),
    typeof(AbpBackgroundJobsDomainSharedModule),
    typeof(AbpIdentityDomainSharedModule),
    typeof(AbpIdentityServerDomainSharedModule),
    typeof(AbpPermissionManagementDomainSharedModule),
    typeof(AbpTenantManagementDomainSharedModule)
    )]
public class ProductsDomainSharedModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        ProductsGlobalFeatureConfigurator.Configure();
        ProductsModuleExtensionConfigurator.Configure();
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<ProductsDomainSharedModule>();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Add<ProductsResource>("en")
                .AddBaseTypes(typeof(AbpValidationResource))
                .AddVirtualJson("/Localization/Products");

            options.DefaultResourceType = typeof(ProductsResource);
        });

        Configure<AbpExceptionLocalizationOptions>(options =>
        {
            options.MapCodeNamespace("Products", typeof(ProductsResource));
        });
    }
}
