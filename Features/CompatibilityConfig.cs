// -----------------------------------------------------------------------
// <copyright file="CompatibilityConfig.cs" company="Skyfr0676 and Redforce04">
// Copyright (c) Skyfr0676 and Redforce04. All rights reserved.
// Licensed under the Undetermined license.
// </copyright>
// -----------------------------------------------------------------------

namespace SSMenuSystem.Features;

using System.ComponentModel;

/// <summary>
/// The compatibility configs.
/// </summary>
public sealed class CompatibilityConfig
{
    /// <summary>
    /// Gets or sets a value indicating whether the compatibility system will be enabled and all plugins that use SSSystem will be registered as menu.
    /// </summary>
    [Description("If enabled, the comptability system will be enabled and all plugins that use SSSystem will be registered as menu.")]
    public bool CompatibilityEnabled { get; set; } = true;

    /*[Description("If enabled, all keybinds on compatibility menu will be marked as global (displayed on all screens).")]
    public bool KeybindsAllGlobal { get; set; } = true;*/
}