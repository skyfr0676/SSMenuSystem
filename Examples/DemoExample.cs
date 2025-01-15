using System;
using ServerSpecificSyncer.Features;
using UserSettings.ServerSpecific;
using UserSettings.ServerSpecific.Examples;

namespace ServerSpecificSyncer.Examples
{
    internal class DemoExample : Menu
    {
        private string[] _options = new string[4]
        {
            "Option 1",
            "Option 2",
            "Option 3",
            "Option 4"
        };

        public override ServerSpecificSettingBase[] Settings => new ServerSpecificSettingBase[]
        {
            new SSGroupHeader("GroupHeader"),
            new SSTwoButtonsSetting(1, "TwoButtonsSetting", "Option A", "Option B"),
            new SSTextArea(2, "TextArea"),
            new SSTextArea(3,
                "Multiline collapsable TextArea.\nLorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.",
                SSTextArea.FoldoutMode.ExtendedByDefault),
            new SSSliderSetting(4, "SliderSetting", 0.0f, 1f),
            new SSPlaintextSetting(5, "Plaintext"),
            new SSKeybindSetting(6, "KeybindSetting"),
            new SSDropdownSetting(7, "DropdownSetting", _options),
            new SSDropdownSetting(8, "Scrollable DropdownSetting", _options,
                entryType: SSDropdownSetting.DropdownEntryType.Scrollable),
            new SSButton(9, "Button", "Press me!"),
            new SSGroupHeader("Hints", hint: "Group headers are used to separate settings into subcategories."),
            new SSTwoButtonsSetting(10, "Another TwoButtonsSetting", "Option A", "Option B",
                hint: "Two Buttons are used to store Boolean values."),
            new SSSliderSetting(11, "Another SliderSetting", 0.0f, 1f,
                hint: "Sliders store a numeric value within a defined range."),
            new SSPlaintextSetting(12, "Another Plaintext", hint: "Plaintext fields store any provided text."),
            new SSKeybindSetting(13, "Another KeybindSetting",
                hint: "Allows checking if the player is currently holding the action key."),
            new SSDropdownSetting(14, "Another DropdownSetting", _options,
                hint: "Stores an integer value between 0 and the length of options minus 1."),
            new SSDropdownSetting(15, "Another Scrollable DropdownSetting", _options,
                entryType: SSDropdownSetting.DropdownEntryType.Scrollable,
                hint:
                "Alternative to dropdown. API is the same as in regular dropdown, but the client-side entry behaves differently."),
            new SSButton(16, "Another Button", "Press me! (again)",
                hint: "Triggers an event whenever it is pressed.")
        };

        public override string Name { get; set; } = "Demo Example";
        public override int Id { get; set; } = -6;
        public override Type MenuRelated { get; set; } = typeof(MainExample);
    }
}