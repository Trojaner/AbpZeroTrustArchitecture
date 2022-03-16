using Volo.Abp.Settings;

namespace Sample.Auditing.Settings;

public class AuditingSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        //Define your own settings here. Example:
        //context.Add(new SettingDefinition(AuditingSettings.MySetting1));
    }
}
