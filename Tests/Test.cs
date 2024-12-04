#if DEBUG
using Exiled.API.Features;
using Hints;
using ServerSpecificSyncer.Features;
using ServerSpecificSyncer.Features.Wrappers;
using UserSettings.ServerSpecific;

namespace ServerSpecificSyncer.Tests
{
    public class Test : Menu
    {
        public override ServerSpecificSettingBase[] Settings => new ServerSpecificSettingBase[]
        {
            new Button(5, "test", "ttt", (hub, _) => ShowHint(hub, $"x2 Boost"), 1, "test test test"),
            new Dropdown(6, "test", new[]{"test1", "test2", "test3"}, (hub, _, selectionned) => ShowHint(hub, $"x2 Boost -> {selectionned}")),
            new Plaintext(7, "test", (hub, newText, _) => ShowHint(hub, $"x2 Boost -> {newText}")),
            new Slider(8, "test",-5, 5, (hub, newValue, _) => ShowHint(hub, $"x2 Boost -> {newValue}"), 1),
            new YesNoButton(9, "test","A", "B", (hub, isB, _) => ShowHint(hub, $"x2 Boost -> {isB}")),
            new Keybind(10, "test global no perms", (hub) => Log.Info("global no perms"), preventInteractionOnGui:false, isGlobal:true),
        };

        public void ShowHint(ReferenceHub hub, string message, float duration = 4)
        {
            hub.hints.Show(new TextHint(message, new HintParameter[1]
            {
                new StringHintParameter(message)
            }, durationScalar: duration));
        }

        public override string Name { get; set; } = "try";
        public override int Id { get; set; } = -1;
        protected override string Description { get; set; } = "tkt fr";
    }
}
#endif