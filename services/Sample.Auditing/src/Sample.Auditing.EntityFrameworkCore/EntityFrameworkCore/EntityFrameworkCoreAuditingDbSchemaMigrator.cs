using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Sample.Auditing.Data;
using Volo.Abp.DependencyInjection;

namespace Sample.Auditing.EntityFrameworkCore;

public class EntityFrameworkCoreAuditingDbSchemaMigrator
    : IAuditingDbSchemaMigrator, ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public EntityFrameworkCoreAuditingDbSchemaMigrator(
        IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task MigrateAsync()
    {
        /* We intentionally resolving the AuditingDbContext
         * from IServiceProvider (instead of directly injecting it)
         * to properly get the connection string of the current tenant in the
         * current scope.
         */

        await _serviceProvider
            .GetRequiredService<AuditingDbContext>()
            .Database
            .MigrateAsync();
    }
}
