using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Volo.Abp.Auditing;

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