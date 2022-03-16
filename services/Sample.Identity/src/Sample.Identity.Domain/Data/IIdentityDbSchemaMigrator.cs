using System.Threading.Tasks;

namespace Sample.Identity.Data;

public interface IIdentityDbSchemaMigrator
{
    Task MigrateAsync();
}
