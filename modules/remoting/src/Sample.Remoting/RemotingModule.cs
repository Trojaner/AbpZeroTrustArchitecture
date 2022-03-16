namespace Sample.Remoting;

using Auditing;
using Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Auditing;
using Volo.Abp.Modularity;
using Volo.Abp.MultiTenancy;


[DependsOn(
    typeof(IdentityApplicationContractsModule),
    typeof(AuditingApplicationContractsModule))]
public class RemotingModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {

        var configuration = context.Services.GetConfiguration();
        Configure<RemotingClientOptions>(configuration.GetSection("Remoting:Client"));

        var options = context.Services.ExecutePreConfiguredActions<RemotingOptions>();

        if (options.UseRemoteAuditing)
        {
            context.Services.AddHttpClientProxies(
                typeof(AuditingApplicationContractsModule).Assembly,
                remoteServiceConfigurationName: "Auditing"
            );

            context.Services.Replace(ServiceDescriptor.Transient<IAuditingStore, SampleAuditingStore>());
        }

        if (options.UseRemotePermissionChecks)
        {
            context.Services.AddHttpClientProxies(
                typeof(IdentityApplicationContractsModule).Assembly,
                remoteServiceConfigurationName: "Identity"
            );

            context.Services.AddHttpClientProxies(
                typeof(AbpAspNetCoreMvcContractsModule).Assembly,
                remoteServiceConfigurationName: "Identity"
            );


            context.Services.Replace(ServiceDescriptor.Transient<ITenantStore, SampleTenantStore>());
            context.Services.Replace(ServiceDescriptor.Transient<IPermissionChecker, SamplePermissionChecker>());
        }
    }
}
