using System;
using UserSettings.ServerSpecific;

namespace ServerSpecificSyncer.Features.Wrappers
{
    public class Button : SSButton
    {
        public Action<ReferenceHub, SSButton> Action { get; }
        
        public Button(int? id, string label, string buttonText, Action<ReferenceHub, SSButton> onClick, float? holdTimeSeconds = null, string hint = null) : base(id, label, buttonText, holdTimeSeconds, hint)
        {
            Action = onClick;
        }
    }
}