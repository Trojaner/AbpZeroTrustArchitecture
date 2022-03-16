namespace Sample.Identity;

using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

[Serializable]
public class CheckPermissionsInput : EntityDto
{
    public string Id { get; set; }
    public string Type { get; set; }
    public ICollection<string> Names { get; set; }
}