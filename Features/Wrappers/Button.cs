// -----------------------------------------------------------------------
// <copyright file="Button.cs" company="Skyfr0676 and Redforce04">
// Copyright (c) Skyfr0676 and Redforce04. All rights reserved.
// Licensed under the Undetermined license.
// </copyright>
// -----------------------------------------------------------------------

namespace SSMenuSystem.Features.Wrappers;

using System;

using Interfaces;
using UserSettings.ServerSpecific;

/// <summary>
/// Initialize a new instance of wrapper for <see cref="SSButton"/>. This setting make a dropdown where player can select an input.
/// </summary>
public class Button : SSButton, ISetting
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Button"/> class.
    /// </summary>
    /// <param name="id">The id of <see cref="SSButton"/>. value "null" and is not supported, even if you can set it to null, and it will result of client crash.</param>
    /// <param name="label">The label of <see cref="Button"/>. displayed at left in the row.</param>
    /// <param name="buttonText">The text displayed on the button.</param>
    /// <param name="onClick">Triggered when the player press the button.</param>
    /// <param name="holdTimeSeconds">When value is not equal to null, player will need to press specified time in seconds before the click will actually happen.</param>
    /// <param name="hint">The hint (located in "?"). If null, no hint will be displayed.</param>
    public Button(int? id, string label, string buttonText, Action<ReferenceHub, SSButton>? onClick, float? holdTimeSeconds = null, string? hint = null)
        : base(id, label, buttonText, holdTimeSeconds, hint)
    {
        this.Action = onClick;
        this.Base = new SSButton(id, label, buttonText, holdTimeSeconds, hint);
    }

    /// <summary>
    /// Gets the method that will be executed when the index is updated. It's contains two parameters: <br></br><br></br>
    /// - <see cref="ReferenceHub"/>, the player concerned by the press.<br></br>
    /// - <see cref="SSButton"/>, where it's the class synced.
    /// </summary>
    public Action<ReferenceHub, SSButton>? Action { get; }

    /// <summary>
    /// Gets the base instance (sent in to the client).
    /// </summary>
    public ServerSpecificSettingBase Base { get; }
}