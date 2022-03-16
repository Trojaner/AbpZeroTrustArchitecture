using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Volo.Abp.Auditing;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Identity;
using IdentityUser = Volo.Abp.Identity.IdentityUser;

namespace Sample.Identity;

[Authorize]
public class PermissionCheckerAppService : IPermissionCheckerAppService, ITransientDependency
{
    private readonly IPermissionStore _permissionStore;
    private readonly ILogger<PermissionCheckerAppService> _logger;
    private readonly IUserClaimsPrincipalFactory<IdentityUser> _userClaimsPrincipalFactory;
    private readonly IdentityUserManager _userManager;
    private readonly IPermissionChecker _permissionChecker;

    public PermissionCheckerAppService(
        IPermissionStore permissionStore,
        ILogger<PermissionCheckerAppService> logger,
        IUserClaimsPrincipalFactory<IdentityUser> userClaimsPrincipalFactory,
        IdentityUserManager userManager,
        IPermissionChecker permissionChecker)
    {
        _permissionStore = permissionStore;
        _logger = logger;
        _userClaimsPrincipalFactory = userClaimsPrincipalFactory;
        _userManager = userManager;
        _permissionChecker = permissionChecker;
    }

    [DisableAuditing]
    public virtual async Task<bool> CheckPermissionAsync(CheckPermissionInput input)
    {
        if (input.Type.Equals("Client", StringComparison.OrdinalIgnoreCase))
        {
            return await _permissionStore.IsGrantedAsync(input.Name, ClientPermissionValueProvider.ProviderName, input.Id);
        }

        var claimsPrincipal = await CreateUserClaimsPrincipal(input.Id, input.Type);
        var result = await _permissionChecker.IsGrantedAsync(claimsPrincipal, input.Name);

        _logger.LogInformation($"Permission \"{input.Name}\" for {input.Id ?? "<anonymous>"} ({input.Type}): {(result ? "Granted" : "Denied")}");
        return result;
    }

    [DisableAuditing]
    public virtual async Task<MultiplePermissionGrantResultDto> CheckPermissionsAsync(CheckPermissionsInput input)
    {
        var resultDto = new MultiplePermissionGrantResultDto();

        if (input.Type.Equals("Client", StringComparison.OrdinalIgnoreCase))
        {
            var result = await _permissionStore.IsGrantedAsync(input.Names.ToArray(), ClientPermissionValueProvider.ProviderName, input.Id);

            foreach (var r in result.Result)
            {
                resultDto.Result.Add(r.Key, r.Value);
            }
        }
        else
        {
            var claimsPrincipal = await CreateUserClaimsPrincipal(input.Id, input.Type);
            foreach (var name in input.Names)
            {
                var granted = await _permissionChecker.IsGrantedAsync(claimsPrincipal, name)
                    ? PermissionGrantResult.Granted
                    : PermissionGrantResult.Undefined;

                resultDto.Result.Add(name, granted);

                _logger.LogInformation(
                    $"Permission \"{name}\" for {input.Id ?? "<anonymous>"} ({input.Type}): {(granted == PermissionGrantResult.Granted ? "Granted" : "Denied")}");
            }
        }

        return resultDto;
    }

    [DisableAuditing]
    protected virtual async Task<ClaimsPrincipal> CreateUserClaimsPrincipal(string id, string type)
    {
        if (id == null)
        {
            return null;
        }

        var user = await _userManager.GetByIdAsync(Guid.Parse(id));
        if (user == null)
        {
            throw new EntityNotFoundException(typeof(IdentityUser), id);
        }

        return await _userClaimsPrincipalFactory.CreateAsync(user);
    }
}