using Volo.Abp.Settings;

namespace Sample.Ordering.Settings;

public class OrderingSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        //Define your own settings here. Example:
        //context.Add(new SettingDefinition(OrderingSettings.MySetting1));
    }
}
