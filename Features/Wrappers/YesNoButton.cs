using System;
using ServerSpecificSyncer.Features.Interfaces;
using UserSettings.ServerSpecific;

namespace ServerSpecificSyncer.Features.Wrappers
{
    public class YesNoButton : SSTwoButtonsSetting, ISetting
    {
        public Action<ReferenceHub, bool, SSTwoButtonsSetting> Action { get; }
        public ServerSpecificSettingBase Base { get; set; }


        public YesNoButton(int? id, string label, string optionA, string optionB, Action<ReferenceHub, bool, SSTwoButtonsSetting> onChanged, bool defaultIsB = false, string hint = null) : base(id, label, optionA, optionB, defaultIsB, hint)
        {
            Base = new SSTwoButtonsSetting(id, label, optionA, optionB, defaultIsB, hint);
            Action = onChanged;
        }
    }
}