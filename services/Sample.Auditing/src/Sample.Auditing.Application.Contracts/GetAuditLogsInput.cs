using System;
using System.Net;
using System.Text.Json.Serialization;
using Volo.Abp.Application.Dtos;

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