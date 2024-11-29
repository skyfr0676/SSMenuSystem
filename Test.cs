using Exiled.API.Features;
using Hints;
using ServerSpecificSyncer.Features;
using ServerSpecificSyncer.Features.Wrappers;
using UserSettings.ServerSpecific;

namespace ServerSpecificSyncer
{
    public class Test : Menu
    {
        public override ServerSpecificSettingBase[] Settings => new ServerSpecificSettingBase[]
        {
            new Button(5, "test", "ttt", ((hub, _) => Server.Broadcast.TargetAddElement(hub.connectionToClient, "x2 Boost", 4, Broadcast.BroadcastFlags.Normal)), 1, "test test test"),
            new Dropdown(6, "test", new[]{"test1", "test2", "test3"}, ((hub, selectionned, _) => Server.Broadcast.TargetAddElement(hub.connectionToClient, $"x2 Boost -> {selectionned}", 4, Broadcast.BroadcastFlags.Normal))),
            new Plaintext(7, "test", ((hub, newText, _) => Server.Broadcast.TargetAddElement(hub.connectionToClient, $"x2 Boost -> {newText}", 4, Broadcast.BroadcastFlags.Normal))),
            new Slider(8, "test",-5, 5, ((hub, newValue, _) => Server.Broadcast.TargetAddElement(hub.connectionToClient, $"x2 Boost -> {newValue}", 4, Broadcast.BroadcastFlags.Normal)), 1),
            new YesNoButton(9, "test","A", "B", ((hub, isB, _) => hub.hints.Show(new TextHint($"x2 Boost -> {isB}", durationScalar:4)))),
        };

        public override string Name { get; set; } = "try";
        public override int Id { get; set; } = -1;
        protected override string Description { get; set; } = "tkt fr";
    }
}