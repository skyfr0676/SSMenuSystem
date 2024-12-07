using System;
using UnityEngine;
using UserSettings.ServerSpecific;

namespace ServerSpecificSyncer.Features.Wrappers
{
    public struct ParameterSync<T>
        where T : ServerSpecificSettingBase
    {
        public ParameterSync(T @base, int settingId, string label, string hint, string strValue = "", float intValue = -1f, KeyCode keyValue = KeyCode.None, bool boolValue = false)
        {
            Base = @base;
            SettingId = settingId;
            Label = label;
            Hint = hint;
            StrValue = strValue;
            IntValue = intValue;
            KeyValue = keyValue;
            BoolValue = boolValue;
        }

        public T Base { get; }
        public int SettingId { get; }
        public string Label { get; }
        public string Hint { get; }

        /// <summary>
        /// Used for <see cref="SSPlaintextSetting"/> and <see cref="SSDropdownSetting"/>
        /// </summary>
        public string StrValue { get; }

        /// <summary>
        /// Used for <see cref="SSSliderSetting"/> and <see cref="SSDropdownSetting"/> (the index).
        /// </summary>
        public float IntValue { get; }

        /// <summary>
        /// Used for <see cref="SSDropdownSetting"/>.
        /// </summary>
        public KeyCode KeyValue { get; }
        
        /// <summary>
        /// Used for <see cref="SSTwoButtonsSetting"/> (<see cref="SSTwoButtonsSetting.SyncIsA"/>).
        /// </summary>
        public bool BoolValue { get; }
    }
}