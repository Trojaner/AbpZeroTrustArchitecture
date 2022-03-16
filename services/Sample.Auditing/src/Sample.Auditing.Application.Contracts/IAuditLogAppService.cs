using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Sample.Auditing;

public interface IAuditLogAppService : IApplicationService
{
    Task CreateAsync(AuditLogInfoDto logInfo);
}