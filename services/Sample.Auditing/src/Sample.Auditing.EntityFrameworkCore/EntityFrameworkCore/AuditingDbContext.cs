using Microsoft.EntityFrameworkCore;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Identity;
using Volo.Abp.TenantManagement;

namespace Sample.Auditing.EntityFrameworkCore;

[ConnectionStringName("Default")]
public class AuditingDbContext :
    AbpDbContext<AuditingDbContext>
{
    /* Add DbSet properties for your Aggregate Roots / Entities here. */

    public AuditingDbContext(DbContextOptions<AuditingDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        /* Include modules to your migration db context */

        builder.ConfigureAuditLogging();


        /* Configure your own tables/entities inside here */

        //builder.Entity<YourEntity>(b =>
        //{
        //    b.ToTable(AuditingConsts.DbTablePrefix + "YourEntities", AuditingConsts.DbSchema);
        //    b.ConfigureByConvention(); //auto configure for the base class props
        //    //...
        //});
    }
}
