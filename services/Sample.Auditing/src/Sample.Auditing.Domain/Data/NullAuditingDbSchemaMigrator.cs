using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace Sample.Auditing.Data;

/* This is used if database provider does't define
 * IAuditingDbSchemaMigrator implementation.
 */
public class NullAuditingDbSchemaMigrator : IAuditingDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
