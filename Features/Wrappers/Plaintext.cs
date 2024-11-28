using System;
using TMPro;
using UserSettings.ServerSpecific;

namespace ServerSpecificSyncer.Features.Wrappers
{
    public class Plaintext : SSPlaintextSetting
    {
        public Action<ReferenceHub, string, SSPlaintextSetting> OnChanged { get; }

        public Plaintext(int? id, string label, Action<ReferenceHub, string, SSPlaintextSetting> onChanged, string placeholder = "...", int characterLimit = 64, TMP_InputField.ContentType contentType = TMP_InputField.ContentType.Standard, string hint = null) : base(id, label, placeholder, characterLimit, contentType, hint)
        {
            OnChanged = onChanged;
        }
    }
}