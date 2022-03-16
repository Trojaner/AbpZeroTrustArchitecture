using Volo.Abp.Modularity;

namespace Sample.Auditing;

[DependsOn(
    typeof(AuditingApplicationModule),
    typeof(AuditingDomainTestModule)
    )]
public class AuditingApplicationTestModule : AbpModule
{

}
