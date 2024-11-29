using System;
using ServerSpecificSyncer.Features.Interfaces;
using UserSettings.ServerSpecific;

namespace ServerSpecificSyncer.Features.Wrappers
{
    public class Slider : SSSliderSetting, ISetting
    {
        public Action<ReferenceHub, float, SSSliderSetting> Action { get; }
        public ServerSpecificSettingBase Base { get; set; }

        public Slider(int? id, string label, float minValue, float maxValue, Action<ReferenceHub, float, SSSliderSetting> onChanged, float defaultValue = 0, bool integer = false, string valueToStringFormat = "0.##", string finalDisplayFormat = "{0}", string hint = null) : base(id, label, minValue, maxValue, defaultValue, integer, valueToStringFormat, finalDisplayFormat, hint)
        {
            Base = new SSSliderSetting(id, label, minValue, maxValue, defaultValue, integer, valueToStringFormat, finalDisplayFormat,
                hint);
            Action = onChanged;
        }
    }
}