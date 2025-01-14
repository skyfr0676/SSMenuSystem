using System;
using System.Collections.Generic;
using System.Reflection;
using UserSettings.ServerSpecific;

namespace ServerSpecificSyncer.Features
{
    public class AssemblyMenu : Menu
    {
        public Assembly Assembly { get; set; }
        public ServerSpecificSettingBase[] OverrideSettings { get; set; }
        public override ServerSpecificSettingBase[] Settings => OverrideSettings ?? Array.Empty<ServerSpecificSettingBase>();
        public override string Name { get; set; }
        public override int Id { get; set; }
        public override bool CheckAccess(ReferenceHub hub) =>
            (ActuallySendedToClient.TryGetValue(hub, out var settings) && settings != null && !settings.IsEmpty()) ||
            (OverrideSettings != null && !OverrideSettings.IsEmpty());
        public Dictionary<ReferenceHub, ServerSpecificSettingBase[]> ActuallySendedToClient { get; set; } = new();
    }
}