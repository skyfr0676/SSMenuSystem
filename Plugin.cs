using Exiled.API.Enums;
using HarmonyLib;
using ServerSpecificSyncer.Features;
using UserSettings.ServerSpecific;
using Log = PluginAPI.Core.Log;
#if EXILED
using System;
using Exiled.API.Features;
#elif NWAPI
using PluginAPI.Core;
#endif

namespace ServerSpecificSyncer
{
    /// <summary>
    /// Load the plugin to send datas to player
    /// </summary>
    public class Plugin
#if EXILED
        : Plugin<Config, Translation>
#endif
    {
#if EXILED
        /// <summary>
        /// Gets the author of the plugin.
        /// </summary>
        public override string Author => "Sky";

        /// <summary>
        /// Gets the name shown in the Loader.
        /// </summary>
        public override string Name => "ServerSpecificSyncer";
        
        /// <summary>
        /// Gets the version of the plugin.
        /// </summary>
        public override Version Version => new(1, 0, 3);
        
        /// <summary>
        /// Gets the prefix used for configs.
        /// </summary>
        public override string Prefix => "ss_syncer";

        /// <summary>
        /// Gets the plugin priority.
        /// </summary>
        public override PluginPriority Priority => PluginPriority.First;

#endif

        private static Config _staticConfig;
        
        /// <summary>
        /// Gets the <see cref="Config"/> instance. Can be null if the plugin is not enabled.
        /// </summary>
        public static Config StaticConfig => _staticConfig ??= Instance?.Config;
        
        /// <summary>
        /// Gets the <see cref="Plugin"/> instance. can be null if the plugin is not enabled.
        /// </summary>
        public static Plugin Instance { get; private set; }
        
        private Harmony _harmony;
        
#if EXILED
        /// <summary>
        /// When the plugin is enabled.
        /// </summary>
        public override void OnEnabled()
        {
            Exiled.Events.Handlers.Player.Verified += EventHandler.Verified;
            Exiled.Events.Handlers.Player.Left += EventHandler.Left;
            Exiled.Events.Handlers.Player.ChangingGroup += EventHandler.ChangingGroup;
            GenericEnable();
            base.OnEnabled();
        }
        
#elif NWAPI
        [PluginAPI.Core.Attributes.PluginConfig("config.yml")]
        public Config Config;
        
        [PluginAPI.Core.Attributes.PluginEntryPoint("ServerSpecificSyncer", "1.0.3", "sync all plugins to one server specific", "sky")]
        public void OnEnabled()
        {
            if (Config == null)
            {
                Log.Error("can't load plugin, because Config is malformed/invalid.");
                return;
            }

            if (!Config.IsEnabled)
                return;
            
            PluginAPI.Events.EventManager.RegisterEvents<EventHandler>(this);
            GenericEnable();
            Log.Info("ServerSpecificSyncer@1.0.0 has been enabled!");
        }
#endif
        private void GenericEnable()
        {
            Instance = this;

#if DEBUG
            Log.Warning("EXPERIMENTAL VERSION IS ACTIVATED. BE AWARD OF BUGS CAN BE DONE. NOT STABLE VERSION.");
            Menu.RegisterAll();
            Menu.RegisterPin(new ServerSpecificSettingBase[]{new SSTextArea(null, "this pinned content is related to the called assembly\nwith Menu.UnregisterPin() you just unregister ONLY pinned settings by the called assembly.", SSTextArea.FoldoutMode.CollapsedByDefault, "This is a pinned content.")});
            StaticConfig.Debug = true;
#endif

            _harmony = new Harmony("fr.sky.patches");
            _harmony.PatchAll();
            ServerSpecificSettingsSync.ServerOnSettingValueReceived += EventHandler.OnReceivingInput;
        }

        /// <summary>
        /// Get the loaded Translations, depending on using EXILED or NWAPI.
        /// </summary>
        /// <returns>The loaded translation. Can be null if the plugin is not enabled.</returns>
        public static Translation GetTranslation()
        {
#if EXILED
            return Instance?.Translation;
#elif NWAPI
            return StaticConfig?.Translation;
#endif
        }
    }
}