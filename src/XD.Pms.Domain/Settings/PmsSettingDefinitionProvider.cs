using Volo.Abp.Settings;

namespace XD.Pms.Settings;

public class PmsSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        //Define your own settings here. Example:
        //context.Add(new SettingDefinition(PmsSettings.MySetting1));
    }
}
