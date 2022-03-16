using System.Threading;
using System.Threading.Tasks;

namespace Sample.Remoting;

public interface IRemotingTokenStore
{
    Task<string> GetTokenAsync(CancellationToken cancellationToken);
}