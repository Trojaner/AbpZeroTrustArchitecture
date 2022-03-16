using Sample.Identity.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace Sample.Identity;

[DependsOn(
    typeof(IdentityEntityFrameworkCoreTestModule)
    )]
public class IdentityDomainTestModule : AbpModule
{

}
