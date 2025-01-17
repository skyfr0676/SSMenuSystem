using SSMenuSystem.Features;
using UserSettings.ServerSpecific;

namespace SSMenuSystem.Examples
{
    internal class MainExample : Menu
    {
        public override ServerSpecificSettingBase[] Settings => new ServerSpecificSettingBase[] { };
        public override string Name { get; set; } = "Examples";
        public override int Id { get; set; } = -200987;
    }
}