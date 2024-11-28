using Exiled.API.Features;
using ServerSpecificSyncer.Features;
using ServerSpecificSyncer.Features.Wrappers;
using UserSettings.ServerSpecific;

namespace ServerSpecificSyncer
{
    public class Test : Menu
    {
        public override ServerSpecificSettingBase[] Settings => new ServerSpecificSettingBase[]
        {
            new Button(5, "test", "ttt", OnClick, 1, "test test test")
        };

        private void OnClick(ReferenceHub arg1, SSButton arg2)
        {
            Server.Broadcast.TargetAddElement(arg1.connectionToClient, "x2 Boost", 4, Broadcast.BroadcastFlags.Normal);
        }

        public override string Name { get; set; } = "try";
        public override int Id { get; set; } = -1;
    }
}