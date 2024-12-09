using System;
using ServerSpecificSyncer.Features.Interfaces;
using UnityEngine;
using UserSettings.ServerSpecific;

namespace ServerSpecificSyncer.Features.Wrappers
{
    /// <summary>
    /// Keybind system, to make keybinds global (or local, if <see cref="IsGlobal"/> is defined to false.
    /// </summary>
    public class Keybind : SSKeybindSetting, ISetting
    {
        public const int Increment = 0;
        /// <summary>
        /// Initialize instance of <see cref="Keybind"/>
        /// </summary>
        /// <param name="id">The id of <see cref="SSKeybindSetting"/> (up by 10.000).</param>
        /// <param name="label">The label of <see cref="SSKeybindSetting"/>.</param>
        /// <param name="onUsed"><inheritdoc cref="Action"/></param>
        /// <param name="suggestedKey">The suggested key of <see cref="SSKeybindSetting"/>.</param>
        /// <param name="preventInteractionOnGui">The parameter used to block interaction when UI is opened in <see cref="SSKeybindSetting"/>.</param>
        /// <param name="hint">The hint of <see cref="SSKeybindSetting"/>.</param>
        /// <param name="isGlobal"><inheritdoc cref="IsGlobal"/></param>
        public Keybind(int? id, string label, Action<ReferenceHub> onUsed, KeyCode suggestedKey = KeyCode.None, bool preventInteractionOnGui = true,
            string hint = null, bool isGlobal = true) : base(id+Increment, label, suggestedKey, preventInteractionOnGui, hint)
        {
            IsGlobal = isGlobal;
            Action = onUsed;
            Base = new SSKeybindSetting(id + Increment, label, suggestedKey, preventInteractionOnGui, hint);
;        }
        
        /// <summary>
        /// Gets or Sets whether the <see cref="SSKeybindSetting"/> would be shown and enabled on all pages or not.
        /// </summary>
        public bool IsGlobal { get; }
        
        /// <summary>
        /// The action will be executed when the button is pressed.
        /// </summary>
        public Action<ReferenceHub> Action { get; }

        public ServerSpecificSettingBase Base { get; }
    }
}