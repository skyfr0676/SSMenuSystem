// -----------------------------------------------------------------------
// <copyright file="Plugin.cs" company="Skyfr0676 and Redforce04">
// Copyright (c) Skyfr0676 and Redforce04. All rights reserved.
// Licensed under the Undetermined license.
// </copyright>
// -----------------------------------------------------------------------

namespace SSMenuSystem;

using System;
#if EXILED
using Exiled.API.Features;
#endif
using Features;
using HarmonyLib;
using LabApi.Events.CustomHandlers;
#if LABAPI
using LabApi.Features;
using LabApi.Loader;
using LabApi.Loader.Features.Plugins;
#endif
using UserSettings.ServerSpecific;
using Log = Features.Log;

/// <summary>
/// Load the plugin to send data to player.
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global
#if EXILED
public class Plugin : Plugin<Config, Translation>
#else
public class Plugin : Plugin<Config>
#endif
{
    private Harmony? harmony;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private EventHandler handler;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    /// <summary>
    /// Gets the <see cref="Plugin"/> instance. can be null if the plugin is not enabled.
    /// </summary>
    public static Plugin? Instance { get; private set; }

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
    public override Version Version => new (2, 0, 7);

#if EXILED
    /// <inheritdoc/>
    public override Version RequiredExiledVersion => new(9, 5, 0);

    /// <summary>
    /// Gets the prefix used for configs.
    /// </summary>
    public override string Prefix => "ss_menu_system";

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
    /// <summary>
    /// Gets the plugin translations.
    /// </summary>
    public Translation? Translation { get; private set; }

    /// <inheritdoc />
    public override string Description => "Convert all Server-Specifics Settings created by plugins into menu. Help for multi-plugin comptability and organization.";

    /// <inheritdoc />
    public override Version RequiredApiVersion => LabApiProperties.CurrentVersion;

    /// <inheritdoc />
    public override void Enable()
    {
        if (this.Config == null)
        {
            Log.Error("can't load plugin, because Config is malformed/invalid.");
            return;
        }

        if (!this.Config.IsEnabled)
        {
            return;
        }

        this.handler = new EventHandler();
        if (!this.TryLoadConfig("translation.yml", out Translation? translation))
        {
            Log.Error("There is an error while loading translation. Using default one.");
            translation = new ();
        }

        this.Translation = translation;
        CustomHandlersManager.RegisterEventsHandler(this.handler);

        this.GenericEnable();
    }

    /// <inheritdoc />
    public override void Disable()
    {
        this.GenericDisable();
    }
#endif

    private void GenericEnable()
    {
        Menu.RegisterAll();
        Instance = this;
        this.harmony = new Harmony("fr.sky.patches");
        this.harmony.PatchAll();
        this.handler = new EventHandler();
        CustomHandlersManager.RegisterEventsHandler(this.handler);
        Menu.RegisterQueuedAssemblies();

#if DEBUG
        Log.Warn("EXPERIMENTAL VERSION IS ACTIVATED. BE AWARD OF BUGS CAN BE DONE. NOT STABLE VERSION.");
        Menu.RegisterPin([new SSTextArea(null, "this pinned content is related to the called assembly\nwith Menu.UnregisterPin() you just unregister ONLY pinned settings by the called assembly.", SSTextArea.FoldoutMode.CollapsedByDefault, "This is a pinned content.")]);
        this.Config!.Debug = true;
#endif

        ServerSpecificSettingsSync.ServerOnSettingValueReceived += EventHandler.OnReceivingInput;
        Log.Info($"{this.Name}@{this.Version} has been enabled!");
    }

    private void GenericDisable()
    {
        Menu.UnregisterAll();
        CustomHandlersManager.UnregisterEventsHandler(this.handler);
        ServerSpecificSettingsSync.ServerOnSettingValueReceived -= EventHandler.OnReceivingInput;

        Instance = null;
        this.harmony?.UnpatchSelf();
        this.harmony = null;
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        this.handler = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        Log.Info($"{this.Name}@{this.Version} has been disabled!");
    }
}