using System;
using ServerSpecificSyncer.Features.Interfaces;
using UserSettings.ServerSpecific;

namespace ServerSpecificSyncer.Features.Wrappers
{
    public class Dropdown : SSDropdownSetting, ISetting
    {
        public Action<ReferenceHub, SSDropdownSetting, string> Action { get; }
        public ServerSpecificSettingBase Base { get; set; }

        public Dropdown(int? id, string label, string[] options, Action<ReferenceHub, SSDropdownSetting, string> onChanged, int defaultOptionIndex = 0, DropdownEntryType entryType = DropdownEntryType.Regular, string hint = null) : base(id, label, options, defaultOptionIndex, entryType, hint)
        {
            Action = onChanged;
            Base = new SSDropdownSetting(id, label, options, defaultOptionIndex, entryType, hint);
        }
    }
}