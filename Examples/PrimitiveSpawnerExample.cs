using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AdminToys;
using Exiled.Events.Commands.Hub;
using GameCore;
using Mirror;
using SSMenuSystem.Features;
using SSMenuSystem.Features.Wrappers;
using UnityEngine;
using UserSettings.ServerSpecific;
using Utils.NonAllocLINQ;
using Log = PluginAPI.Core.Log;

namespace SSMenuSystem.Examples
{
    internal class PrimitiveSpawnerExample : Menu
    {
        private List<ServerSpecificSettingBase> _settings;
        private readonly List<ServerSpecificSettingBase> _addedSettings = new();
        private List<ColorPreset> _presets;
        private SSTextArea _selectedColorTextArea;
        private bool AnySpawned => !_spawnedToys.IsEmpty();
        private readonly List<PrimitiveObjectToy> _spawnedToys = new();

        public override ServerSpecificSettingBase[] Settings => GetSettings();

        public ServerSpecificSettingBase[] GetSettings()
        {
            _presets = new List<ColorPreset>
            {
                new("White", Color.white),
                new("Black", Color.black),
                new("Gray", Color.gray),
                new("Red", Color.red),
                new("Green", Color.green),
                new("Blue", Color.blue),
                new("Yellow", Color.yellow),
                new("Cyan", Color.cyan),
                new("Magenta", Color.magenta),
            };

            _selectedColorTextArea ??= new SSTextArea(ExampleId.SelectedColor, "Selected Color: None");

            _settings = new List<ServerSpecificSettingBase>
            {
                new Dropdown(ExampleId.Type, "Type", EnumUtils<PrimitiveType>.Values.Select(x => x.ToString()).ToArray(), (hub, _, _, _) => ReloadColorInfoForUser(hub)),
                new Dropdown(ExampleId.Color, "Color (preset)", _presets.Select(x => x.Name).ToArray(), (hub, _, _, _) => ReloadColorInfoForUser(hub)),
                new Slider(ExampleId.Opacity, "Opacity", 0, 100, (hub, _, _) => ReloadColorInfoForUser(hub), 100, true, finalDisplayFormat: "{0}%"),
                new Plaintext(ExampleId.CustomColor, "Color", (hub, _, _) => ReloadColorInfoForUser(hub), characterLimit:11, hint: "Leave empty to use a preset."),
                _selectedColorTextArea,
                new YesNoButton(ExampleId.Collisions, "Collisions", "Enabled", "Disabled"),
                new YesNoButton(ExampleId.Renderer, "Renderer", "Visible", "Invisible", null, false, "Invisible primitives can still receive collisions."),
                new Slider(ExampleId.ScaleX, "Scale (X)", 0, 50, null, 1, valueToStringFormat: "0.00", finalDisplayFormat: "x{0}"),
                new Slider(ExampleId.ScaleY, "Scale (Y)", 0, 50, null, 1, valueToStringFormat: "0.00", finalDisplayFormat: "x{0}"),
                new Slider(ExampleId.ScaleZ, "Scale (Z)", 0, 50, null, 1, valueToStringFormat: "0.00", finalDisplayFormat: "x{0}"),
                new Button(ExampleId.ConfirmSpawning, "Confirm Spawning", "Spawn", (hub, _) => Spawn(hub))
            };

            _settings.AddRange(_addedSettings);

            return _settings.ToArray();
        }

        public void ReloadColorInfoForUser(ReferenceHub hub)
        {
            Log.Info("reload color info for user " + hub.nicknameSync.MyNick + " triggered.");
            _selectedColorTextArea.SendTextUpdate(GetColorInfoForUser(hub), receiveFilter: (h) => h == hub);
        }

        public void Spawn(ReferenceHub sender)
        {
            PrimitiveObjectToy primitiveObjectToy = null;
            foreach (GameObject gameObject in NetworkClient.prefabs.Values.ToList())
            {
                if (gameObject.TryGetComponent(out PrimitiveObjectToy component))
                {
                    primitiveObjectToy = UnityEngine.Object.Instantiate(component);
                    primitiveObjectToy.OnSpawned(sender, new ArraySegment<string>(Array.Empty<string>()));
                    break;
                }
            }

            if (!primitiveObjectToy)
                return;
            int selection = sender.GetParameter<PrimitiveSpawnerExample, SSDropdownSetting>(ExampleId.Type).SyncSelectionIndexRaw;
            primitiveObjectToy.NetworkPrimitiveType = (PrimitiveType)selection;

            Color color = GetColorInfo(sender);
            primitiveObjectToy.NetworkMaterialColor = color;
            Vector3 scale = new(sender.GetParameter<PrimitiveSpawnerExample, SSSliderSetting>(ExampleId.ScaleX).SyncFloatValue,
                sender.GetParameter<PrimitiveSpawnerExample, SSSliderSetting>(ExampleId.ScaleY).SyncFloatValue,
                sender.GetParameter<PrimitiveSpawnerExample, SSSliderSetting>(ExampleId.ScaleZ).SyncFloatValue);
            primitiveObjectToy.transform.localScale = scale;
            primitiveObjectToy.NetworkScale = scale;
            PrimitiveFlags collisions =
                sender.GetParameter<PrimitiveSpawnerExample, SSTwoButtonsSetting>(ExampleId.Collisions).SyncIsA
                    ? PrimitiveFlags.Collidable
                    : PrimitiveFlags.None;

            PrimitiveFlags visible =
                sender.GetParameter<PrimitiveSpawnerExample, SSTwoButtonsSetting>(ExampleId.Renderer).SyncIsA
                    ? PrimitiveFlags.Visible
                    : PrimitiveFlags.None;

            primitiveObjectToy.NetworkPrimitiveFlags = collisions | visible;

            if (!AnySpawned)
            {
                _addedSettings.Add(new SSGroupHeader("Spawned Primitives"));
                _addedSettings.Add(new Button(ExampleId.DestroyAll, "All Primitives", "Destroy All (HOLD)", null, 2));
            }
            string hint =
                $"{primitiveObjectToy.PrimitiveType} Color: {color} Size: {scale} SpawnPosition: {primitiveObjectToy.transform.position}" + "\n" + "Spawned by " + sender.LoggedNameFromRefHub() + " at round time " + RoundStart.RoundLength.ToString(@"hh\:mm\:ss\.fff", CultureInfo.InvariantCulture);
            _addedSettings.Add(new Button(ExampleId.DestroySpecific + (int)primitiveObjectToy.netId, $"Primitive NetID#{primitiveObjectToy.netId}", "Destroy (HOLD)", null, 0.4f, hint));
            _spawnedToys.Add(primitiveObjectToy);            ReloadAll();
        }

        private void DestroyAll()
        {
            foreach (PrimitiveObjectToy toy in _spawnedToys.ToList())
            {
                NetworkServer.Destroy(toy.gameObject);
                _spawnedToys.Remove(toy);
            }

            _addedSettings.Clear();
            ReloadAll();
        }

        public override void OnInput(ReferenceHub hub, ServerSpecificSettingBase setting)
        {
            if (setting.SettingId > ExampleId.DestroySpecific)
                Destroy(setting.SettingId);
            if (setting.SettingId == ExampleId.DestroyAll)
                DestroyAll();

            base.OnInput(hub, setting);
        }

        private void Destroy(int netId)
        {
            int buttonId = netId;

            if (_addedSettings.Any(x => x.SettingId == buttonId))
                _addedSettings.Remove(_addedSettings.First(x => x.SettingId == buttonId));

            foreach (PrimitiveObjectToy toy in _spawnedToys.ToList().Where(toy => toy.netId == netId - ExampleId.DestroySpecific))
            {
                _spawnedToys.Remove(toy);
                NetworkServer.Destroy(toy.gameObject);
            }

            if (!AnySpawned)
                _addedSettings.Clear();

            ReloadAll();
        }

        public string GetColorInfoForUser(ReferenceHub hub)
        {
            return "Selected color: <color=" + GetColorInfo(hub).ToHex() + ">███████████</color>";
        }

        private Color GetColorInfo(ReferenceHub hub)
        {
            string[] array = hub.GetParameter<PrimitiveSpawnerExample, SSPlaintextSetting>(ExampleId.CustomColor)
                .SyncInputText.Split(' ');
            int selectionIndex = hub.GetParameter<PrimitiveSpawnerExample, SSDropdownSetting>(ExampleId.Color)
                .SyncSelectionIndexRaw;
            Color color = _presets[selectionIndex].Color;
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
                hub.GetParameter<PrimitiveSpawnerExample, SSSliderSetting>(ExampleId.Opacity)
                    .SyncFloatValue / 100f);
        }

        public override bool CheckAccess(ReferenceHub hub) => PermissionsHandler.IsPermitted(hub.serverRoles.Permissions, PlayerPermissions.FacilityManagement);

        public override string Name { get; set; } = "Primitive Spawner";
        public override int Id { get; set; } = -4;

        // ReSharper disable ConvertToConstant.Local
        private static class ExampleId
        {
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
        }
        // ReSharper restore ConvertToConstant.Local

        private readonly struct ColorPreset
        {
            public ColorPreset(string name, Color color)
            {
                Name = name;
                Color = color;
            }
            public readonly string Name;
            public readonly Color Color;
        }
        public override Type MenuRelated { get; set; } = typeof(MainExample);
    }
}