using Sample.Auditing.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace Sample.Auditing;

[DependsOn(
    typeof(AuditingEntityFrameworkCoreTestModule)
    )]
public class AuditingDomainTestModule : AbpModule
{

}
