using System;
using System.Collections.Generic;
using System.Reflection;
using UserSettings.ServerSpecific;

namespace SSMenuSystem.Features
{
    internal class AssemblyMenu : Menu
    {
        internal Assembly Assembly { get; set; }
        internal ServerSpecificSettingBase[] OverrideSettings { get; set; }
        public override ServerSpecificSettingBase[] Settings => OverrideSettings ?? Array.Empty<ServerSpecificSettingBase>();
        public override string Name { get; set; }
        public override int Id { get; set; }
        public override bool CheckAccess(ReferenceHub hub) =>
            (ActuallySendedToClient.TryGetValue(hub, out ServerSpecificSettingBase[] settings) && settings != null && !settings.IsEmpty()) ||
            (OverrideSettings != null && !OverrideSettings.IsEmpty());
        internal Dictionary<ReferenceHub, ServerSpecificSettingBase[]> ActuallySendedToClient { get; set; } = new();
    }
}