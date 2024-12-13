using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AdminToys;
using GameCore;
using Mirror;
using ServerSpecificSyncer.Features;
using ServerSpecificSyncer.Features.Wrappers;
using UnityEngine;
using UserSettings.ServerSpecific;

namespace ServerSpecificSyncer.Examples
{
    internal class PrimitiveSpawnerExample : Menu
    {
        private List<ServerSpecificSettingBase> _settings;
        private List<ColorPreset> _presets;
        private SSTextArea _selectedColorTextArea;
        private bool AnySpawned => !_spawnedToys.IsEmpty();
        private readonly List<PrimitiveObjectToy> _spawnedToys = new();
    
        public override ServerSpecificSettingBase[] Settings
        {
            get
            {
                if (_settings != null) return _settings.ToArray();
            
                _presets = new()
                {
                    new ColorPreset("White", Color.white),
                    new ColorPreset("Black", Color.black),
                    new ColorPreset("Gray", Color.gray),
                    new ColorPreset("Red", Color.red),
                    new ColorPreset("Green", Color.green),
                    new ColorPreset("Blue", Color.blue),
                    new ColorPreset("Yellow", Color.yellow),
                    new ColorPreset("Cyan", Color.cyan),
                    new ColorPreset("Magenta", Color.magenta),
                };
                
                _settings = new();
                _settings.Add(new Dropdown(ExampleId.Type, "Type", EnumUtils<PrimitiveType>.Values.Select(x => x.ToString()).ToArray(), (hub, setting, arg3) => ReloadColorInfoForUser(hub)));
                _settings.Add(new Dropdown(ExampleId.Color, "Color (preset)", _presets.Select(x => x.Name).ToArray(), (hub, setting, arg3) => ReloadColorInfoForUser(hub)));
                _settings.Add(new Slider(ExampleId.Opacity, "Opacity", 0, 100, (hub, f, arg3) => ReloadColorInfoForUser(hub), 100, true, finalDisplayFormat: "{0}%"));
                _settings.Add(new Plaintext(ExampleId.CustomColor, "Opacity", (hub, setting, arg3) => ReloadColorInfoForUser(hub), characterLimit:11, hint: "Leave empty to use a preset."));
                _selectedColorTextArea = new SSTextArea(ExampleId.SelectedColor, "Selected Color: None");
                _settings.Add(_selectedColorTextArea);
                _settings.Add(new YesNoButton(ExampleId.Collisions, "Collisions", "Enabled", "Disabled", null));
                _settings.Add(new YesNoButton(ExampleId.Renderer, "Renderer", "Visible", "Invisible", null, false, "Invisible primitives can still receive collisions."));
                _settings.Add(new Slider(ExampleId.ScaleX, "Scale (X)", 0, 50, null, 1, valueToStringFormat: "0.00", finalDisplayFormat: "x{0}"));
                _settings.Add(new Slider(ExampleId.ScaleY, "Scale (Y)", 0, 50, null, 1, valueToStringFormat: "0.00", finalDisplayFormat: "x{0}"));
                _settings.Add(new Slider(ExampleId.ScaleZ, "Scale (Z)", 0, 50, null, 1, valueToStringFormat: "0.00", finalDisplayFormat: "x{0}"));
                _settings.Add(new Button(ExampleId.ConfirmSpawning, "Confirm Spawning", "Spawn", (hub, btn) => Spawn(hub)));

                return _settings.ToArray();
            }
        }

        public void ReloadColorInfoForUser(ReferenceHub hub) => _selectedColorTextArea.SendTextUpdate(GetColorInfoForUser(hub), receiveFilter:(h) => h == hub);

        public void Spawn(ReferenceHub sender)
        { 
            PrimitiveObjectToy primitiveObjectToy = (PrimitiveObjectToy) null;
            foreach (GameObject gameObject in NetworkClient.prefabs.Values)
            {
                PrimitiveObjectToy component;
                if (gameObject.TryGetComponent<PrimitiveObjectToy>(out component))
                {
                    primitiveObjectToy = UnityEngine.Object.Instantiate<PrimitiveObjectToy>(component);
                    primitiveObjectToy.OnSpawned(sender, new ArraySegment<string>(Array.Empty<string>()));
                    break;
                }
            }

            if (primitiveObjectToy == null)
                return;
            int selection = sender.GetParameter<PrimitiveSpawnerExample, SSDropdownSetting>(ExampleId.Type).SyncSelectionIndexValidated;
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
                _settings.Add(new SSGroupHeader("Spawned Primitives"));
                _settings.Add(new Button(ExampleId.DestroyAll, "All Primitives", "Destroy All (HOLD)", null, 2));
            }
            string hint =
                $"{primitiveObjectToy.PrimitiveType} Color: {color} Size: {scale} SpawnPosition: {primitiveObjectToy.transform.position}" + "\n" + "Spawned by " + sender.LoggedNameFromRefHub() + " at round time " + RoundStart.RoundLength.ToString(@"hh\:mm\:ss\.fff", CultureInfo.InvariantCulture);
            _settings.Add(new Button(ExampleId.DestroySpecific + (int)primitiveObjectToy.netId, $"Primitive NetID#{primitiveObjectToy.netId}", "Destroy (HOLD)", ((hub, button) => Destroy(primitiveObjectToy.netId)), 0.4f, hint));
            _spawnedToys.Add(primitiveObjectToy);
            ReloadAll();
        }

        private void DestroyAll()
        {
            foreach (PrimitiveObjectToy toy in _spawnedToys.ToList())
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

            foreach (PrimitiveObjectToy toy in _spawnedToys.ToList().Where(toy => toy.netId == netId))
            {
                _spawnedToys.Remove(toy);
                NetworkServer.Destroy(toy.gameObject);
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
            string[] array = hub.GetParameter<PrimitiveSpawnerExample, Plaintext>(ExampleId.Color).SyncInputText.Split(' ');
            int selectionIndex = hub.GetParameter<PrimitiveSpawnerExample, SSDropdownSetting>(ExampleId.CustomColor).SyncSelectionIndexValidated;
            Color color = _presets[selectionIndex].Color;
            string element1;
            float result1;
            string element2;
            float result2;
            string element3;
            float result3;
            return new Color(!array.TryGet<string>(0, out element1) || !float.TryParse(element1, out result1) ? color.r : result1 / (float) byte.MaxValue, !array.TryGet<string>(1, out element2) || !float.TryParse(element2, out result2) ? color.g : result2 / (float) byte.MaxValue, !array.TryGet<string>(2, out element3) || !float.TryParse(element3, out result3) ? color.b : result3 / (float) byte.MaxValue, Parameters.GetParameter<PrimitiveSpawnerExample, SSSliderSetting>(hub, ExampleId.Color).SyncFloatValue / 100f);
        }

        public override bool CheckAccess(ReferenceHub hub) => PermissionsHandler.IsPermitted(hub.serverRoles.Permissions, PlayerPermissions.FacilityManagement);

        public override string Name { get; set; } = "Primitive Spawner";
        public override int Id { get; set; } = -4;

        private static class ExampleId
        {
            internal const int Type = 1;
            internal const int Color = 2;
            internal const int Opacity = 3;
            internal const int CustomColor = 4;
            internal const int SelectedColor = 5;
            internal const int Collisions = 6;
            internal const int Renderer = 7;
            internal const int ScaleX = 8;
            internal const int ScaleY = 9;
            internal const int ScaleZ = 10;
            internal const int ConfirmSpawning = 11;
            internal const int DestroyAll = 12;
            internal const int DestroySpecific = 13;
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

