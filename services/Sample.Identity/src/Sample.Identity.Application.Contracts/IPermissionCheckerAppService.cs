namespace Sample.Identity;

using System.Threading.Tasks;
using Volo.Abp.Application.Services;

public interface IPermissionCheckerAppService : IApplicationService
{
    Task<bool> CheckPermissionAsync(CheckPermissionInput input);

    Task<MultiplePermissionGrantResultDto> CheckPermissionsAsync(CheckPermissionsInput input);
}