// -----------------------------------------------------------------------
// <copyright file="Dropdown.cs" company="Skyfr0676 and Redforce04">
// Copyright (c) Skyfr0676 and Redforce04. All rights reserved.
// Licensed under the Undetermined license.
// </copyright>
// -----------------------------------------------------------------------

namespace SSMenuSystem.Features.Wrappers;

using System;

using Interfaces;
using UserSettings.ServerSpecific;

/// <summary>
/// Initialize a new instance of wrapper for <see cref="SSDropdownSetting"/>. This setting make a dropdown where player can select an input.
/// </summary>
public class Dropdown : SSDropdownSetting, ISetting
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Dropdown"/> class.
    /// </summary>
    /// <param name="id">The id of <see cref="SSDropdownSetting"/>. value "null" and is not supported, even if you can set it to null, and it will result of client crash.</param>
    /// <param name="label">The label of <see cref="Dropdown"/>. displayed at left in the row.</param>
    /// <param name="options">All options available.</param>
    /// <param name="onChanged">Triggered when the player selected a new index.</param>
    /// <param name="defaultOptionIndex">Select per default an option, according to <see cref="SSDropdownSetting.Options"/>.</param>
    /// <param name="entryType">The different available types NW gave.</param>
    /// <param name="hint">The hint (located in "?"). If null, no hint will be displayed.</param>
    public Dropdown(int? id, string label, string[] options, Action<ReferenceHub, string, int, SSDropdownSetting>? onChanged = null, int defaultOptionIndex = 0, DropdownEntryType entryType = DropdownEntryType.Regular, string? hint = null)
        : base(id, label, options, defaultOptionIndex, entryType, hint)
    {
        this.Action = onChanged;
        this.Base = new SSDropdownSetting(id, label, options, defaultOptionIndex, entryType, hint);
    }

    /// <summary>
    /// Gets the method that will be executed when the index is updated. It's contains four parameters: <br></br><br></br>
    /// - <see cref="ReferenceHub"/>, the player concerned by the change<br></br>
    /// - <see cref="string"/>, The new selected text, according to the new index. <br></br>
    /// - <see cref="int"/>, The new selected index. <br></br>
    /// - <see cref="SSPlaintextSetting"/>, where it's the class synced.
    /// </summary>
    /// <remarks>No errors will be thrown if <see cref="Action"/> is null.</remarks>
    public Action<ReferenceHub, string, int, SSDropdownSetting>? Action { get; }

    /// <summary>
    /// Gets or sets the base instance (sent in to the client).
    /// </summary>
    public ServerSpecificSettingBase Base { get; set; }
}