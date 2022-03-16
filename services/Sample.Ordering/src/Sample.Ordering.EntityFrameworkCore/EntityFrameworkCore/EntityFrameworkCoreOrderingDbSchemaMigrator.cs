using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Sample.Ordering.Data;
using Volo.Abp.DependencyInjection;

namespace Sample.Ordering.EntityFrameworkCore;

public class EntityFrameworkCoreOrderingDbSchemaMigrator
    : IOrderingDbSchemaMigrator, ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public EntityFrameworkCoreOrderingDbSchemaMigrator(
        IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task MigrateAsync()
    {
        /* We intentionally resolving the OrderingDbContext
         * from IServiceProvider (instead of directly injecting it)
         * to properly get the connection string of the current tenant in the
         * current scope.
         */

        await _serviceProvider
            .GetRequiredService<OrderingDbContext>()
            .Database
            .MigrateAsync();
    }
}
