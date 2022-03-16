using Volo.Abp.Modularity;

namespace Sample.Ordering;

[DependsOn(
    typeof(OrderingApplicationModule),
    typeof(OrderingDomainTestModule)
    )]
public class OrderingApplicationTestModule : AbpModule
{

}
