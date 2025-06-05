// -----------------------------------------------------------------------
// <copyright file="LabelButton.cs" company="Skyfr0676 and Redforce04">
// Copyright (c) Skyfr0676 and Redforce04. All rights reserved.
// Licensed under the Undetermined license.
// </copyright>
// -----------------------------------------------------------------------

namespace SSMenuSystem.Configs;

/// <summary>
/// Button labels config.
/// </summary>
public class LabelButton
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LabelButton"/> class.
    /// </summary>
    /// <param name="label">the label text.</param>
    /// <param name="buttonText">the button text.</param>
    public LabelButton(string label, string buttonText)
    {
        this.Label = label;
        this.ButtonText = buttonText;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LabelButton"/> class.
    /// </summary>
    public LabelButton()
    {
        this.Label = "MISSING_LABEL";
        this.ButtonText = "MISSING_VALUE";
    }

    /// <summary>
    /// Gets or sets the label of button (displayed at the left).
    /// </summary>
    public string Label { get; set; }

    /// <summary>
    /// Gets or sets the Button content (displayed on the button).
    /// </summary>
    public string ButtonText { get; set; }
}