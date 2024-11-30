using System;
using ServerSpecificSyncer.Features.Interfaces;
using UserSettings.ServerSpecific;

namespace ServerSpecificSyncer.Features.Wrappers
{
    public class Button : SSButton, ISetting
    {
        public Action<ReferenceHub, SSButton> Action { get; }
        public ServerSpecificSettingBase Base { get; }

        public Button(int? id, string label, string buttonText, Action<ReferenceHub, SSButton> onClick, float? holdTimeSeconds = null, string hint = null) : base(id, label, buttonText, holdTimeSeconds, hint)
        {
            Action = onClick;
            Base = new SSButton(id, label, buttonText, holdTimeSeconds, hint);
        }
    }
}