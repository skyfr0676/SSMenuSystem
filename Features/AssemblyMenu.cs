// -----------------------------------------------------------------------
// <copyright file="AssemblyMenu.cs" company="Skyfr0676 and Redforce04">
// Copyright (c) Skyfr0676 and Redforce04. All rights reserved.
// Licensed under the Undetermined license.
// </copyright>
// -----------------------------------------------------------------------

#pragma warning disable SA1010, SA1011 // Opening square brackets should be spaced correctly. Closing square bracket should be followed by a space.
namespace SSMenuSystem.Features;

using System.Collections.Generic;
using System.Reflection;

using UserSettings.ServerSpecific;

/// <summary>
/// The Assembly Menu.
/// </summary>
internal class AssemblyMenu : Menu
{
    /// <summary>
    /// Gets the server specific settings.
    /// </summary>
    public override ServerSpecificSettingBase[] Settings => this.OverrideSettings ?? [];

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    public override string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the id of the menu.
    /// </summary>
    public override int Id { get; set; }

    /// <summary>
    /// Gets or Sets the dictionary of server specific settings actually sent to the client.
    /// </summary>
    internal Dictionary<ReferenceHub, ServerSpecificSettingBase[]> ActuallySentToClient { get; set; } = new ();

    /// <summary>
    /// Gets or sets the Assembly.
    /// </summary>
    internal Assembly? Assembly { get; set; }

    /// <summary>
    /// Gets or sets the override settings.
    /// </summary>
    internal ServerSpecificSettingBase[]? OverrideSettings { get; set; }

    /// <summary>
    /// Checks access for a specific player.
    /// </summary>
    /// <param name="hub">The player to check.</param>
    /// <returns>True if the player should have access otherwise false.</returns>
    public override bool CheckAccess(ReferenceHub hub) =>
        (this.ActuallySentToClient.TryGetValue(hub, out ServerSpecificSettingBase[] settings) && settings != null && !settings.IsEmpty()) ||
        (this.OverrideSettings != null && !this.OverrideSettings.IsEmpty());
}

#pragma warning restore SA1010, SA1011
