# Zero Trust Microservice Architecture with ABP Framework

## Introduction
In this article we will explore how to integrate a zero trust microservice architecture with ABP Framework. We will implement centralized permission management, tenant management and auditing. Each microservice will have it's own identity and permissions.

### What is zero trust?
[Zero trust](https://en.wikipedia.org/wiki/Zero_trust_security_model) refers to a security model where you never implicitly trust a device or service and always verify instead. For example, many organizations trust all devices inside their network. In a zero trust environment, there is no difference between a device that is inside or outside a network.  

### How does a zero trust microservice architecture work?
In a zero trust microservice architecture, no service is trusted implicitly. All services must have an identity and explicit permissions to execute an action on other services. This way, if any service gets compromised, the attacker will only have as much authorization as the service itself, which should be limited to the service's dependencies. 

 ## Getting started

### Project Architecture
The project will consist of 4 microservices:  
* Two generic microservices where one depends on another for demonstration purposes
* Identity microservice for authentication and authorization
* Auditing microservice

#### Creating the Identity Service
This service will host IdentityServer with the permission management and tenant management modules in the EntityFrameworkCore layer.

Create a new project using `abp new Sample.Identity -t app`. After that, remove all modules expect for permission management and tenant management.

After that create the following app service and DTOs in application contracts:
```c#
namespace Sample.Identity;

public interface IPermissionCheckerAppService : IApplicationService
{
    Task<bool> CheckPermissionAsync(CheckPermissionInput input);

    Task<MultiplePermissionGrantResultDto> CheckPermissionsAsync(CheckPermissionsInput input);
}
```

```c#
namespace Sample.Identity;

[Serializable]
public class CheckPermissionInput : EntityDto
{
    public string Id { get; set; }
    public string Type { get; set; }
    public string Name { get; set; }
}
```
```c#
namespace Sample.Identity;

[Serializable]
public class CheckPermissionsInput : EntityDto
{
    public string Id { get; set; }
    public string Type { get; set; }
    public ICollection<string> Names { get; set; }
}
```
```c#
namespace Sample.Identity;

[Serializable]
public class MultiplePermissionGrantResultDto
{
    public Dictionary<string, PermissionGrantResult> Result { get; set; }
}
```

and implement the service in the application layer to expose permission checking:
```c#
namespace Sample.Identity;

[Authorize]
public class PermissionCheckerAppService : IPermissionCheckerAppService, ITransientDependency
{
    private readonly IPermissionStore _permissionStore;
    private readonly ILogger<PermissionCheckerAppService> _logger;
    private readonly IUserClaimsPrincipalFactory<IdentityUser> _userClaimsPrincipalFactory;
    private readonly IdentityUserManager _userManager;
    private readonly IPermissionChecker _permissionChecker;

    public PermissionCheckerAppService(
        IPermissionStore permissionStore,
        ILogger<PermissionCheckerAppService> logger,
        IUserClaimsPrincipalFactory<IdentityUser> userClaimsPrincipalFactory,
        IdentityUserManager userManager,
        IPermissionChecker permissionChecker)
    {
        _permissionStore = permissionStore;
        _logger = logger;
        _userClaimsPrincipalFactory = userClaimsPrincipalFactory;
        _userManager = userManager;
        _permissionChecker = permissionChecker;
    }

    [DisableAuditing]
    public virtual async Task<bool> CheckPermissionAsync(CheckPermissionInput input)
    {
        if (input.Type.Equals("Client", StringComparison.OrdinalIgnoreCase))
        {
            return await _permissionStore.IsGrantedAsync(input.Name, ClientPermissionValueProvider.ProviderName, input.Id);
        }

        var claimsPrincipal = await CreateUserClaimsPrincipal(input.Id, input.Type);
        var result = await _permissionChecker.IsGrantedAsync(claimsPrincipal, input.Name);

        _logger.LogInformation($"Permission \"{input.Name}\" for {input.Id ?? "<anonymous>"} ({input.Type}): {(result ? "Granted" : "Denied")}");
        return result;
    }

    [DisableAuditing]
    public virtual async Task<MultiplePermissionGrantResultDto> CheckPermissionsAsync(CheckPermissionsInput input)
    {
        var resultDto = new MultiplePermissionGrantResultDto();

        if (input.Type.Equals("Client", StringComparison.OrdinalIgnoreCase))
        {
            var result = await _permissionStore.IsGrantedAsync(input.Names.ToArray(), ClientPermissionValueProvider.ProviderName, input.Id);

            foreach (var r in result.Result)
            {
                resultDto.Result.Add(r.Key, r.Value);
            }
        }
        else
        {
            var claimsPrincipal = await CreateUserClaimsPrincipal(input.Id, input.Type);
            foreach (var name in input.Names)
            {
                var granted = await _permissionChecker.IsGrantedAsync(claimsPrincipal, name)
                    ? PermissionGrantResult.Granted
                    : PermissionGrantResult.Undefined;

                resultDto.Result.Add(name, granted);

                _logger.LogInformation(
                    $"Permission \"{name}\" for {input.Id ?? "<anonymous>"} ({input.Type}): {(granted == PermissionGrantResult.Granted ? "Granted" : "Denied")}");
            }
        }

        return resultDto;
    }

    [DisableAuditing]
    protected virtual async Task<ClaimsPrincipal> CreateUserClaimsPrincipal(string id, string type)
    {
        if (id == null)
        {
            return null;
        }

        var user = await _userManager.GetByIdAsync(Guid.Parse(id));
        if (user == null)
        {
            throw new EntityNotFoundException(typeof(IdentityUser), id);
        }

        return await _userClaimsPrincipalFactory.CreateAsync(user);
    }
}
```

#### Creating the Products and Ordering Microservices
These will be our sample generic microservices where one microservice will depend on another (in this case ordering will depend on products).

Create the services by using `abp new Sample.<Name> -t app --tiered` and by deleting the IdentityServer layer manually. Finally remove the EntityFrameworkCore modules for the feature-management, auditing and tenant-management modules as these will not be needed. 

After that create a sample app service in products that orders will consume. Don't forget to add the permissions. 

**Note:** Each microservice's application contracts layer must be added to the identity host module so the permissions get registered.

#### Creating the Auditing Microservice
This service will host the full ABP auditing module.  

Create a new project using `abp new Sample.Auditing -t app`. After that, remove all modules expect for auditing itself in the EntityFrameworkCore layer.

Similar to the identity service, we need to expose an app service for creating audit logs.  

Start by creating the following service and DTOs in the application contracts layer:
```c#
namespace Sample.Auditing;

public interface IAuditLogAppService : IApplicationService
{
    Task CreateAsync(AuditLogInfoDto logInfo);

    Task<PagedResultDto<AuditLogInfoDto>> GetListAsync(GetAuditLogsInput input);
}
```

```c#
namespace Sample.Auditing;

[Serializable]
public class AuditLogInfoDto : EntityDto
{
    public string ApplicationName { get; set; }
    public Guid? UserId { get; set; }
    public string UserName { get; set; }
    public Guid? TenantId { get; set; }
    public string TenantName { get; set; }
    public Guid? ImpersonatorUserId { get; set; }
    public Guid? ImpersonatorTenantId { get; set; }
    public DateTime ExecutionTime { get; set; }
    public int ExecutionDuration { get; set; }
    public string ClientId { get; set; }
    public string CorrelationId { get; set; }
    public string ClientIpAddress { get; set; }
    public string ClientName { get; set; }
    public string BrowserInfo { get; set; }
    public string HttpMethod { get; set; }
    public int? HttpStatusCode { get; set; }
    public string Url { get; set; }
    public ICollection<AuditLogActionInfoDto> Actions { get; set; }
    public string Exceptions { get; set; }
    public string Comments { get; set; }
    public ICollection<EntityChangeInfoDto> EntityChanges { get; set; }
}
```

```c#
namespace Sample.Auditing;

[Serializable]
public class AuditLogActionInfoDto
{
    public string ServiceName { get; set; }
    public string MethodName { get; set; }
    public string Parameters { get; set; }
    public DateTime ExecutionTime { get; set; }
    public int ExecutionDuration { get; set; }
}
```

```c#
namespace Sample.Auditing;

[Serializable]
public class EntityChangeInfoDto
{
    public DateTime ChangeTime { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public EntityChangeType ChangeType { get; set; }
    public Guid? EntityTenantId { get; set; }
    public string EntityId { get; set; }
    public string EntityTypeFullName { get; set; }
    public ICollection<EntityPropertyChangeInfoDto> PropertyChanges { get; set; }
}
```

```c#
namespace Sample.Auditing;

[Serializable]
public class EntityPropertyChangeInfoDto
{
    public string NewValue { get; set; }
    public string OriginalValue { get; set; }
    public string PropertyName { get; set; }
    public string PropertyTypeFullName { get; set; }
}
```

```c#
namespace Sample.Auditing;

[Serializable]
public class GetAuditLogsInput : PagedAndSortedResultRequestDto
{
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string HttpMethod { get; set; }
    public string Url { get; set; }
    public Guid? UserId { get; set; }
    public string UserName { get; set; }
    public string ApplicationName { get; set; }
    public string ClientIpAddress { get; set; }
    public string CorrelationId { get; set; }
    public int? MaxExecutionDuration { get; set; }
    public int? MinExecutionDuration { get; set; }
    public bool? HasException { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public HttpStatusCode? HttpStatusCode { get; set; }
    public string EntityTypeFullName { get; set; }
    public string EntityId { get; set; }
    public Guid? TenantId { get; set; }
    public Guid? EntityTenantId { get; set; }
}
```
and implement it in the application layer to expose auditing APIs: 
```c#
namespace Sample.Auditing;

public class AuditLogAppService : ApplicationService, IAuditLogAppService
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IGuidGenerator _guidGenerator;
    private readonly IRepository<AuditLog> _repository;

    public AuditLogAppService(
        IAuditLogRepository auditLogRepository,
        IGuidGenerator guidGenerator,
        IRepository<AuditLog> repository)
    {
        _auditLogRepository = auditLogRepository;
        _guidGenerator = guidGenerator;
        _repository = repository;
    }

    [Authorize]
    [DisableAuditing]
    [UnitOfWork(false, IsDisabled = true)]
    public Task CreateAsync(AuditLogInfoDto auditLogInfo)
    {
        // https://github.com/abpframework/abp/blob/dev/modules/audit-logging/src/Volo.Abp.AuditLogging.Domain/Volo/Abp/AuditLogging/AuditLogInfoToAuditLogConverter.cs

        var auditLog = MapAuditLog(auditLogInfo);
        return _auditLogRepository.InsertAsync(auditLog, true);
    }

    protected virtual AuditLog MapAuditLog(AuditLogInfoDto auditLogInfo)
    {
        var auditLogId = _guidGenerator.Create();
        var constructor = typeof(AuditLog)
            .GetConstructor(BindingFlags.NonPublic | BindingFlags.CreateInstance | BindingFlags.Instance, null, new Type[0], null);

        if (constructor == null)
        {
            throw new Exception("Failed to find constructor in AuditLog");
        }

        var entityChanges = auditLogInfo
                                .EntityChanges?
                                .Select(entityChangeInfo => MapEntityChange(auditLogId, auditLogInfo.TenantId, entityChangeInfo))
                                .ToList()
                            ?? new List<EntityChange>();

        var actions = auditLogInfo
                          .Actions?
                          .Select(auditLogActionInfo => MapLogAction(auditLogId, auditLogInfo.TenantId, auditLogActionInfo))
                          .ToList()
                      ?? new List<AuditLogAction>();

        var auditLog = (AuditLog)constructor.Invoke(Array.Empty<object>());

        SetReflectionProperty(auditLog, "Id", auditLogId);
        SetReflectionProperty(auditLog, "ApplicationName", auditLogInfo.ApplicationName.Truncate(AuditLogConsts.MaxApplicationNameLength));
        SetReflectionProperty(auditLog, "TenantId", auditLogInfo.TenantId);
        SetReflectionProperty(auditLog, "TenantName", auditLogInfo.TenantName);
        SetReflectionProperty(auditLog, "UserId", auditLogInfo.UserId);
        SetReflectionProperty(auditLog, "UserName", auditLogInfo.UserName);
        SetReflectionProperty(auditLog, "ExecutionTime", auditLogInfo.ExecutionTime);
        SetReflectionProperty(auditLog, "ExecutionDuration", auditLogInfo.ExecutionDuration);
        SetReflectionProperty(auditLog, "ClientIpAddress", auditLogInfo.ClientIpAddress.Truncate(AuditLogConsts.MaxClientIpAddressLength));
        SetReflectionProperty(auditLog, "ClientName", auditLogInfo.ClientName.Truncate(AuditLogConsts.MaxClientNameLength));
        SetReflectionProperty(auditLog, "ClientId", auditLogInfo.ClientId.Truncate(AuditLogConsts.MaxClientIdLength));
        SetReflectionProperty(auditLog, "CorrelationId", auditLogInfo.CorrelationId.Truncate(AuditLogConsts.MaxCorrelationIdLength));
        SetReflectionProperty(auditLog, "BrowserInfo", auditLogInfo.BrowserInfo.Truncate(AuditLogConsts.MaxBrowserInfoLength));
        SetReflectionProperty(auditLog, "HttpMethod", auditLogInfo.HttpMethod.Truncate(AuditLogConsts.MaxHttpMethodLength));
        SetReflectionProperty(auditLog, "Url", auditLogInfo.Url.Truncate(AuditLogConsts.MaxUrlLength));
        SetReflectionProperty(auditLog, "HttpStatusCode", auditLogInfo.HttpStatusCode);
        SetReflectionProperty(auditLog, "ImpersonatorUserId", auditLogInfo.ImpersonatorUserId);
        SetReflectionProperty(auditLog, "ImpersonatorTenantId", auditLogInfo.ImpersonatorTenantId);
        SetReflectionProperty(auditLog, "EntityChanges", entityChanges);
        SetReflectionProperty(auditLog, "Actions", actions);
        SetReflectionProperty(auditLog, "Exceptions", auditLogInfo.Exceptions);
        SetReflectionProperty(auditLog, "Comments", auditLogInfo.Comments);

        return auditLog;
    }

    protected virtual AuditLogAction MapLogAction(Guid auditLogId, Guid? tenantId, AuditLogActionInfoDto auditLogActionInfo)
    {
        var constructor = typeof(AuditLogAction)
            .GetConstructor(BindingFlags.NonPublic | BindingFlags.CreateInstance | BindingFlags.Instance, null, new Type[0], null);

        if (constructor == null)
        {
            throw new Exception("Failed to find constructor in AuditLogAction");
        }

        var auditLogAction = (AuditLogAction)constructor.Invoke(new object[0]);
        SetReflectionProperty(auditLogAction, "Id", _guidGenerator.Create());
        SetReflectionProperty(auditLogAction, "TenantId", tenantId);
        SetReflectionProperty(auditLogAction, "AuditLogId", auditLogId);
        SetReflectionProperty(auditLogAction, "ExecutionTime", auditLogActionInfo.ExecutionTime);
        SetReflectionProperty(auditLogAction, "ExecutionDuration", auditLogActionInfo.ExecutionDuration);
        SetReflectionProperty(auditLogAction, "ServiceName", auditLogActionInfo.ServiceName.TruncateFromBeginning(AuditLogActionConsts.MaxServiceNameLength));
        SetReflectionProperty(auditLogAction, "MethodName", auditLogActionInfo.MethodName.TruncateFromBeginning(AuditLogActionConsts.MaxMethodNameLength));
        SetReflectionProperty(auditLogAction, "Parameters", auditLogActionInfo.Parameters.Length > AuditLogActionConsts.MaxParametersLength ? "" : auditLogActionInfo.Parameters);

        return auditLogAction;
    }


    protected virtual EntityChange MapEntityChange(Guid auditLogId, Guid? tenantId, EntityChangeInfoDto entityChangeInfo)
    {
        var constructor = typeof(EntityChange)
            .GetConstructor(BindingFlags.NonPublic | BindingFlags.CreateInstance | BindingFlags.Instance, null, new Type[0], null);

        if (constructor == null)
        {
            throw new Exception("Failed to find constructor in EntityChange");
        }

        var id = _guidGenerator.Create();

        var propertyChanges = entityChangeInfo
                                  .PropertyChanges?
                                  .Select(p => MapEntityPropertyChange(id, tenantId, p))
                                  .ToList()
                              ?? new List<EntityPropertyChange>();

        var entityChange = (EntityChange)constructor.Invoke(Array.Empty<object>());
        SetReflectionProperty(entityChange, "Id", id);
        SetReflectionProperty(entityChange, "TenantId", tenantId);
        SetReflectionProperty(entityChange, "AuditLogId", auditLogId);
        SetReflectionProperty(entityChange, "ChangeTime", entityChangeInfo.ChangeTime);
        SetReflectionProperty(entityChange, "ChangeType", entityChangeInfo.ChangeType);
        SetReflectionProperty(entityChange, "EntityId", entityChangeInfo.EntityId.Truncate(EntityChangeConsts.MaxEntityTypeFullNameLength));
        SetReflectionProperty(entityChange, "EntityTypeFullName", entityChangeInfo.EntityTypeFullName.TruncateFromBeginning(EntityChangeConsts.MaxEntityTypeFullNameLength));
        SetReflectionProperty(entityChange, "PropertyChanges", propertyChanges);

        return entityChange;
    }

    protected virtual EntityPropertyChange MapEntityPropertyChange(Guid entityChangeId, Guid? tenantId, EntityPropertyChangeInfoDto entityPropertyChangeInfo)
    {
        var constructor = typeof(EntityPropertyChange)
            .GetConstructor(BindingFlags.NonPublic | BindingFlags.CreateInstance | BindingFlags.Instance, null, new Type[0], null);

        if (constructor == null)
        {
            throw new Exception("Failed to find constructor in EntityPropertyChange");
        }

        var entityChange = (EntityPropertyChange)constructor.Invoke(Array.Empty<object>());
        SetReflectionProperty(entityChange, "Id", _guidGenerator.Create());
        SetReflectionProperty(entityChange, "TenantId", tenantId);
        SetReflectionProperty(entityChange, "EntityChangeId", entityChangeId);
        SetReflectionProperty(entityChange, "NewValue", entityPropertyChangeInfo.NewValue.Truncate(EntityPropertyChangeConsts.MaxNewValueLength));
        SetReflectionProperty(entityChange, "OriginalValue", entityPropertyChangeInfo.OriginalValue.Truncate(EntityPropertyChangeConsts.MaxOriginalValueLength));
        SetReflectionProperty(entityChange, "PropertyName", entityPropertyChangeInfo.PropertyName.TruncateFromBeginning(EntityPropertyChangeConsts.MaxPropertyNameLength));
        SetReflectionProperty(entityChange, "PropertyTypeFullName", entityPropertyChangeInfo.PropertyTypeFullName.TruncateFromBeginning(EntityPropertyChangeConsts.MaxPropertyTypeFullNameLength));

        return entityChange;
    }

    private void SetReflectionProperty(object o, string propertyName, object value)
    {
        var method = o.GetType()
            .GetProperty(propertyName, BindingFlags.Public
                               | BindingFlags.NonPublic
                               | BindingFlags.Static
                               | BindingFlags.Instance)?
            .GetSetMethod(true);

        if (method == null)
        {
            throw new Exception($"Failed to find property setter for: {o.GetType()}.{propertyName}");
        }

        method.Invoke(o, new object[] { value });
    }
}
```

### Creating the Remoting Module
The remoting module is responsible for centralizing permissions, tenants and auditing. It needs to be added to each microservice. 

Create a new project called `Sample.Remoting` and add the following packages:  
* `Microsoft.AspNetCore.Http.Abstractions`
* `Volo.Abp.AspNetCore.Mvc.Contracts`
* `Volo.Abp.Auditing`
* `Volo.Abp.Authorization.Abstractions`
* `Volo.Abp.ExceptionHandling`
* `Volo.Abp.Http.Client`
* `Volo.Abp.ObjectMapping`

After that create the following options for configuring the module:
```c#
namespace Sample.Remoting;

public class RemotingOptions
{
    public bool UseAuthentication { get; set; } = true;
    public bool UseRemoteAuditing { get; set; } = true;
    public bool UseRemotePermissionChecks { get; set; } = true;
}
```

#### Authenticating with IdentityServer
To implement authentication, we will create a `IRemotingTokenStore` service which will retrieve and cache the JWT token for the microservice. This service will also automatically renew the tokens before expiration.

First create the configuration for configuring the client:
```c#
namespace Sample.Remoting;

public class RemotingClientOptions
{
    public string ClientName { get; set; }
    public string ClientSecret { get; set; }
    public string Scope { get; set; }
}
```

After that create and implement the service for retrieving the auth token from IdentityServer:
```c#
namespace Sample.Remoting;

public interface IRemotingTokenStore
{
    Task<string> GetTokenAsync(CancellationToken cancellationToken);
}
```

```c#
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
```
#### Integrating Authentication for Dynamic Http Clients
Implement a  `IRemoteServiceHttpClientAuthenticator` so that [dynamic http clients](https://docs.abp.io/en/abp/4.4/API/Dynamic-CSharp-API-Clients) use the bearer token:

```c#
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

        context.Request.Headers.Authorization = null;
        var authHeader = $"Bearer {await _tokenStore.GetTokenAsync(cancellationToken)}";
        if (!string.IsNullOrEmpty(authHeader))
        {
            context.Request.Headers.Add("Authorization", authHeader);
        }
    }
}
```

#### Audit Logging
First add a reference to the auditing application contracts layer as we need to call the auditing API for storing audit logs. After that implement the following custom `IAuditingStore` which redirects all audit logs to the auditing service:

```c#
namespace Sample.Remoting;

public class SampleAuditingStore : IAuditingStore
{
    private readonly IExceptionToErrorInfoConverter _exceptionToErrorInfoConverter;
    private readonly IAuditLogAppService _auditLogAppService;

    public SampleAuditingStore(
        IExceptionToErrorInfoConverter exceptionToErrorInfoConverter, 
        IAuditLogAppService auditLogAppService)
    {
        _exceptionToErrorInfoConverter = exceptionToErrorInfoConverter;
        _auditLogAppService = auditLogAppService;
    }

    public Task SaveAsync(AuditLogInfo auditInfo)
    {
        var auditInfoDto = MapAuditInfo(auditInfo);
        return _auditLogAppService.CreateAsync(auditInfoDto);
    }

    protected virtual AuditLogInfoDto MapAuditInfo(AuditLogInfo auditInfoDto)
    {
        var remoteServiceErrorInfos = auditInfoDto.Exceptions?
                                          .Select(exception =>
                                              _exceptionToErrorInfoConverter.Convert(exception, true))
                                          .ToList()
                                      ?? new List<RemoteServiceErrorInfo>();


        return new()
        {
            Actions = auditInfoDto.Actions.Select(MapActions).ToList(),
            Comments = auditInfoDto.Comments.JoinAsString(Environment.NewLine),
            TenantId = auditInfoDto.TenantId,
            ExecutionTime = auditInfoDto.ExecutionTime,
            UserName = auditInfoDto.UserName,
            UserId = auditInfoDto.UserId,
            HttpStatusCode = auditInfoDto.HttpStatusCode,
            Url = auditInfoDto.Url,
            Exceptions = remoteServiceErrorInfos.Any()
                ? JsonSerializer.Serialize(remoteServiceErrorInfos, new JsonSerializerOptions
                {
                    WriteIndented = true
                })
                : null,
            ApplicationName = auditInfoDto.ApplicationName,
            BrowserInfo = auditInfoDto.BrowserInfo,
            ClientId = auditInfoDto.ClientId,
            ClientIpAddress = auditInfoDto.ClientIpAddress,
            ClientName = auditInfoDto.ClientName,
            CorrelationId = auditInfoDto.CorrelationId,
            EntityChanges = auditInfoDto.EntityChanges.Select(MapEntityChanges).ToList(),
            ExecutionDuration = auditInfoDto.ExecutionDuration,
            HttpMethod = auditInfoDto.HttpMethod,
            TenantName = auditInfoDto.TenantName,
            ImpersonatorTenantId = auditInfoDto.ImpersonatorTenantId,
            ImpersonatorUserId = auditInfoDto.ImpersonatorUserId,
        };
    }

    protected virtual AuditLogActionInfoDto MapActions(AuditLogActionInfo action)
    {
        return new()
        {
            ExecutionTime = action.ExecutionTime,
            ExecutionDuration = action.ExecutionDuration,
            MethodName = action.MethodName,
            Parameters = action.Parameters,
            ServiceName = action.ServiceName
        };
    }

    protected virtual EntityChangeInfoDto MapEntityChanges(EntityChangeInfo entityChange)
    {
        return new()
        {
            ChangeTime = entityChange.ChangeTime,
            ChangeType = entityChange.ChangeType,
            EntityId = entityChange.EntityId,
            EntityTenantId = entityChange.EntityTenantId,
            EntityTypeFullName = entityChange.EntityTypeFullName,
            PropertyChanges = entityChange.PropertyChanges.Select(MapPropertyChanges).ToList()
        };
    }

    private EntityPropertyChangeInfoDto MapPropertyChanges(EntityPropertyChangeInfo propertyChange)
    {
        return new()
        {
            NewValue = propertyChange.NewValue,
            OriginalValue = propertyChange.OriginalValue,
            PropertyName = propertyChange.PropertyName,
            PropertyTypeFullName = propertyChange.PropertyTypeFullName
        };
    }
}
```

#### Resolving tenants
Implement the following `ITenantStore` to resolve tenants from IdentityServer:
```c#
namespace Sample.Remoting;

public class SampleTenantStore : ITenantStore
{
    private readonly IAbpTenantAppService _tenantAppService;

    public SampleTenantStore(
        IAbpTenantAppService tenantAppService)
    {
        _tenantAppService = tenantAppService;
    }

    public async Task<TenantConfiguration> FindAsync(string name)
    {
        var result = await _tenantAppService.FindTenantByNameAsync(name);
        if (!result.Success)
        {
            return null;
        }

        return new TenantConfiguration(result.TenantId!.Value, result.Name);
    }

    public async Task<TenantConfiguration> FindAsync(Guid id)
    {
        var result = await _tenantAppService.FindTenantByIdAsync(id);
        if (!result.Success)
        {
            return null;
        }

        return new TenantConfiguration(result.TenantId!.Value, result.Name);
    }

    public TenantConfiguration Find(string name)
    {
        return AsyncHelper.RunSync(() => FindAsync(name));
    }

    public TenantConfiguration Find(Guid id)
    {
        return AsyncHelper.RunSync(() => FindAsync(id));
    }
} 
```

#### Checking Permissions
Add a reference to the identity application contracts project. After that implement the following `IPermissionChecker` to redirect permission checks to IdentityServer: 
```c#
namespace Sample.Remoting;

public class SamplePermissionChecker : IPermissionChecker
{
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<SamplePermissionChecker> _logger;
    private readonly IPermissionCheckerAppService _permissionAppService;
    private readonly IObjectMapper _mapper;

    public SamplePermissionChecker(
        ICurrentUser currentUser,
        ILogger<SamplePermissionChecker> logger,
        IPermissionCheckerAppService permissionAppService,
        IObjectMapper mapper)
    {
        _currentUser = currentUser;
        _logger = logger;
        _permissionAppService = permissionAppService;
        _mapper = mapper;
    }

    public async Task<bool> IsGrantedAsync(ClaimsPrincipal claimsPrincipal, string name)
    {
        var clientId = claimsPrincipal.FindClientId();
        var userId = claimsPrincipal.FindUserId()?.ToString();

        _logger.LogInformation($"Checking permission {name} for principal {userId}/{clientId}");

        return await _permissionAppService.CheckPermissionAsync(new CheckPermissionInput
        {
            Name = name,
            Type = userId == null && clientId != null ? "Client" : "User",
            Id = claimsPrincipal.FindUserId()?.ToString() ?? clientId,
        });
    }

    public async Task<bool> IsGrantedAsync(string name)
    {
        _logger.LogInformation($"Checking permission {name} for principal {_currentUser.Id}");

        return await _permissionAppService.CheckPermissionAsync(new CheckPermissionInput
        {
            Name = name,
            Type = "User",
            Id = _currentUser.Id?.ToString(),
        });
    }

    public async Task<MultiplePermissionGrantResult> IsGrantedAsync(string[] names)
    {
        _logger.LogInformation($"Checking permission {string.Join(", ", names)} for {_currentUser.Id}");

        var result = await _permissionAppService.CheckPermissionsAsync(new CheckPermissionsInput
        {
            Names = names,
            Type = "User",
            Id = _currentUser.Id?.ToString(),
        });

        return _mapper.Map<MultiplePermissionGrantResultDto, MultiplePermissionGrantResult>(result);
    }

    public async Task<MultiplePermissionGrantResult> IsGrantedAsync(ClaimsPrincipal claimsPrincipal, string[] names)
    {
        var clientId = claimsPrincipal.FindClientId();
        var userId = claimsPrincipal.FindUserId()?.ToString();

        _logger.LogInformation($"Checking permission {string.Join(", ", names)} for principal {userId}/{clientId}");

        var result = await _permissionAppService.CheckPermissionsAsync(new CheckPermissionsInput
        {
            Names = names,
            Type = userId == null && clientId != null ? "Client" : "User",
            Id = claimsPrincipal.FindUserId()?.ToString() ?? clientId
        });

        return _mapper.Map<MultiplePermissionGrantResultDto, MultiplePermissionGrantResult>(result);
    }
}
```

#### Creating the Remoting ABP Module
Finally create the remoting module and add 
it to all microservices:
```c#
namespace Sample.Remoting;

[DependsOn(
    typeof(IdentityApplicationContractsModule),
    typeof(AuditingApplicationContractsModule))]
public class RemotingModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {

        var configuration = context.Services.GetConfiguration();
        Configure<RemotingClientOptions>(configuration.GetSection("Remoting:Client"));

        var options = context.Services.ExecutePreConfiguredActions<RemotingOptions>();

        if (options.UseRemoteAuditing)
        {
            context.Services.AddHttpClientProxies(
                typeof(AuditingApplicationContractsModule).Assembly,
                remoteServiceConfigurationName: "Auditing"
            );

            context.Services.Replace(ServiceDescriptor.Transient<IAuditingStore, SampleAuditingStore>());
        }

        if (options.UseRemotePermissionChecks)
        {
            context.Services.AddHttpClientProxies(
                typeof(IdentityApplicationContractsModule).Assembly,
                remoteServiceConfigurationName: "Identity"
            );

            context.Services.AddHttpClientProxies(
                typeof(AbpAspNetCoreMvcContractsModule).Assembly,
                remoteServiceConfigurationName: "Identity"
            );


            context.Services.Replace(ServiceDescriptor.Transient<ITenantStore, SampleTenantStore>());
            context.Services.Replace(ServiceDescriptor.Transient<IPermissionChecker, SamplePermissionChecker>());
        }
    }
}
```

### Configuring IdentityServer
In Identity Server, create a client for each microservice. Make sure that the grant type is set to client credentials.

### Configuring the Microservices
For each microservice add the following to the `appsettings.json`:
```json
{
   ...
   "AuthServer": {
       "Authority": "https://..." # Set to identity service
   },
   "RemoteServices": {
      "Identity": { # Not needed on identity service
          "BaseUrl": "https://..."
      }
      "Auditing": { # Not needed on auditing service
          "BaseUrl": "https://..." 
      }
   },
   "Remoting": {
      "Client": {
          "ClientName": "XXXXX",
          "ClientSecret": "XXXXX",
          "Scope": "XXXXX"
      }
   }
}
```

For the identity microservice, disable remote permission checks in the host module:
```c#
public override void PreConfigureServices(ServiceConfigurationContext context)
{
    PreConfigure<RemotingOptions>(o => o.UseRemotePermissionChecks = false);
}
```

Similarly, disable remote auditing for the auditing microservice:
```c#
public override void PreConfigureServices(ServiceConfigurationContext context)
{
    PreConfigure<RemotingOptions>(o => o.UseRemoteAuditing = false);
}
```

## Conclusion
We have created a zero trust microservice project where each microservice has it's own identity and permissions. All permissions and tenants are configured centralized in the identity service and all audit logs get saved in the auditing service's database.

### Caveats
* **Lot's of dependencies**: The identity service will have a dependency to all application contracts layers. You will have to recompile and redeploy the identity service every time a new microservice or permission is added. Otherwise, the permissions will not be registered and an exception will be thrown on permission check. 
	* You could integrate service auto-discovery in combination with the [Abp.DynamicPermissions](https://github.com/EasyAbp/Abp.DynamicPermission) library to fix this issue (e.g. by providing an API that lists all permission definitions in each service).
* **Difficult debugging**: On missing permissions, the remote exception is not forwarded correctly. You will only see a generic authorization exception but not which permission is missing. 
	* This issue could be fixed by forwarding these exceptions correctly.
* **A new single point of failure**: if the identity service goes down, nothing will work as authorization will not be possible.

**Note**: In this article we did not look into centralizing feature management and setting management. 
* You can easily centralize them by implementing `IFeatureStore` and `ISettingStore` in the remoting module and redirecting them to the identity service.

## Source Code
The sample source code is available [here](https://github.com/Trojaner/AbpZeroTrustArchitecture).
