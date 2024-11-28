using System;
using UnityEngine;
using UserSettings.ServerSpecific;

namespace ServerSpecificSyncer.Features
{
    public class Keybind : SSKeybindSetting
    {
        public Keybind(int? id, string label, Action<ReferenceHub> onUsed, KeyCode suggestedKey = KeyCode.None, bool preventInteractionOnGui = true,
            string hint = null, bool isGlobal = true) : base(id+100000, label, suggestedKey, preventInteractionOnGui, hint)
        {
            IsGlobal = isGlobal;
            OnUsed = onUsed;
        }
        
        public bool IsGlobal { get; }
        
        public Action<ReferenceHub> OnUsed { get; }
    }
}