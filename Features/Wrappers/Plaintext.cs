// -----------------------------------------------------------------------
// <copyright file="Plaintext.cs" company="Skyfr0676 and Redforce04">
// Copyright (c) Skyfr0676 and Redforce04. All rights reserved.
// Licensed under the Undetermined license.
// </copyright>
// -----------------------------------------------------------------------

namespace SSMenuSystem.Features.Wrappers;

using System;

using Interfaces;
using TMPro;
using UserSettings.ServerSpecific;

/// <summary>
/// Initialize a new instance of wrapper for <see cref="SSPlaintextSetting"/>. This setting make a plaintext where player can put text. When the player leave the plaintext (with "Esc", clicking anywhere or press "Enter"), the value will be updated on the server.
/// </summary>
public class Plaintext : SSPlaintextSetting, ISetting
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Plaintext"/> class.
    /// </summary>
    /// <param name="id">The id of <see cref="SSPlaintextSetting"/>. value "null" and is not supported, even if you can set it to null, and it will result of client crash.</param>
    /// <param name="label">The label of <see cref="Plaintext"/>. displayed at left in the row.</param>
    /// <param name="onChanged">Triggered when the player update the value.</param>
    /// <param name="placeholder">The placeholder value (if content is empty, a gray placeholder will be shown (if not empty or null).</param>
    /// <param name="characterLimit">The maximum characters a plaintext can take.</param>
    /// <param name="contentType">The type of content the plaintext can take.</param>
    /// <param name="hint">The hint (located in "?"). If null, no hint will be displayed.</param>
    public Plaintext(int? id, string label, Action<ReferenceHub, string, SSPlaintextSetting>? onChanged = null, string placeholder = "...", int characterLimit = 64, TMP_InputField.ContentType contentType = TMP_InputField.ContentType.Standard, string? hint = null)
        : base(id, label, placeholder, characterLimit, contentType, hint)
    {
        this.Base = new SSPlaintextSetting(id, label, placeholder, characterLimit, contentType, hint);
        this.OnChanged = onChanged;
    }

    /// <summary>
    /// Gets the method that will be executed when the value is updated. It's contains three parameters: <br></br><br></br>
    /// - <see cref="ReferenceHub"/>, the player concerned by the change<br></br>
    /// - <see cref="string"/>, The new value specified by <see cref="ReferenceHub"/><br></br>
    /// - <see cref="SSPlaintextSetting"/>, where it's the class synced.
    /// </summary>
    /// <remarks>No errors will be thrown if <see cref="Action"/> is null.</remarks>
    public Action<ReferenceHub, string, SSPlaintextSetting>? OnChanged { get; }

    /// <summary>
    /// Gets or sets the base instance (sent in to the client).
    /// </summary>
    public ServerSpecificSettingBase Base { get; set; }
}