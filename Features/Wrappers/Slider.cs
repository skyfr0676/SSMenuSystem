using System;
using UserSettings.ServerSpecific;

namespace ServerSpecificSyncer.Features.Wrappers
{
    public class Slider : SSSliderSetting
    {
        public Action<ReferenceHub, float, SSSliderSetting> Action { get; }
        public SSSliderSetting Base { get; set; }

        public Slider(int? id, string label, float minValue, float maxValue, Action<ReferenceHub, float, SSSliderSetting> onChanged, float defaultValue = 0, bool integer = false, string valueToStringFormat = "0.##", string finalDisplayFormat = "{0}", string hint = null) : base(id, label, minValue, maxValue, defaultValue, integer, valueToStringFormat, finalDisplayFormat, hint)
        {
            Base = new(id, label, minValue, maxValue, defaultValue, integer, valueToStringFormat, finalDisplayFormat,
                hint);
            Action = onChanged;
        }
    }
}