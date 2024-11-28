using System;
using PluginAPI.Core;
using ServerSpecificSyncer.Features;
using UnityEngine;
using UserSettings.ServerSpecific;

namespace ServerSpecificSyncer.Test.Subs
{
    public class Bugga : Menu
    {
        protected override ServerSpecificSettingBase[] Settings => new ServerSpecificSettingBase[]
        {
            //new SSPlaintextSetting(2, "franchement"), //TODO: COMPRENDRE L'ERREUR
            new SSSliderSetting(2, "Are you ready ?", -5f, 5, 0, true),
            new SSTextArea(3, "bruuh"),
            new Keybind(4, "test local", (hub) => Log.Warning("local test check passed"), KeyCode.K, false, isGlobal:true),
            new Keybind(5, "test global", (hub) => Log.Warning("global test check passed"), KeyCode.H, false),
        };
        public override string Name { get; set; } = "Sub-Menu related";
        public override int Id { get; set; } = -1;
        public override Type? MenuRelated { get; set; } = typeof(SubMenuTest);
    }
}