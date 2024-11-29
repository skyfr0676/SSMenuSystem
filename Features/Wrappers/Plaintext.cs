using System;
using ServerSpecificSyncer.Features.Interfaces;
using TMPro;
using UserSettings.ServerSpecific;

namespace ServerSpecificSyncer.Features.Wrappers
{
    public class Plaintext : SSPlaintextSetting, ISetting
    {
        public Action<ReferenceHub, string, SSPlaintextSetting> OnChanged { get; }
        public ServerSpecificSettingBase Base { get; set; }

        public Plaintext(int? id, string label, Action<ReferenceHub, string, SSPlaintextSetting> onChanged, string placeholder = "...", int characterLimit = 64, TMP_InputField.ContentType contentType = TMP_InputField.ContentType.Standard, string hint = null) : base(id, label, placeholder, characterLimit, contentType, hint)
        {
            Base = new SSPlaintextSetting(id, label, placeholder, characterLimit, contentType, hint);
            OnChanged = onChanged;
        }
    }
}