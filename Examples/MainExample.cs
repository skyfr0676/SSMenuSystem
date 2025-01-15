using ServerSpecificSyncer.Features;
using UserSettings.ServerSpecific;

namespace ServerSpecificSyncer.Examples
{
    public class MainExample : Menu
    {
        public override ServerSpecificSettingBase[] Settings => new ServerSpecificSettingBase[] { };
        public override string Name { get; set; } = "Examples";
        public override int Id { get; set; } = -200987;
    }
}