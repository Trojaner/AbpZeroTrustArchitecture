using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;

namespace Sample.Identity;

public class IdentityTestDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    public Task SeedAsync(DataSeedContext context)
    {
        /* Seed additional test data... */

        return Task.CompletedTask;
    }
}
