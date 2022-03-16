using Sample.Products.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace Sample.Products;

[DependsOn(
    typeof(ProductsEntityFrameworkCoreTestModule)
    )]
public class ProductsDomainTestModule : AbpModule
{

}
