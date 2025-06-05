// -----------------------------------------------------------------------
// <copyright file="PrimitiveSpawnerExample.cs" company="Skyfr0676 and Redforce04">
// Copyright (c) Skyfr0676 and Redforce04. All rights reserved.
// Licensed under the Undetermined license.
// </copyright>
// -----------------------------------------------------------------------

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
using Utils.NonAllocLINQ;

using Log = Features.Log;

/// <summary>
/// The primitive spawner example.
/// </summary>
internal class PrimitiveSpawnerExample : Menu
{
    private readonly List<ServerSpecificSettingBase> addedSettings = new ();
    private readonly List<PrimitiveObjectToy> spawnedToys = new ();
    private List<ServerSpecificSettingBase>? settings;
    private List<ColorPreset>? presets;
    private SSTextArea? selectedColorTextArea;

    /// <inheritdoc/>
    public override ServerSpecificSettingBase[] Settings => this.GetSettings();

    /// <inheritdoc/>
    public override string Name { get; set; } = "Primitive Spawner";

    /// <inheritdoc/>
    public override int Id { get; set; } = -4;

    /// <inheritdoc/>
    public override Type? MenuRelated { get; set; } = typeof(MainExample);

    private bool AnySpawned => !this.spawnedToys.IsEmpty();

    /// <summary>
    /// Triggered whenever a player uses an input.
    /// </summary>
    /// <param name="hub">The player using the input.</param>
    /// <param name="setting">The server specific setting being used.</param>
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

    /// <inheritdoc/>
    public override bool CheckAccess(ReferenceHub hub) => PermissionsHandler.IsPermitted(hub.serverRoles.Permissions, PlayerPermissions.FacilityManagement);

    /// <summary>
    /// Gets a color for a specific user.
    /// </summary>
    /// <param name="hub">The player to get the color for.</param>
    /// <returns>The color string for the player.</returns>
    // ReSharper disable once MemberCanBePrivate.Global
    public string GetColorInfoForUser(ReferenceHub hub)
    {
        return "Selected color: <color=" + this.GetColorInfo(hub).ToHex() + ">███████████</color>";
    }

    /// <summary>
    /// Gets the cached settings.
    /// </summary>
    /// <returns>The cached settings.</returns>
    // ReSharper disable once MemberCanBePrivate.Global
    public ServerSpecificSettingBase[] GetSettings()
    {
        this.presets =
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

        this.selectedColorTextArea ??= new SSTextArea(ExampleId.SelectedColor, "Selected Color: None");

        this.settings =
        [
            new Dropdown(ExampleId.Type, "Type", EnumUtils<PrimitiveType>.Values.Select(x => x.ToString()).ToArray(), (hub, _, _, _) => this.ReloadColorInfoForUser(hub)),
            new Dropdown(ExampleId.Color, "Color (preset)", this.presets.Select(x => x.Name).ToArray(), (hub, _, _, _) => this.ReloadColorInfoForUser(hub)),
            new Slider(ExampleId.Opacity, "Opacity", 0, 100, (hub, _, _) => this.ReloadColorInfoForUser(hub), 100, true, finalDisplayFormat: "{0}%"),
            new Plaintext(ExampleId.CustomColor, "Color", (hub, _, _) => this.ReloadColorInfoForUser(hub), characterLimit: 11, hint: "Leave empty to use a preset."), this.selectedColorTextArea,
            new YesNoButton(ExampleId.Collisions, "Collisions", "Enabled", "Disabled"),
            new YesNoButton(ExampleId.Renderer, "Renderer", "Visible", "Invisible", null, false, "Invisible primitives can still receive collisions."),
            new Slider(ExampleId.ScaleX, "Scale (X)", 0, 50, null, 1, valueToStringFormat: "0.00", finalDisplayFormat: "x{0}"),
            new Slider(ExampleId.ScaleY, "Scale (Y)", 0, 50, null, 1, valueToStringFormat: "0.00", finalDisplayFormat: "x{0}"),
            new Slider(ExampleId.ScaleZ, "Scale (Z)", 0, 50, null, 1, valueToStringFormat: "0.00", finalDisplayFormat: "x{0}"),
            new Button(ExampleId.ConfirmSpawning, "Confirm Spawning", "Spawn", (hub, _) => this.Spawn(hub)),
        ];

        this.settings.AddRange(this.addedSettings);

        return this.settings.ToArray();
    }

    /// <summary>
    /// Reloads color info for a specific user.
    /// </summary>
    /// <param name="hub">The user to reload the color info for.</param>
    // ReSharper disable once MemberCanBePrivate.Global
    public void ReloadColorInfoForUser(ReferenceHub hub)
    {
        Log.Info("reload color info for user " + hub.nicknameSync.MyNick + " triggered.");
        this.selectedColorTextArea?.SendTextUpdate(this.GetColorInfoForUser(hub), receiveFilter: (h) => h == hub);
    }

    /// <summary>
    /// Spawns a primitive for a specific user.
    /// </summary>
    /// <param name="sender">The user to spawn the primitive for.</param>
    // ReSharper disable once MemberCanBePrivate.Global
    public void Spawn(ReferenceHub sender)
    {
        PrimitiveObjectToy? primitiveObjectToy = null;
        foreach (GameObject gameObject in NetworkClient.prefabs.Values.ToList())
        {
            if (gameObject.TryGetComponent(out PrimitiveObjectToy component))
            {
                primitiveObjectToy = UnityEngine.Object.Instantiate(component);
                primitiveObjectToy.OnSpawned(sender, new ArraySegment<string>([]));
                break;
            }
        }

        if (!primitiveObjectToy)
        {
            return;
        }

        int selection = sender.GetParameter<PrimitiveSpawnerExample, SSDropdownSetting>(ExampleId.Type) !.SyncSelectionIndexRaw;
        primitiveObjectToy!.NetworkPrimitiveType = (PrimitiveType)selection;

        Color color = this.GetColorInfo(sender);
        primitiveObjectToy.NetworkMaterialColor = color;
        Vector3 scale = new (sender.GetParameter<PrimitiveSpawnerExample, SSSliderSetting>(ExampleId.ScaleX) !.SyncFloatValue,
            sender.GetParameter<PrimitiveSpawnerExample, SSSliderSetting>(ExampleId.ScaleY) !.SyncFloatValue,
            sender.GetParameter<PrimitiveSpawnerExample, SSSliderSetting>(ExampleId.ScaleZ) !.SyncFloatValue);
        primitiveObjectToy.transform.localScale = scale;
        primitiveObjectToy.NetworkScale = scale;
        PrimitiveFlags collisions =
            sender.GetParameter<PrimitiveSpawnerExample, SSTwoButtonsSetting>(ExampleId.Collisions) !.SyncIsA
                ? PrimitiveFlags.Collidable
                : PrimitiveFlags.None;

        PrimitiveFlags visible =
            sender.GetParameter<PrimitiveSpawnerExample, SSTwoButtonsSetting>(ExampleId.Renderer) !.SyncIsA
                ? PrimitiveFlags.Visible
                : PrimitiveFlags.None;

        primitiveObjectToy.NetworkPrimitiveFlags = collisions | visible;

        if (!this.AnySpawned)
        {
            this.addedSettings.Add(new SSGroupHeader("Spawned Primitives"));
            this.addedSettings.Add(new Button(ExampleId.DestroyAll, "All Primitives", "Destroy All (HOLD)", null, 2));
        }

        string hint = $"{primitiveObjectToy.PrimitiveType} Color: {color} Size: {scale} SpawnPosition: {primitiveObjectToy.transform.position}" + "\n" + "Spawned by " + sender.LoggedNameFromRefHub() + " at round time " + RoundStart.RoundLength.ToString(@"hh\:mm\:ss\.fff", CultureInfo.InvariantCulture);
        this.addedSettings.Add(new Button(ExampleId.DestroySpecific + (int)primitiveObjectToy.netId, $"Primitive NetID#{primitiveObjectToy.netId}", "Destroy (HOLD)", null, 0.4f, hint));
        this.spawnedToys.Add(primitiveObjectToy);
        this.ReloadAll();
    }

    private void DestroyAll()
    {
        foreach (PrimitiveObjectToy toy in this.spawnedToys.ToList())
        {
            NetworkServer.Destroy(toy.gameObject);
            this.spawnedToys.Remove(toy);
        }

        this.addedSettings.Clear();
        this.ReloadAll();
    }

    private void Destroy(int netId)
    {
        int buttonId = netId;

        if (this.addedSettings.Any(x => x.SettingId == buttonId))
        {
            this.addedSettings.Remove(this.addedSettings.First(x => x.SettingId == buttonId));
        }

        foreach (PrimitiveObjectToy toy in this.spawnedToys.ToList().Where(toy => toy.netId == netId - ExampleId.DestroySpecific))
        {
            this.spawnedToys.Remove(toy);
            NetworkServer.Destroy(toy.gameObject);
        }

        if (!this.AnySpawned)
        {
            this.addedSettings.Clear();
        }

        this.ReloadAll();
    }

    private Color GetColorInfo(ReferenceHub hub)
    {
        string[] array = hub.GetParameter<PrimitiveSpawnerExample, SSPlaintextSetting>(ExampleId.CustomColor) !.SyncInputText.Split(' ');
        int selectionIndex = hub.GetParameter<PrimitiveSpawnerExample, SSDropdownSetting>(ExampleId.Color) !.SyncSelectionIndexRaw;
        Color color = this.presets![selectionIndex].Color;
#pragma warning disable SA1118 // Spans multiple lines.
        return new Color(
            !array.TryGet(0, out string element1) || !float.TryParse(element1, out float result1)
                ? color.r
                : result1 / byte.MaxValue,
            !array.TryGet(1, out string element2) || !float.TryParse(element2, out float result2)
                ? color.g
                : result2 / byte.MaxValue,
            !array.TryGet(2, out string element3) || !float.TryParse(element3, out float result3)
                ? color.b
                : result3 / byte.MaxValue,
            hub.GetParameter<PrimitiveSpawnerExample, SSSliderSetting>(ExampleId.Opacity) !.SyncFloatValue / 100f);
#pragma warning restore SA1118
    }

    private static class ExampleId
    {
        // ReSharper disable ConvertToConstant.Local
        internal static readonly int Type = 1;
        internal static readonly int Color = 2;
        internal static readonly int Opacity = 3;
        internal static readonly int CustomColor = 4;
        internal static readonly int SelectedColor = 5;
        internal static readonly int Collisions = 6;
        internal static readonly int Renderer = 7;
        internal static readonly int ScaleX = 8;
        internal static readonly int ScaleY = 9;
        internal static readonly int ScaleZ = 10;
        internal static readonly int ConfirmSpawning = 11;
        internal static readonly int DestroyAll = 12;
        internal static readonly int DestroySpecific = 13;

        // ReSharper restore ConvertToConstant.Local
    }

#pragma warning disable SA1201 // Struct shouldn't follow a class.
    private readonly struct ColorPreset(string name, Color color)
    {
        public readonly string Name = name;
        public readonly Color Color = color;
    }

#pragma warning restore SA1201
}
