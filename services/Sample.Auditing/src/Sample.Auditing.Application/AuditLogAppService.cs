using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Services;
using Volo.Abp.Auditing;
using Volo.Abp.AuditLogging;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using BindingFlags = System.Reflection.BindingFlags;
using Volo.Abp.Uow;

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