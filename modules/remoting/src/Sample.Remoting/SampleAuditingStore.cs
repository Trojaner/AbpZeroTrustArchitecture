using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.ExceptionHandling;
using Volo.Abp.Auditing;
using Volo.Abp.Http;

namespace Sample.Remoting;

using Auditing;

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