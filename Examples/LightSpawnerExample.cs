// -----------------------------------------------------------------------
// <copyright file="LightSpawnerExample.cs" company="Skyfr0676 and Redforce04">
// Copyright (c) Skyfr0676 and Redforce04. All rights reserved.
// Licensed under the Undetermined license.
// </copyright>
// -----------------------------------------------------------------------

#pragma warning disable SA1011 // Square bracket nullability symbol spacing.
namespace SSMenuSystem.Examples;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using AdminToys;
using GameCore;
using Mirror;
using Features;
using Features.Wrappers;
using UnityEngine;
using UserSettings.ServerSpecific;

/// <summary>
/// A light spawner demo.
/// </summary>
internal class LightSpawnerExample : Menu
{
    private readonly List<ServerSpecificSettingBase> addedSettings = new ();
    private readonly List<LightSourceToy> spawnedToys = new ();
    private List<ServerSpecificSettingBase>? settings;
    private List<ColorPreset>? presets;
    private LightShadows[]? shadowsType;
    private LightType[]? lightType;
    private LightShape[]? lightShape;
    private SSTextArea? selectedColorTextArea;

    /// <inheritdoc/>
    public override ServerSpecificSettingBase[] Settings => this.GetSettings();

    /// <inheritdoc/>
    public override string Name { get; set; } = "Light Spawner";

    /// <inheritdoc/>
    public override int Id { get; set; } = -5;

    /// <inheritdoc/>
    public override Type? MenuRelated { get; set; } = typeof(MainExample);

    private bool AnySpawned => !this.spawnedToys.IsEmpty();

    /// <inheritdoc/>
    public override bool CheckAccess(ReferenceHub hub) => PermissionsHandler.IsPermitted(hub.serverRoles.Permissions, PlayerPermissions.FacilityManagement);

    /// <inheritdoc />
    public override void OnInput(ReferenceHub hub, ServerSpecificSettingBase setting)
    {
        if (setting.SettingId > ExampleId.DestroySpecific)
        {
            this.Destroy(setting.SettingId);
        }

        if (setting.SettingId == ExampleId.DestroyAll)
        {
            this.DestroyAll();
        }

        base.OnInput(hub, setting);
    }

    /// <summary>
    /// Gets a color info for a user.
    /// </summary>
    /// <param name="hub">The player's hub.</param>
    /// <returns>The color string for a user.</returns>
    public string GetColorInfoForUser(ReferenceHub hub)
    {
        return "Selected color: <color=" + this.GetColorInfo(hub).ToHex() + ">███████████</color>";
    }

    private ServerSpecificSettingBase[] GetSettings()
    {
        this.presets ??=
        [
            new ("White", Color.white),
            new ("Black", Color.black),
            new ("Gray", Color.gray),
            new ("Red", Color.red),
            new ("Green", Color.green),
            new ("Blue", Color.blue),
            new ("Yellow", Color.yellow),
            new ("Cyan", Color.cyan),
            new ("Magenta", Color.magenta),
        ];

        this.shadowsType ??= EnumUtils<LightShadows>.Values;
        this.lightType ??= EnumUtils<LightType>.Values;
        this.lightShape ??= EnumUtils<LightShape>.Values;
        this.selectedColorTextArea ??= new SSTextArea(ExampleId.SelectedColor, "Selected Color: None");

        this.settings =
        [
            new Slider(ExampleId.Intensity, "Intensity", 0, 100, (hub, _, _) => this.ReloadColorInfoForUser(hub), 1, valueToStringFormat: "0.00", finalDisplayFormat: "x{0}"),
            new Slider(ExampleId.Range, "Range", 0, 100, null, 10, valueToStringFormat: "0.00", finalDisplayFormat: "x{0}"),
            new Dropdown(ExampleId.Color, "Color (preset)", this.presets.Select(x => x.Name).ToArray(), (hub, _, _, _) => this.ReloadColorInfoForUser(hub)),
            new Plaintext(ExampleId.CustomColor, "Custom Color (R G B)", (hub, _, _) => this.ReloadColorInfoForUser(hub), characterLimit: 11, hint: "Leave empty to use a preset."), this.selectedColorTextArea,
            new Dropdown(ExampleId.ShadowType, "Shadows Type", this.shadowsType.Select(x => x.ToString()).ToArray()),
            new Slider(ExampleId.ShadowStrength, "Shadow Strength", 0, 100),
            new Dropdown(ExampleId.LightType, "Light Type", this.lightType.Select(x => x.ToString()).ToArray()),
            new Dropdown(ExampleId.LightShape, "Light Shape", this.lightShape.Select(x => x.ToString()).ToArray()),
            new Slider(ExampleId.SpotAngle, "Spot Angle", 0, 100, null, 30, valueToStringFormat: "0.00", finalDisplayFormat: "x{0}"),
            new Slider(ExampleId.InnerSpotAngle, "Inner Spot Angle", 0, 100, valueToStringFormat: "0.00", finalDisplayFormat: "x{0}"),
            new Button(ExampleId.ConfirmSpawning, "Confirm Spawning", "Spawn", (hub, _) => this.Spawn(hub)),
        ];
        this.settings.AddRange(this.addedSettings);
        return this.settings.ToArray();
    }

    private void ReloadColorInfoForUser(ReferenceHub hub) => this.selectedColorTextArea?.SendTextUpdate(this.GetColorInfoForUser(hub), receiveFilter: (h) => h == hub);

    private void Spawn(ReferenceHub sender)
    {
        LightSourceToy? lightSourceToy = null;
        foreach (GameObject gameObject in NetworkClient.prefabs.Values)
        {
            if (!gameObject.TryGetComponent(out LightSourceToy component))
            {
                continue;
            }

            lightSourceToy = UnityEngine.Object.Instantiate(component);
            lightSourceToy.OnSpawned(sender, new ArraySegment<string>(Array.Empty<string>()));
            break;
        }

        if (!lightSourceToy)
        {
            return;
        }

        lightSourceToy!.NetworkLightIntensity = sender.GetParameter<LightSpawnerExample, SSSliderSetting>(ExampleId.Intensity) !.SyncFloatValue;
        lightSourceToy.NetworkLightRange = sender.GetParameter<LightSpawnerExample, SSSliderSetting>(ExampleId.Range) !.SyncFloatValue;
        Color color = this.GetColorInfo(sender);
        lightSourceToy.NetworkLightColor = color;
        lightSourceToy.NetworkShadowType = (LightShadows)sender.GetParameter<LightSpawnerExample, SSDropdownSetting>(ExampleId.ShadowType) !.SyncSelectionIndexRaw;
        lightSourceToy.NetworkShadowStrength = sender.GetParameter<LightSpawnerExample, SSSliderSetting>(ExampleId.ShadowStrength) !.SyncFloatValue;
        lightSourceToy.NetworkLightType = (LightType)sender.GetParameter<LightSpawnerExample, SSDropdownSetting>(ExampleId.LightType) !.SyncSelectionIndexRaw;
        lightSourceToy.NetworkLightShape = (LightShape)sender.GetParameter<LightSpawnerExample, SSDropdownSetting>(ExampleId.LightShape) !.SyncSelectionIndexRaw;
        lightSourceToy.NetworkSpotAngle = sender.GetParameter<LightSpawnerExample, SSSliderSetting>(ExampleId.SpotAngle) !.SyncFloatValue;
        lightSourceToy.NetworkInnerSpotAngle = sender.GetParameter<LightSpawnerExample, SSSliderSetting>(ExampleId.InnerSpotAngle) !.SyncFloatValue;

        if (!this.AnySpawned)
        {
            this.addedSettings.Add(new SSGroupHeader("Spawned Lights"));
            this.addedSettings.Add(new Button(ExampleId.DestroyAll, "All Lights", "Destroy All (HOLD)", null, 2));
        }

        string hint = $"{lightSourceToy.LightType} Color: {color} SpawnPosition: {lightSourceToy.transform.position}" + "\n" + ("Spawned by " + sender.LoggedNameFromRefHub() + " at round time " + RoundStart.RoundLength.ToString(@"hh\:mm\:ss\.fff", CultureInfo.InvariantCulture));
        this.addedSettings.Add(new Button(ExampleId.DestroySpecific + (int)lightSourceToy.netId, $"Primitive NetID#{lightSourceToy.netId}", "Destroy (HOLD)", null, 0.4f, hint));
        this.spawnedToys.Add(lightSourceToy);
        this.ReloadAll();
    }

    private void DestroyAll()
    {
        foreach (LightSourceToy toy in this.spawnedToys.ToList())
        {
            this.spawnedToys.Remove(toy);
            NetworkServer.Destroy(toy.gameObject);
        }

        this.addedSettings.Clear();
        this.ReloadAll();
    }

    private void Destroy(int netId)
    {
        if (this.addedSettings.Any(x => x.SettingId == netId))
        {
            this.addedSettings.Remove(this.addedSettings.First(x => x.SettingId == netId));
        }

        foreach (LightSourceToy toy in this.spawnedToys.ToList().Where(toy => toy.netId == netId - ExampleId.DestroySpecific))
        {
            this.spawnedToys.Remove(toy);
            NetworkServer.Destroy(toy.gameObject);
            break;
        }

        if (!this.AnySpawned)
        {
            this.addedSettings.Clear();
        }

        this.ReloadAll();
    }

    private Color GetColorInfo(ReferenceHub hub)
    {
        string[] array = hub.GetParameter<LightSpawnerExample, SSPlaintextSetting>(ExampleId.CustomColor) !.SyncInputText.Split(' ');
        int selectionIndex = hub.GetParameter<LightSpawnerExample, SSDropdownSetting>(ExampleId.Color) !.SyncSelectionIndexRaw;
        Color color = this.presets![selectionIndex].Color;

#pragma warning disable SA1118 // parameter spans multiple lines.
        return new Color(
            !array.TryGet(0, out string element1) || !float.TryParse(element1, out float result1)
                ? color.r
                : result1 / byte.MaxValue,
            !array.TryGet(1, out string element2) || !float.TryParse(element2, out float result2)
                ? color.g
                : result2 / byte.MaxValue,
            !array.TryGet(2, out string element3) || !float.TryParse(element3, out float result3)
                ? color.b
                : result3 / byte.MaxValue);
#pragma warning restore SA1118
    }

    private static class ExampleId
    {
        // ReSharper disable ConvertToConstant.Local
        internal static readonly int Intensity = 1;
        internal static readonly int Range = 2;
        internal static readonly int Color = 3;
        internal static readonly int CustomColor = 4;
        internal static readonly int SelectedColor = 5;
        internal static readonly int ShadowType = 6;
        internal static readonly int ShadowStrength = 7;
        internal static readonly int LightType = 8;
        internal static readonly int LightShape = 9;
        internal static readonly int SpotAngle = 10;
        internal static readonly int InnerSpotAngle = 11;
        internal static readonly int ConfirmSpawning = 12;
        internal static readonly int DestroyAll = 13;
        internal static readonly int DestroySpecific = 14;

        // ReSharper restore ConvertToConstant.Local
    }

#pragma warning disable SA1201 // A struct should not follow a class
    private readonly struct ColorPreset
    {
        public readonly string Name;
        public readonly Color Color;

        public ColorPreset(string name, Color color)
        {
            this.Name = name;
            this.Color = color;
        }
    }
}
#pragma warning restore SA1011, SA1201
