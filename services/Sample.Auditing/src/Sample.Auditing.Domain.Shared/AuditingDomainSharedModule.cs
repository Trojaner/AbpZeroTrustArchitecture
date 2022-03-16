using Sample.Auditing.Localization;
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

namespace Sample.Auditing;

[DependsOn(
    typeof(AbpAuditLoggingDomainSharedModule),
    typeof(AbpBackgroundJobsDomainSharedModule),
    typeof(AbpIdentityDomainSharedModule),
    typeof(AbpIdentityServerDomainSharedModule),
    typeof(AbpPermissionManagementDomainSharedModule),
    typeof(AbpTenantManagementDomainSharedModule)
    )]
public class AuditingDomainSharedModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        AuditingGlobalFeatureConfigurator.Configure();
        AuditingModuleExtensionConfigurator.Configure();
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<AuditingDomainSharedModule>();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Add<AuditingResource>("en")
                .AddBaseTypes(typeof(AbpValidationResource))
                .AddVirtualJson("/Localization/Auditing");

            options.DefaultResourceType = typeof(AuditingResource);
        });

        Configure<AbpExceptionLocalizationOptions>(options =>
        {
            options.MapCodeNamespace("Auditing", typeof(AuditingResource));
        });
    }
}
