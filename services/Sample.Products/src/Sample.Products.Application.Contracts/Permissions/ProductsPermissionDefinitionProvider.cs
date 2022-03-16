using Sample.Products.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace Sample.Products.Permissions;

public class ProductsPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(ProductsPermissions.GroupName);
        //Define your own permissions here. Example:
        //myGroup.AddPermission(ProductsPermissions.MyPermission1, L("Permission:MyPermission1"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<ProductsResource>(name);
    }
}
