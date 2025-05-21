using System.Linq;
using System.Reflection;
using HarmonyLib;
using LabApi.Events.CustomHandlers;
using SSMenuSystem.Features;
using UserSettings.ServerSpecific;
using Log = SSMenuSystem.Features.Log;
using System;
using JetBrains.Annotations;
using LabApi.Loader;
#if EXILED
using Exiled.API.Features;
#else
using LabApi.Loader.Features.Plugins;
#endif

namespace SSMenuSystem
{
    /// <summary>
    /// Load the plugin to send datas to player
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Plugin : Plugin<Config
#if EXILED
        , Translation
#endif
        >
    {
        /// <summary>
        /// Gets the author of the plugin.
        /// </summary>
        public override string Author => "Sky";

        /// <summary>
        /// Gets the name shown in the Loader.
        /// </summary>
        public override string Name => "SS-Menu System";

        /// <summary>
        /// Gets the version of the plugin.
        /// </summary>
        public override Version Version => new(2, 0, 7);

#if EXILED
        /// <inheritdoc/>
        public override Version RequiredExiledVersion => new(9, 5, 0);

        /// <summary>
        /// Gets the prefix used for configs.
        /// </summary>
        public override string Prefix => "ss_menu_system";
#else

        /// <summary>
        /// Gets the plugin translations.
        /// </summary>
        public Translation Translation { get; private set; }

        /// <inheritdoc />
        public override string Description => "Convert all Server-Specifics Settings created by plugins into menu. Help for multi-plugin comptability and organization.";

        /// <inheritdoc />
        public override Version RequiredApiVersion => new(1, 0, 0);
#endif

        /// <summary>
        /// Gets the <see cref="Plugin"/> instance. can be null if the plugin is not enabled.
        /// </summary>
        [CanBeNull]
        public static Plugin Instance { get; private set; }

        private Harmony _harmony;
        private EventHandler _handler;

#if EXILED
        /// <inheritdoc/>
        public override void OnEnabled()
        {
            GenericEnable();
            base.OnEnabled();
        }

        /// <inheritdoc />
        public override void OnDisabled()
        {
            GenericDisable();
            base.OnDisabled();
        }

#else

        /// <inheritdoc />
        public override void Enable()
        {
            if (Config == null)
            {
                Log.Error("can't load plugin, because Config is malformed/invalid.");
                return;
            }

            if (!Config.IsEnabled)
                return;

            _handler = new EventHandler();
            if (!this.TryLoadConfig("translation.yml", out Translation translation))
            {
                Log.Error("There is an error while loading translation. Using default one.");
                translation = new();
            }

            Translation = translation;

            CustomHandlersManager.RegisterEventsHandler(_handler);
            GenericEnable();
            Log.Info($"{Name}@{Version} has been enabled!");
        }

        /// <inheritdoc />
        public override void Disable()
        {
            GenericDisable();
            Log.Info($"{Name}@{Version} has been disabled!");
        }
#endif
        private void GenericEnable()
        {
            Menu.RegisterAll();
            Instance = this;
            _harmony = new Harmony("fr.sky.patches");
            _harmony.PatchAll();
            _handler = new EventHandler();
            CustomHandlersManager.RegisterEventsHandler(_handler);
            Menu.RegisterQueuedAssemblies();

#if DEBUG
            Log.Warn("EXPERIMENTAL VERSION IS ACTIVATED. BE AWARD OF BUGS CAN BE DONE. NOT STABLE VERSION.");
            Menu.RegisterPin(new[]{new SSTextArea(null, "this pinned content is related to the called assembly\nwith Menu.UnregisterPin() you just unregister ONLY pinned settings by the called assembly.", SSTextArea.FoldoutMode.CollapsedByDefault, "This is a pinned content.")});
            Config!.Debug = true;
#endif

            ServerSpecificSettingsSync.ServerOnSettingValueReceived += EventHandler.OnReceivingInput;
        }

        private void GenericDisable()
        {
            Menu.UnregisterAll();
            CustomHandlersManager.UnregisterEventsHandler(_handler);
            ServerSpecificSettingsSync.ServerOnSettingValueReceived -= EventHandler.OnReceivingInput;

            Instance = null;
            _harmony.UnpatchAll(_harmony.Id);
            _harmony = null;
            _handler = null;

            Log.Info($"{Name}@{Version} has been disabled!");
        }
    }
}