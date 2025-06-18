using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AdminToys;
using GameCore;
using Mirror;
using UnityEngine;
using SSMenuSystem.Features;
using SSMenuSystem.Features.Wrappers;
using UserSettings.ServerSpecific;
using Log = SSMenuSystem.Features.Log;

namespace SSMenuSystem.Examples
{
    internal class LightSpawnerExample : Menu
    {
        private List<ServerSpecificSettingBase> _settings;
        private readonly List<ServerSpecificSettingBase> _addedSettings = new();
        private List<ColorPreset> _presets;
        private LightShadows[] _shadowsType;
        private LightType[] _lightType;
        private LightShape[] _lightShape;

        private SSTextArea _selectedColorTextArea;
        private bool AnySpawned => !_spawnedToys.IsEmpty();
        private readonly List<LightSourceToy> _spawnedToys = new();

        public override ServerSpecificSettingBase[] Settings => GetSettings();

        private ServerSpecificSettingBase[] GetSettings()
        {
            _presets ??= new List<ColorPreset>
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

            _shadowsType ??= EnumUtils<LightShadows>.Values;
            _lightType ??= EnumUtils<LightType>.Values;
            _lightShape ??= EnumUtils<LightShape>.Values;
            _selectedColorTextArea ??= new SSTextArea(ExampleId.SelectedColor, "Selected Color: None");

            _settings = new List<ServerSpecificSettingBase>
            {
                new Slider(ExampleId.Intensity, "Intensity", 0, 100, (hub, _, _) => ReloadColorInfoForUser(hub), 1, valueToStringFormat: "0.00", finalDisplayFormat: "x{0}"),
                new Slider(ExampleId.Range, "Range", 0, 100, null, 10, valueToStringFormat: "0.00", finalDisplayFormat: "x{0}"),
                new Dropdown(ExampleId.Color, "Color (preset)", _presets.Select(x => x.Name).ToArray(), (hub, _, _, _) => ReloadColorInfoForUser(hub)),
                new Plaintext(ExampleId.CustomColor, "Custom Color (R G B)", (hub, _, _) => ReloadColorInfoForUser(hub), characterLimit:11, hint: "Leave empty to use a preset."),
                _selectedColorTextArea,
                new Dropdown(ExampleId.ShadowType, "Shadows Type", _shadowsType.Select(x => x.ToString()).ToArray()),
                new Slider(ExampleId.ShadowStrength, "Shadow Strength", 0, 100),
                new Dropdown(ExampleId.LightType, "Light Type", _lightType.Select(x => x.ToString()).ToArray()),
                new Dropdown(ExampleId.LightShape, "Light Shape", _lightShape.Select(x => x.ToString()).ToArray()),
                new Slider(ExampleId.SpotAngle, "Spot Angle", 0, 100, null, 30, valueToStringFormat: "0.00", finalDisplayFormat: "x{0}"),
                new Slider(ExampleId.InnerSpotAngle, "Inner Spot Angle", 0, 100, valueToStringFormat: "0.00", finalDisplayFormat: "x{0}"),
                new Button(ExampleId.ConfirmSpawning, "Confirm Spawning", "Spawn", (hub, _) => Spawn(hub))
            };
            _settings.AddRange(_addedSettings);
            return _settings.ToArray();
        }

        private void ReloadColorInfoForUser(ReferenceHub hub) => _selectedColorTextArea.SendTextUpdate(GetColorInfoForUser(hub), receiveFilter:(h) => h == hub);

        private void Spawn(ReferenceHub sender)
        {
            LightSourceToy lightSourceToy = null;
            foreach (GameObject gameObject in NetworkClient.prefabs.Values)
            {
                if (gameObject.TryGetComponent(out LightSourceToy component))
                {
                    lightSourceToy = UnityEngine.Object.Instantiate(component);
                    lightSourceToy.OnSpawned(sender, new ArraySegment<string>(Array.Empty<string>()));
                    break;
                }
            }

            if (lightSourceToy == null)
                return;
            lightSourceToy.NetworkLightIntensity = sender.GetParameter<LightSpawnerExample, SSSliderSetting>(ExampleId.Intensity).SyncFloatValue;
            lightSourceToy.NetworkLightRange = sender.GetParameter<LightSpawnerExample, SSSliderSetting>(ExampleId.Range).SyncFloatValue;
            Color color = GetColorInfo(sender);
            lightSourceToy.NetworkLightColor = color;
            lightSourceToy.NetworkShadowType = (LightShadows)sender.GetParameter<LightSpawnerExample, SSDropdownSetting>(ExampleId.ShadowType).SyncSelectionIndexRaw;
            lightSourceToy.NetworkShadowStrength = sender.GetParameter<LightSpawnerExample, SSSliderSetting>(ExampleId.ShadowStrength).SyncFloatValue;
            lightSourceToy.NetworkLightType = (LightType)sender.GetParameter<LightSpawnerExample, SSDropdownSetting>(ExampleId.LightType).SyncSelectionIndexRaw;
            lightSourceToy.NetworkLightShape = (LightShape)sender.GetParameter<LightSpawnerExample, SSDropdownSetting>(ExampleId.LightShape).SyncSelectionIndexRaw;
            lightSourceToy.NetworkSpotAngle = sender.GetParameter<LightSpawnerExample, SSSliderSetting>(ExampleId.SpotAngle).SyncFloatValue;
            lightSourceToy.NetworkInnerSpotAngle = sender.GetParameter<LightSpawnerExample, SSSliderSetting>(ExampleId.InnerSpotAngle).SyncFloatValue;

            if (!AnySpawned)
            {
                _addedSettings.Add(new SSGroupHeader("Spawned Lights"));
                _addedSettings.Add(new Button(ExampleId.DestroyAll, "All Lights", "Destroy All (HOLD)", null, 2));
            }

            string hint =
                $"{lightSourceToy.LightType} Color: {color} SpawnPosition: {lightSourceToy.transform.position}" + "\n" + ("Spawned by " + sender.LoggedNameFromRefHub() + " at round time " + RoundStart.RoundLength.ToString("hh\\:mm\\:ss\\.fff", CultureInfo.InvariantCulture));
            _addedSettings.Add(new Button(ExampleId.DestroySpecific + (int)lightSourceToy.netId, $"Primitive NetID#{lightSourceToy.netId}", "Destroy (HOLD)", null, 0.4f, hint));
            _spawnedToys.Add(lightSourceToy);
            ReloadAll();
        }

        private void DestroyAll()
        {
            foreach (LightSourceToy toy in _spawnedToys.ToList())
            {
                _spawnedToys.Remove(toy);
                NetworkServer.Destroy(toy.gameObject);
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
            if (_addedSettings.Any(x => x.SettingId == netId))
                _addedSettings.Remove(_addedSettings.First(x => x.SettingId == netId));

            foreach (LightSourceToy toy in _spawnedToys.ToList().Where(toy => toy.netId == netId - ExampleId.DestroySpecific))
            {
                _spawnedToys.Remove(toy);
                NetworkServer.Destroy(toy.gameObject);
                break;
            }

            if (!AnySpawned)
                _addedSettings.Clear();

            ReloadAll();
        }

        public string GetColorInfoForUser(ReferenceHub hub)
        {
            return "Selected color: <color=" + this.GetColorInfo(hub).ToHex() + ">███████████</color>";
        }

        private Color GetColorInfo(ReferenceHub hub)
        {
            string[] array = hub.GetParameter<LightSpawnerExample, SSPlaintextSetting>(ExampleId.CustomColor).SyncInputText.Split(' ');
            int selectionIndex = hub.GetParameter<LightSpawnerExample, SSDropdownSetting>(ExampleId.Color).SyncSelectionIndexRaw;
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
                    : result3 / byte.MaxValue);
        }

        public override bool CheckAccess(ReferenceHub hub) => PermissionsHandler.IsPermitted(hub.serverRoles.Permissions, PlayerPermissions.FacilityManagement);

        public override string Name { get; set; } = "Light Spawner";
        public override int Id { get; set; } = -5;

        // ReSharper disable ConvertToConstant.Local
        private static class ExampleId
        {
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