using Microsoft.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace Sample.Ordering.EntityFrameworkCore;

[ConnectionStringName("Default")]
public class OrderingDbContext :
    AbpDbContext<OrderingDbContext>
{
    /* Add DbSet properties for your Aggregate Roots / Entities here. */

    public OrderingDbContext(DbContextOptions<OrderingDbContext> options)
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
        //    b.ToTable(OrderingConsts.DbTablePrefix + "YourEntities", OrderingConsts.DbSchema);
        //    b.ConfigureByConvention(); //auto configure for the base class props
        //    //...
        //});
    }
}
