using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace Sample.Ordering.Data;

/* This is used if database provider does't define
 * IOrderingDbSchemaMigrator implementation.
 */
public class NullOrderingDbSchemaMigrator : IOrderingDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
