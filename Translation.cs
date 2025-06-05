// -----------------------------------------------------------------------
// <copyright file="Translation.cs" company="Skyfr0676 and Redforce04">
// Copyright (c) Skyfr0676 and Redforce04. All rights reserved.
// Licensed under the Undetermined license.
// </copyright>
// -----------------------------------------------------------------------

namespace SSMenuSystem;

using System.ComponentModel;

#if EXILED
using Exiled.API.Interfaces;
#endif
using Configs;

/// <summary>
/// The translation configs.
/// </summary>
public class Translation
#if EXILED
    : ITranslation
#endif
{
    /// <summary>
    /// Gets or sets the text on the main-menu, button displayed to open a menu where {0} = menu name.
    /// </summary>
    [Description("On the main-menu, button displayed to open a menu where {0} = menu name.")]
    public LabelButton OpenMenu { get; set; } = new ("Open {0}", "Open");

    /// <summary>
    /// Gets or sets the button displayed when menu is opened.
    /// </summary>
    [Description("the button displayed when menu is opened.")]
    public LabelButton ReturnToMenu { get; set; } = new ("Return to menu", "Return");

    /// <summary>
    /// Gets or sets the button that displayed when sub-menu is opened (return to related menu) where {0} = menu name.
    /// </summary>
    [Description("The button that displayed when sub-menu is opened (return to related menu) where {0} = menu name.")]
    public LabelButton ReturnTo { get; set; } = new ("Return to {0}", "Return");

    /// <summary>
    /// Gets or sets the reload button.
    /// </summary>
    [Description("The reload button.")]
    public LabelButton ReloadButton { get; set; } = new ("Reload menus", "Reload");

    /// <summary>
    /// Gets or sets the global keybinding header, with label and hint. Disabled temporary.
    /// </summary>
    [Description("The global keybinding header, with label and hint. Disabled temporary.")]
    public GroupHeader GlobalKeybindingTitle { get; set; } = new ("Global Keybinding", "don't take a look at this (nah seriously it's just to make some keybindings global)");

    /// <summary>
    /// Gets or sets the text displayed when an error is occured (to avoid client crash + explain why it didn't work). Can accept TextMeshPro tags.
    /// </summary>
    [Description("Text displayed when an error is occured (to avoid client crash + explain why it's don't work). Can accept TextMeshPro tags.")]
    public string ServerError { get; set; } = "INTERNAL SERVER ERROR";

    /// <summary>
    /// Gets or sets the title of sub-menus when there is one.
    /// </summary>
    [Description("Title of sub-menus when there is one.")]
    public GroupHeader SubMenuTitle { get; set; } = new ("Sub-Menus", null);

    /// <summary>
    /// Gets or sets the Translation when player doesn't have permission to see total errors (= see a part of code name).
    /// </summary>
    [Description("Translation when player doesn't have permission to see total errors (= see a part of code name).")]
    public string NoPermission { get; set; } = "insufficient permissions to see the full errors";
}