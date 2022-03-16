using Microsoft.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace Sample.Products.EntityFrameworkCore;

[ConnectionStringName("Default")]
public class ProductsDbContext : AbpDbContext<ProductsDbContext>
{
    public ProductsDbContext(DbContextOptions<ProductsDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        /* Include modules to your migration db context */

        builder.ConfigureBackgroundJobs();

        /* Configure your own tables/entities inside here */

        //builder.Entity<YourEntity>(b =>
        //{
        //    b.ToTable(ProductsConsts.DbTablePrefix + "YourEntities", ProductsConsts.DbSchema);
        //    b.ConfigureByConvention(); //auto configure for the base class props
        //    //...
        //});
    }
}
