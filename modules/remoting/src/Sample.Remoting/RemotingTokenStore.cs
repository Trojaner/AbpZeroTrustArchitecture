using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;

namespace Sample.Remoting;

public class RemotingTokenStore : IRemotingTokenStore, ISingletonDependency
{
    private readonly IOptions<RemotingClientOptions> _clientOptions;
    private readonly ILogger<RemotingTokenStore> _logger;
    private readonly IConfiguration _configuration;
    private readonly TimeSpan _refreshOffset = TimeSpan.FromMinutes(1);
    private readonly AsyncLocal<Random> _random;

    private string _token;

    public RemotingTokenStore(
        IOptions<RemotingClientOptions> clientOptions,
        ILogger<RemotingTokenStore> logger,
        IConfiguration configuration)
    {
        _clientOptions = clientOptions;
        _logger = logger;
        _configuration = configuration;

        _random = new AsyncLocal<Random>();
    }

    public virtual Task<string> GetTokenAsync(CancellationToken cancellationToken)
    {
        return GetTokenInternalAsync(cancellationToken);
    }

    protected virtual async Task<string> GetTokenInternalAsync(CancellationToken cancellationToken, int retryCount = 0)
    {
        if (!_token.IsNullOrEmpty())
        {
            return _token;
        }

        _logger.LogInformation($"Logging in...");

        var clientId = _clientOptions.Value.ClientName;
        var clientSecret = _clientOptions.Value.ClientSecret;
        var scope = _clientOptions.Value.Scope;

        _random.Value ??= new Random();

        if (retryCount > 0)
        {
            // Read https://dzone.com/articles/understanding-retry-pattern-with-exponential-back for more information on why we need a backoff delay
            var delay = CalculateBackoffDelay(retryCount);

            _logger.LogCritical($"[#{retryCount}] Failed to log in, retrying in {delay} seconds...");

            await Task.Delay(TimeSpan.FromSeconds(delay), cancellationToken);
        }

        var authority = _configuration["AuthServer:Authority"];
        var client = new HttpClient();
        var discovery = await client.GetDiscoveryDocumentAsync(authority, cancellationToken);
        if (discovery.IsError)
        {
            throw new Exception($"Failed to get discovery document from {authority}: {discovery.Error}", discovery.Exception);
        }

        var tokenResponse = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
        {
            Address = discovery.TokenEndpoint,
            ClientId = clientId,
            ClientSecret = clientSecret,
            Scope = scope
        }, cancellationToken);

        if (tokenResponse.IsError)
        {
            throw new Exception($"Client login failed: {tokenResponse.Error}", tokenResponse.Exception);
        }

        _token = tokenResponse.AccessToken;

        StartRenewTask(client, discovery, tokenResponse, cancellationToken, retryCount);

        return _token;
    }

    protected virtual double CalculateBackoffDelay(int tryCount)
    {
        var delay = Math.Pow(2, tryCount) - 1;

        delay = Math.Max(delay, 180);

        var jitterPrcentage = 25;
        var lowerBoundary = delay * (100 - jitterPrcentage) / 100;
        var upperBoundary = delay * (100 + jitterPrcentage) / 100;

        delay += (upperBoundary - lowerBoundary) * _random.Value.NextDouble();

        return delay;
    }

    protected virtual void StartRenewTask(
        HttpClient client,
        DiscoveryDocumentResponse discovery,
        TokenResponse tokenResponse,
        CancellationToken cancellationToken,
        int retryCount)
    {
        var nextRefresh = tokenResponse.ExpiresIn;
        var refreshToken = tokenResponse.RefreshToken;

        Task.Factory.StartNew(async () =>
        {
            renewDelay:
            await Task.Delay(TimeSpan.FromSeconds(nextRefresh) - _refreshOffset, cancellationToken);

            _logger.LogInformation($"Refreshing client token...");

            var refreshResult = await client.RequestRefreshTokenAsync(new RefreshTokenRequest
            {
                Address = discovery.TokenEndpoint,
                RefreshToken = refreshToken
            }, cancellationToken: cancellationToken);

            if (refreshResult.IsError)
            {
                _logger.LogError($"Client token refresh failed: " + refreshResult.Error);

                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                await GetTokenInternalAsync(cancellationToken, ++retryCount);
                return;
            }

            retryCount = 0;
            nextRefresh = refreshResult.ExpiresIn;
            refreshToken = refreshResult.RefreshToken;
            _token = refreshResult.AccessToken;

            goto renewDelay;

        }, cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Current).ConfigureAwait(false);
    }
}