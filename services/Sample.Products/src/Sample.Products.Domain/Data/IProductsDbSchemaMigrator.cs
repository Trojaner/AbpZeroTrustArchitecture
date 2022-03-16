using System.Threading.Tasks;

namespace Sample.Products.Data;

public interface IProductsDbSchemaMigrator
{
    Task MigrateAsync();
}
