using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Http.Client.Authentication;

namespace Sample.Remoting;

[Dependency(ReplaceServices = true)]
[ExposeServices(typeof(IRemoteServiceHttpClientAuthenticator))]
public class SampleServiceHttpClientAuthenticator : IRemoteServiceHttpClientAuthenticator, ISingletonDependency
{
    private readonly IOptions<RemotingOptions> _remotingOptions;
    private readonly IRemotingTokenStore _tokenStore;
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    public SampleServiceHttpClientAuthenticator(
        IHttpContextAccessor httpContextAccessor,
        IOptions<RemotingOptions> remotingOptions,
        IRemotingTokenStore tokenStore)
    {
        _httpContextAccessor = httpContextAccessor;
        _remotingOptions = remotingOptions;
        _tokenStore = tokenStore;
    }

    public async Task Authenticate(RemoteServiceHttpClientAuthenticateContext context)
    {
        if (!_remotingOptions.Value.UseAuthentication)
        {
            return;
        }

        var cancellationToken = CancellationToken.None;
        if (_httpContextAccessor.HttpContext != null)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            cancellationToken = httpContext.RequestAborted;
        }

        var authHeader = $"Bearer {await _tokenStore.GetTokenAsync(cancellationToken)}";

        context.Request.Headers.Authorization = null;

        if (!string.IsNullOrEmpty(authHeader))
        {
            context.Request.Headers.Add("Authorization", authHeader);
        }
    }
}
