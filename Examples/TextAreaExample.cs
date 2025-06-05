// -----------------------------------------------------------------------
// <copyright file="TextAreaExample.cs" company="Skyfr0676 and Redforce04">
// Copyright (c) Skyfr0676 and Redforce04. All rights reserved.
// Licensed under the Undetermined license.
// </copyright>
// -----------------------------------------------------------------------

namespace SSMenuSystem.Examples;

using System;
using System.Collections.Generic;

using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using Features;
using Features.Wrappers;
using UnityEngine;
using UserSettings.ServerSpecific;

/// <summary>
/// The text area example menu.
/// </summary>
internal class TextAreaExample : Menu
{
    private List<ServerSpecificSettingBase>? settings;
    private SSTextArea? responseScan;

    /// <inheritdoc/>
    public override ServerSpecificSettingBase[] Settings => this.GetSettings();

    /// <inheritdoc/>
    public override string Name { get; set; } = "Text Area";

    /// <inheritdoc/>
    public override int Id { get; set; } = -7;

    /// <inheritdoc/>
    public override Type? MenuRelated { get; set; } = typeof(MainExample);

    /// <summary>
    /// Triggered whenever the scanner button is pressed.
    /// </summary>
    /// <param name="sender">The player that pressed the button.</param>
    private void OnScannerButtonPressed(ReferenceHub sender)
    {
        if (!(sender.roleManager.CurrentRole is IFpcRole currentRole1))
        {
            this.responseScan?.SendTextUpdate("Your current role is not supported.", false, x => x == sender);
        }
        else
        {
            string? str1 = null;
            foreach (ReferenceHub allHub in ReferenceHub.AllHubs)
            {
                if (allHub.roleManager.CurrentRole is HumanRole currentRole && !(allHub == sender))
                {
                    float num = Vector3.Distance(currentRole.FpcModule.Position, currentRole1.FpcModule.Position);
                    string str2 = $"\n-{allHub.nicknameSync.DisplayName} ({currentRole.GetColoredName()}) - {num} m";
                    str1 ??= "Detected humans: ";

                    str1 += str2;
                }
            }

            this.responseScan?.SendTextUpdate(str1 ?? "No humans detected.", false, x => x == sender);
        }
    }

    private ServerSpecificSettingBase[] GetSettings()
    {
        this.responseScan ??= new SSTextArea(null, "Not Scanned yet");
        this.settings =
        [
            new SSGroupHeader("Different Text Area Types"),
            new SSTextArea(1, "<color=cyan>This</color> <size=30>text</size> <color=red>area</color> <u>supports</u> <i>Rich</i> <b>Text</b> <rotate=\"25\">Tags</rotate>."),
            new SSTextArea(2, "This is another multi-line text area, but this one features auto-generated preview text when collapsed, with ellipses appearing when the text no longer fits. It also has an option enabled to collapse automatically when you switch off this settings tab. In other words, you will need to re-expand this text area each time you visit this tab.", SSTextArea.FoldoutMode.CollapseOnEntry),
            new SSTextArea(3, "This multi-line text area is expanded by default but can be collapsed if needed. It will retain its previous state when toggling this tab on and off.", SSTextArea.FoldoutMode.ExtendedByDefault),
            new SSTextArea(4, "This multi-line text area is similar to the one above, but it will re-expand itself after collapsing each time you visit this tab.", SSTextArea.FoldoutMode.ExtendOnEntry),
            new SSTextArea(5, "This multi-line text area cannot be collapsed.\nIt remains fully expanded at all times, but supports URL links.\nExample link: <link=\"https://www.youtube.com/watch?v=dQw4w9WgXcQ\"><mark=#5865f215>[Click]</mark></link>"),
            new SSGroupHeader("Human Scanner", false, "Generates a list of alive humans with their distances to you. The size is automatically adjusted based on the number of results."), this.responseScan,
            new Button(6, "Scan for players.", "Scan", (hub, _) => this.OnScannerButtonPressed(hub)),
        ];

        return this.settings.ToArray();
    }
}