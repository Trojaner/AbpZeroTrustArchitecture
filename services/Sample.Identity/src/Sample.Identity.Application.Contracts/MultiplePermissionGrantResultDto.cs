namespace Sample.Identity;

using System;
using System.Collections.Generic;
using Volo.Abp.Authorization.Permissions;

[Serializable]
public class MultiplePermissionGrantResultDto
{
    public Dictionary<string, PermissionGrantResult> Result { get; set; }
}