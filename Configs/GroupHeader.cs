// -----------------------------------------------------------------------
// <copyright file="GroupHeader.cs" company="Skyfr0676 and Redforce04">
// Copyright (c) Skyfr0676 and Redforce04. All rights reserved.
// Licensed under the Undetermined license.
// </copyright>
// -----------------------------------------------------------------------

namespace SSMenuSystem.Configs;

/// <summary>
/// Button labels config.
/// </summary>
public class GroupHeader
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GroupHeader"/> class.
    /// </summary>
    /// <param name="label">the label text.</param>
    /// <param name="hint">the button text.</param>
    public GroupHeader(string label, string? hint)
    {
        this.Label = label;
        this.Hint = hint ?? "MISSING_HINT";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GroupHeader"/> class.
    /// </summary>
    public GroupHeader()
    {
        this.Label = "MISSING_LABEL";
        this.Hint = "MISSING_HINT";
    }

    /// <summary>
    /// Gets or sets the label of button (displayed at the left).
    /// </summary>
    public string Label { get; set; }

    /// <summary>
    /// Gets or sets the Button content (displayed on the button).
    /// </summary>
    public string Hint { get; set; }
}