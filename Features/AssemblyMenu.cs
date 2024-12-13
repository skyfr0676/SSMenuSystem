using System.Collections.Generic;
using System.Reflection;
using UserSettings.ServerSpecific;

namespace ServerSpecificSyncer.Features;

public class AssemblyMenu : Menu
{
    public Assembly Assembly { get; set; }
    public ServerSpecificSettingBase[] OverrideSettings { get; set; }
    public override ServerSpecificSettingBase[] Settings => OverrideSettings;
    public override string Name { get; set; }
    public override int Id { get; set; }
    public HashSet<ReferenceHub> AuthorizedPlayers { get; set; } = null;
    public override bool CheckAccess(ReferenceHub hub) => AuthorizedPlayers?.Contains(hub) ?? true;
}