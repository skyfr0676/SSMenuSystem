using PluginAPI.Core;
using ServerSpecificSyncer.Features;
using UserSettings.ServerSpecific;

namespace ServerSpecificSyncer.Test
{
    public class SubMenuTest : Menu
    {
        
        public override ServerSpecificSettingBase[] Settings => new ServerSpecificSettingBase[]
        {
            new SSButton(2, "Black", "This button is not black ?"),
            new SSDropdownSetting(3, "test", new[]{"opt1", "opt2"}),
            new SSTextArea(4, "I want you in my bed")
        };
        public override string Name { get; set; } = "Sub menus test";
        public override int Id { get; set; } = -1;

        public override string Description { get; set; } = "Ceci est un test pour voir si tout foncitonne.";

        public override void OnInput(ReferenceHub hub, ServerSpecificSettingBase setting)
        {
            if (setting.SettingId == 2)
            {
                Player.Get(hub).Kick("t'a appuyé sur le bouton nigga. RETOURNE DANS TON PAYS");
            }
        }
    }
}