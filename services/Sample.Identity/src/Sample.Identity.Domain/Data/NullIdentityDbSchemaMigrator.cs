using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace Sample.Identity.Data;

/* This is used if database provider does't define
 * IIdentityDbSchemaMigrator implementation.
 */
public class NullIdentityDbSchemaMigrator : IIdentityDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
