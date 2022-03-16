using System.Threading.Tasks;

namespace Sample.Ordering.Data;

public interface IOrderingDbSchemaMigrator
{
    Task MigrateAsync();
}
