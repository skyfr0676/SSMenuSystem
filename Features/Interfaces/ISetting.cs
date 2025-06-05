// -----------------------------------------------------------------------
// <copyright file="ISetting.cs" company="Skyfr0676 and Redforce04">
// Copyright (c) Skyfr0676 and Redforce04. All rights reserved.
// Licensed under the Undetermined license.
// </copyright>
// -----------------------------------------------------------------------

namespace SSMenuSystem.Features.Interfaces;

using UserSettings.ServerSpecific;

/// <summary>
/// An interface for defining settings.
/// </summary>
internal interface ISetting
{
    /// <summary>
    /// Gets the base <see cref="ServerSpecificSettingBase"/> component for this item.
    /// </summary>
    ServerSpecificSettingBase Base { get; }
}