#if false
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using AdminToys;
using GameCore;
using Mirror;
using ServerSpecificSyncer.Features;
using ServerSpecificSyncer.Features.Wrappers;
using UnityEngine;
using UserSettings.ServerSpecific;
using Log = PluginAPI.Core.Log;

namespace ServerSpecificSyncer.Examples
{
    internal class LightSpawnerExample : Menu
    {
        private List<ServerSpecificSettingBase> _settings;
        private List<ColorPreset> _presets;
        private LightShadows[] _shadowsType;
        private LightType[] _lightType;
        private LightShape[] _lightShape;
    
        private SSTextArea _selectedColorTextArea;
        private bool AnySpawned => !_spawnedToys.IsEmpty();
        private readonly List<LightSourceToy> _spawnedToys = new();
    
        public override ServerSpecificSettingBase[] Settings
        {
            get
            {
                if (_settings != null) return _settings.ToArray();
            
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

                _settings = new();
                _settings.Add(new Slider(ExampleId.Intensity, "Intensity", 0, 100, (hub, setting, arg3) => ReloadColorInfoForUser(hub), 1, valueToStringFormat: "0.00", finalDisplayFormat: "x{0}"));
                _settings.Add(new Slider(ExampleId.Range, "Range", 0, 100, null, 10, valueToStringFormat: "0.00", finalDisplayFormat: "x{0}"));
                _settings.Add(new Dropdown(ExampleId.Color, "Color (preset)", _presets.Select(x => x.Name).ToArray(), (hub, setting, arg3) => ReloadColorInfoForUser(hub)));
                _settings.Add(new Plaintext(ExampleId.CustomColor, "Custom Color (R G B)", (hub, setting, arg3) => ReloadColorInfoForUser(hub), characterLimit:11, hint: "Leave empty to use a preset."));
                _selectedColorTextArea = new SSTextArea(ExampleId.SelectedColor, "Selected Color: None");
                _settings.Add(_selectedColorTextArea);
                _settings.Add(new Dropdown(ExampleId.ShadowType, "Shadows Type", _shadowsType.Select(x => x.ToString()).ToArray(), null));
                _settings.Add(new Slider(ExampleId.ShadowStrength, "Shadow Strength", 0, 100, null));
                _settings.Add(new Dropdown(ExampleId.LightType, "Light Type", _lightType.Select(x => x.ToString()).ToArray(), null));
                _settings.Add(new Dropdown(ExampleId.LightShape, "Light Shape", _lightShape.Select(x => x.ToString()).ToArray(), null));
                _settings.Add(new Slider(ExampleId.SpotAngle, "Spot Angle", 0, 100, null, 30, valueToStringFormat: "0.00", finalDisplayFormat: "x{0}"));
                _settings.Add(new Slider(ExampleId.InnerSpotAngle, "Inner Spot Angle", 0, 100, null, valueToStringFormat: "0.00", finalDisplayFormat: "x{0}"));
                _settings.Add(new Button(ExampleId.ConfirmSpawning, "Confirm Spawning", "Spawn", (hub, btn) => Spawn(hub)));

                return _settings.ToArray();
            }
        }

        public void ReloadColorInfoForUser(ReferenceHub hub) => _selectedColorTextArea.SendTextUpdate(GetColorInfoForUser(hub), receiveFilter:(h) => h == hub);

        public void Spawn(ReferenceHub sender)
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
            lightSourceToy.NetworkShadowType = (LightShadows)sender.GetParameter<LightSpawnerExample, SSDropdownSetting>(ExampleId.ShadowType).SyncSelectionIndexValidated;
            lightSourceToy.NetworkShadowStrength = sender.GetParameter<LightSpawnerExample, SSSliderSetting>(ExampleId.ShadowStrength).SyncFloatValue;
            lightSourceToy.NetworkLightType = (LightType)sender.GetParameter<LightSpawnerExample, SSDropdownSetting>(ExampleId.LightType).SyncSelectionIndexValidated;
            lightSourceToy.NetworkLightShape = (LightShape)sender.GetParameter<LightSpawnerExample, SSDropdownSetting>(ExampleId.LightShape).SyncSelectionIndexValidated;
            lightSourceToy.NetworkSpotAngle = sender.GetParameter<LightSpawnerExample, SSSliderSetting>(ExampleId.SpotAngle).SyncFloatValue;
            lightSourceToy.NetworkInnerSpotAngle = sender.GetParameter<LightSpawnerExample, SSSliderSetting>(ExampleId.InnerSpotAngle).SyncFloatValue;
        
            if (!AnySpawned)
            {
                _settings.Add(new SSGroupHeader("Spawned Primitives"));
                _settings.Add(new Button(ExampleId.DestroyAll, "All Primitives", "Destroy All (HOLD)", null, 2));
            }

            string hint =
                $"{lightSourceToy.LightType} Color: {color} SpawnPosition: {lightSourceToy.transform.position}" + "\n" + ("Spawned by " + sender.LoggedNameFromRefHub() + " at round time " + RoundStart.RoundLength.ToString("hh\\:mm\\:ss\\.fff", (IFormatProvider) CultureInfo.InvariantCulture));
            _settings.Add(new Button(ExampleId.DestroySpecific + (int)lightSourceToy.netId, $"Primitive NetID#{lightSourceToy.netId}", "Destroy (HOLD)", ((hub, button) => Destroy(lightSourceToy.netId)), 0.4f, hint));
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

            _settings.Clear();
            ReloadAll();
        }

        private void Destroy(uint netId)
        {
            int buttonId = ExampleId.DestroySpecific + (int)netId;
        
            if (buttonId < _settings.Count)
                _settings.RemoveAt(buttonId);

            foreach (LightSourceToy toy in _spawnedToys.ToList().Where(toy => toy.netId == netId))
            {
                _spawnedToys.Remove(toy);
                NetworkServer.Destroy(toy.gameObject);
                break;
            }

            if (!AnySpawned)
                _settings.Clear();

            ReloadAll();
        }

        public string GetColorInfoForUser(ReferenceHub hub)
        {
            return "Selected color: <color=" + this.GetColorInfo(hub).ToHex() + ">███████████</color>";
        }

        private Color GetColorInfo(ReferenceHub hub)
        {
            string[] array = hub.GetParameter<LightSpawnerExample, SSPlaintextSetting>(ExampleId.Color).SyncInputText.Split(' ');
            int selectionIndex = hub.GetParameter<LightSpawnerExample, SSDropdownSetting>(ExampleId.CustomColor).SyncSelectionIndexValidated;
            Color color = _presets[selectionIndex].Color;
            string element1;
            float result1;
            string element2;
            float result2;
            string element3;
            float result3;
            return new Color(!array.TryGet<string>(0, out element1) || !float.TryParse(element1, out result1) ? color.r : result1 / (float) byte.MaxValue, !array.TryGet<string>(1, out element2) || !float.TryParse(element2, out result2) ? color.g : result2 / (float) byte.MaxValue, !array.TryGet<string>(2, out element3) || !float.TryParse(element3, out result3) ? color.b : result3 / (float) byte.MaxValue, Parameters.GetParameter<LightSpawnerExample, SSSliderSetting>(hub, ExampleId.Color).SyncFloatValue / 100f);
        }

        public override bool CheckAccess(ReferenceHub hub) => PermissionsHandler.IsPermitted(hub.serverRoles.Permissions, PlayerPermissions.FacilityManagement);

        public override string Name { get; set; } = "Light Spawner";
        public override int Id { get; set; } = -5;

        private static class ExampleId
        {
            internal const int Intensity = 1;
            internal const int Range = 2;
            internal const int Color = 3;
            internal const int CustomColor = 4;
            internal const int SelectedColor = 5;
            internal const int ShadowType = 6;
            internal const int ShadowStrength = 7;
            internal const int LightType = 8;
            internal const int LightShape = 9;
            internal const int SpotAngle = 10;
            internal const int InnerSpotAngle = 11;
            internal const int ConfirmSpawning = 12;
            internal const int DestroyAll = 13;
            internal const int DestroySpecific = 14;
        }
    
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
    }
}
#endif

