using System;

namespace Sample.Auditing;

[Serializable]
public class EntityPropertyChangeInfoDto
{
    public string NewValue { get; set; }

    public string OriginalValue { get; set; }

    public string PropertyName { get; set; }

    public string PropertyTypeFullName { get; set; }
}