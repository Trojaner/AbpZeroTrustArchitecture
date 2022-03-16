namespace Sample.Identity;

using System;
using Volo.Abp.Application.Dtos;

[Serializable]
public class CheckPermissionInput : EntityDto
{
    public string Id { get; set; }
    public string Type { get; set; }
    public string Name { get; set; }
}