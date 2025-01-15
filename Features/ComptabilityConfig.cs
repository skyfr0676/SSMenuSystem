using System.ComponentModel;

namespace ServerSpecificSyncer.Features
{
    public class ComptabilityConfig
    {
        [Description("If enabled, the comptability system will be enabled and all plugins that use SSSystem will be registered as menu.")]
        public bool ComptabilityEnabled { get; set; } = true;

        /*[Description("If enabled, all keybinds on comptability menu will be marked as global (displayed on all screens).")]
        public bool KeybindsAllGlobal { get; set; } = true;*/
    }
}