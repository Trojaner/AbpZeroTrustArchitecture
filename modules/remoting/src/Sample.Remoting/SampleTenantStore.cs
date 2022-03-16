using System;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.Mvc.MultiTenancy;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Threading;

namespace Sample.Remoting;

public class SampleTenantStore : ITenantStore
{
    private readonly IAbpTenantAppService _tenantAppService;

    public SampleTenantStore(
        IAbpTenantAppService tenantAppService)
    {
        _tenantAppService = tenantAppService;
    }

    public async Task<TenantConfiguration> FindAsync(string name)
    {
        var result = await _tenantAppService.FindTenantByNameAsync(name);
        if (!result.Success)
        {
            return null;
        }

        return new TenantConfiguration(result.TenantId!.Value, result.Name);
    }

    public async Task<TenantConfiguration> FindAsync(Guid id)
    {
        var result = await _tenantAppService.FindTenantByIdAsync(id);
        if (!result.Success)
        {
            return null;
        }

        return new TenantConfiguration(result.TenantId!.Value, result.Name);
    }

    public TenantConfiguration Find(string name)
    {
        return AsyncHelper.RunSync(() => FindAsync(name));
    }

    public TenantConfiguration Find(Guid id)
    {
        return AsyncHelper.RunSync(() => FindAsync(id));
    }
}
