using System;
using SSMenuSystem.Features.Interfaces;
using UnityEngine;
using UserSettings.ServerSpecific;

namespace SSMenuSystem.Features.Wrappers
{
    /// <summary>
    /// Initialize a new instance of wrapper for <see cref="SSKeybindSetting"/> with the <see cref="IsGlobal"/> addition (according to menu). This setting can allow player to bind a key (like cmd_bind) but for an action, and not an command. Because of pages, i added <see cref="IsGlobal"/> paameter, that determinate if the setting will be saw in all pages or not.
    /// </summary>
    public class Keybind : SSKeybindSetting, ISetting
    {
         internal const int Increment = 0;

         /// <summary>
         /// Initialize a new instance of <see cref="Keybind"/>.
         /// </summary>
         /// <param name="id">The id of <see cref="SSKeybindSetting"/>. value "null" and is not supported, even if you can set it to null, and it will result of client crash.</param>
         /// <param name="label">The label of <see cref="Keybind"/>. displayed at left in the row.</param>
         /// <param name="onUsed">Triggered when the player press or release the keybind.</param>
         /// <param name="suggestedKey">The key server will suggest to the client (displayed with a star). Does not mean the default value.</param>
         /// <param name="preventInteractionOnGui">If false, the keybind won't work when any gui (like Main menu, Inventory, RA, etc...) is opened.</param>
         /// <param name="hint">The hint (located in "?"). If null, no hint will be displayed.</param>
         /// <param name="isGlobal"><inheritdoc cref="IsGlobal"/></param>
         public Keybind(int? id, string label, Action<ReferenceHub, bool> onUsed = null, KeyCode suggestedKey = KeyCode.None, bool preventInteractionOnGui = true,
            string hint = null, bool isGlobal = true) : base(id+Increment, label, suggestedKey, preventInteractionOnGui, hint)
        {
            IsGlobal = isGlobal;
            Action = onUsed;
            Base = new SSKeybindSetting(id + Increment, label, suggestedKey, preventInteractionOnGui, hint);
        }

        /// <summary>
        /// Gets or Sets whether the <see cref="SSKeybindSetting"/> would be shown and enabled on all pages or not.
        /// </summary>
        public bool IsGlobal { get; }

        /// <summary>
        /// The method that will be executed when the value is updated. It's contains two parameters: <br></br><br></br>
        /// - <see cref="ReferenceHub"/>, the player concerned by the change<br></br>
        /// - <see cref="bool"/>, True if button is pressed, False if not (triggered every time this value change).
        /// </summary>
        /// <remarks>No errors will be thrown if <see cref="Action"/> is null.</remarks>
        public Action<ReferenceHub, bool> Action { get; }

        /// <summary>
        /// The base instance (sent in to the client).
        /// </summary>
        public ServerSpecificSettingBase Base { get; }
    }
}