using System;

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