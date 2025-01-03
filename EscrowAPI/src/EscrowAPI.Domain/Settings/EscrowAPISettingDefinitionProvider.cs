using Volo.Abp.Settings;

namespace EscrowAPI.Settings;

public class EscrowAPISettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        //Define your own settings here. Example:
        //context.Add(new SettingDefinition(EscrowAPISettings.MySetting1));
    }
}
