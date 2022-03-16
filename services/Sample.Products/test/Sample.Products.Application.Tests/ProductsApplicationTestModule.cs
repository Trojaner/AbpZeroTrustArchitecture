using Volo.Abp.Modularity;

namespace Sample.Products;

[DependsOn(
    typeof(ProductsApplicationModule),
    typeof(ProductsDomainTestModule)
    )]
public class ProductsApplicationTestModule : AbpModule
{

}
