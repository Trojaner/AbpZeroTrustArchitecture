using Volo.Abp.Settings;

namespace Sample.Products.Settings;

public class ProductsSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        //Define your own settings here. Example:
        //context.Add(new SettingDefinition(ProductsSettings.MySetting1));
    }
}
