// -----------------------------------------------------------------------
// <copyright file="MainExample.cs" company="Skyfr0676 and Redforce04">
// Copyright (c) Skyfr0676 and Redforce04. All rights reserved.
// Licensed under the Undetermined license.
// </copyright>
// -----------------------------------------------------------------------

#pragma warning disable SA1010 // brackets.
namespace SSMenuSystem.Examples;

using Features;
using UserSettings.ServerSpecific;

/// <summary>
/// The main example menu.
/// </summary>
internal class MainExample : Menu
{
    /// <inheritdoc/>
    public override ServerSpecificSettingBase[] Settings => [];

    /// <inheritdoc/>
    public override string Name { get; set; } = "Examples";

    /// <inheritdoc/>
    public override int Id { get; set; } = -200987;
}
#pragma warning restore SA1010
