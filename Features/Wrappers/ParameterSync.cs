#if DEBUG
using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UserSettings.ServerSpecific;

namespace ServerSpecificSyncer.Features.Wrappers
{
    public struct ParameterSync<T> : IEquatable<ParameterSync<T>> where T : ServerSpecificSettingBase
    {
        public ParameterSync(T @base, int settingId, string label, string hint, string strValue = "", float intValue = -1f, KeyCode keyValue = KeyCode.None, bool boolValue = false)
        {
            Base = @base;
            SettingId = settingId;
            Label = label;
            Hint = hint;
            StrValue = strValue;
            IntValue = intValue;
            if (StrValue == string.Empty && IntValue > -1f)
                StrValue = IntValue.ToString(CultureInfo.InvariantCulture);
            KeyValue = keyValue;
            if (StrValue == string.Empty && KeyValue != KeyCode.None)
                StrValue = KeyValue.ToString(CultureInfo.InvariantCulture);
            BoolValue = boolValue;
            if (StrValue == string.Empty)
                StrValue = BoolValue.ToString(CultureInfo.InvariantCulture);
        }

        public T Base { get; }
        public int SettingId { get; }
        public string Label { get; }
        public string Hint { get; }

        /// <summary>
        /// Used for all.
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

        public bool Equals(ParameterSync<T> other)
        {
            return Base == other.Base && SettingId == other.SettingId && Label == other.Label && Hint == other.Hint && StrValue == other.StrValue && IntValue.Equals(other.IntValue) && KeyValue == other.KeyValue && BoolValue == other.BoolValue;
        }

        public override bool Equals(object obj)
        {
            return obj is ParameterSync<T> other && Equals(other);
        }
    }
}
#endif