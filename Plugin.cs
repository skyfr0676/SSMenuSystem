using HarmonyLib;
using ServerSpecificSyncer.Features;
using UserSettings.ServerSpecific;
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
        public override Version Version => new Version(1, 0, 1);
        
        /// <summary>
        /// Gets the prefix used for configs.
        /// </summary>
        public override string Prefix => "ss_syncer";

#endif
        public static Config StaticConfig { get; set; }
        
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
        
        [PluginAPI.Core.Attributes.PluginEntryPoint("ServerSpecificSyncer", "1.0.1", "sync all plugins to one server specific", "sky")]
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
            StaticConfig = Config;
#if DEBUG
            Menu.RegisterAll();
            StaticConfig.Debug = true;
#endif
            _harmony = new Harmony("fr.sky.patches");
            _harmony.PatchAll();
            ServerSpecificSettingsSync.ServerOnSettingValueReceived += EventHandler.OnReceivingInput;
        }

        public static Translation GetTranslation()
        {
#if EXILED
            return Instance.Translation;
#elif NWAPI
            return Instance.Config.Translation;
#endif
        }
    }
}