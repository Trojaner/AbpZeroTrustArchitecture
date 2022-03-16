using Sample.Ordering.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace Sample.Ordering;

[DependsOn(
    typeof(OrderingEntityFrameworkCoreTestModule)
    )]
public class OrderingDomainTestModule : AbpModule
{

}
