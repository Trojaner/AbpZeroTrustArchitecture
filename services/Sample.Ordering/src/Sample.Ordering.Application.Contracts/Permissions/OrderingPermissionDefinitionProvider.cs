using Sample.Ordering.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace Sample.Ordering.Permissions;

public class OrderingPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(OrderingPermissions.GroupName);
        //Define your own permissions here. Example:
        //myGroup.AddPermission(OrderingPermissions.MyPermission1, L("Permission:MyPermission1"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<OrderingResource>(name);
    }
}
