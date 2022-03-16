using System.Threading.Tasks;

namespace Sample.Auditing.Data;

public interface IAuditingDbSchemaMigrator
{
    Task MigrateAsync();
}
