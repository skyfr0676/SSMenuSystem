using System;
using UserSettings.ServerSpecific;

namespace ServerSpecificSyncer.Features.Wrappers
{
    public struct ParameterSync<T>
    {
        public ParameterSync(ServerSpecificSettingBase @base, int settingId, string label, string hint, T value)
        {
            Base = @base;
            SettingId = settingId;
            Label = label;
            Hint = hint;
            Value = value;
        }

        public ServerSpecificSettingBase Base { get; }
        public int SettingId { get; }
        public string Label { get; }
        public string Hint { get; }
        public T Value { get; }
    }
}