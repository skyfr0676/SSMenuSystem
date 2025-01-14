#if DEBUG
using Exiled.API.Features;
using Hints;
using ServerSpecificSyncer.Features;
using ServerSpecificSyncer.Features.Wrappers;
using UserSettings.ServerSpecific;

namespace ServerSpecificSyncer.Tests
{
    public class TestBis : Menu
    {
        public override ServerSpecificSettingBase[] Settings => new ServerSpecificSettingBase[]
        {
            new Button(5, "dadadaddetest", "ttt", (hub, _) => ShowHint(hub, $"x2 Boost"), 1, "test test test"),
            new Dropdown(6, "dadadaddetest", new[]{"test1", "test2", "test3"}, (hub, _, selectionned) => ShowHint(hub, $"x2 Boost -> {selectionned}")),
            new Plaintext(7, "dadadaddetest", (hub, newText, _) => ShowHint(hub, $"x2 Boost -> {newText}")),
            new Slider(8, "dadadaddetest",-5, 5, (hub, newValue, _) => ShowHint(hub, $"x2 Boost -> {newValue}")),
            new YesNoButton(9, "dadadaddetest","A", "B", (hub, isB, _) => ShowHint(hub, $"x2 Boost -> {isB}")),
            new Keybind(11, "test global perms", (hub) => Log.Info("global perms"), preventInteractionOnGui:false, isGlobal:true),
            new Keybind(12, "another local test", (hub) => Log.Info("local test"), preventInteractionOnGui:false, isGlobal:false),
            new Keybind(13, "another global test", (hub) => Log.Info("global test"), preventInteractionOnGui:false, isGlobal:true),
        };

        public override bool CheckAccess(ReferenceHub hub) => hub.serverRoles.RemoteAdmin;

        public void ShowHint(ReferenceHub hub, string message, float duration = 4)
        {
            hub.hints.Show(new TextHint(message, new HintParameter[1]
            {
                new StringHintParameter(message)
            }, durationScalar: duration));
        }

        public override string Name { get; set; } = "Try permissions";
        public override int Id { get; set; } = -2;
        protected override string Description { get; set; } = "tkt fr";
    }
}
#endif