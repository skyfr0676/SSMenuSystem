// -----------------------------------------------------------------------
// <copyright file="Config.cs" company="Skyfr0676 and Redforce04">
// Copyright (c) Skyfr0676 and Redforce04. All rights reserved.
// Licensed under the Undetermined license.
// </copyright>
// -----------------------------------------------------------------------

namespace SSMenuSystem;

using System.ComponentModel;

#if EXILED
using Exiled.API.Interfaces;
#endif
using Features;

/// <inheritdoc cref="IConfig"/>
// ReSharper disable UnusedAutoPropertyAccessor.Global
public class Config
#if EXILED
    : IConfig
#endif
{
    /// <summary>
    /// Gets or sets a value indicating whether the plugin should be enabled.
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the plugin should display debugging logs.
    /// </summary>
    public bool Debug { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether pins is allowed or not (pin is a thing that has been displayed on all menus).
    /// </summary>
    [Description("Whether pins is allowed or not (pin is a thing that has been displayed on all menus).")]
    public bool AllowPinnedContent { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether clients (= non-moderators) should see errors or not.
    /// </summary>
    [Description("Whether clients (= non-moderators) should see errors or not.")]
    public bool ShowErrorToClient { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether clients (= non-moderators) should see total errors (= some plugins-content name) or not. It is advised to leave this as false.
    /// </summary>
    [Description("Whether clients (= non-moderators) should see total errors (= some plugins-content name) or not. It is advised to leave this as false.")]
    public bool ShowFullErrorToClient { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether moderators (= has RA access) should see total errors (= some plugins-content name).
    /// </summary>
    [Description("Whether moderators (= has RA access) should see total errors (= some plugins-content name).")]
    public bool ShowFullErrorToModerators { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether singularly registered menus should be automatically shown.
    /// </summary>
    [Description("Indicates whether singularly registered menus should be automatically shown.")]
    public bool ForceMainMenuEvenIfOnlyOne { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether examples is enabled. Warning: if set to true, some content of examples would be Game breaking (speed ability, scan ability, etc...)
    /// </summary>
    [Description("Whether examples is enabled. Warning: if set to true, some content of examples would be Game breaking (speed ability, scan ability, etc...).")]
    public bool EnableExamples { get; set; } = true;

    /// <summary>
    /// Gets or sets the compatibility system config.
    /// </summary>
    public CompatibilityConfig CompatibilitySystem { get; set; } = new ();

#if !EXILED
    /// <summary>
    /// Gets or sets the plugin's translations.
    /// </summary>
    [Description("Translations for the plugin.")]
    public Translation Translation { get; set; } = new ();
#endif
}