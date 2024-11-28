using System;
using UserSettings.ServerSpecific;

namespace ServerSpecificSyncer.Features.Wrappers
{
    public class YesNoButton : SSTwoButtonsSetting
    {
        public Action<ReferenceHub, bool, SSTwoButtonsSetting> Action { get; }
        
        
        public YesNoButton(int? id, string label, string optionA, string optionB, Action<ReferenceHub, bool, SSTwoButtonsSetting> onChanged, bool defaultIsB = false, string hint = null) : base(id, label, optionA, optionB, defaultIsB, hint)
        {
            Action = onChanged;
        }
    }
}