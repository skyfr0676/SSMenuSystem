using System;
using SSMenuSystem.Features.Interfaces;
using UserSettings.ServerSpecific;

namespace SSMenuSystem.Features.Wrappers
{
    /// <summary>
    /// Initialize a new instance of wrapper for <see cref="SSDropdownSetting"/>. This setting make a dropdown where player can select an input.
    /// </summary>
    public class Dropdown : SSDropdownSetting, ISetting
    {

        /// <summary>
        /// The method that will be executed when the index is updated. It's contains four parameters: <br></br><br></br>
        /// - <see cref="ReferenceHub"/>, the player concerned by the change<br></br>
        /// - <see cref="string"/>, The new selected text, according to the new index. <br></br>
        /// - <see cref="int"/>, The new selected index. <br></br>
        /// - <see cref="SSPlaintextSetting"/>, where it's the class synced.
        /// </summary>
        /// <remarks>No errors will be thrown if <see cref="Action"/> is null.</remarks>
        public Action<ReferenceHub, string, int, SSDropdownSetting> Action { get; }

        /// <summary>
        /// The base instance (sent in to the client).
        /// </summary>
        public ServerSpecificSettingBase Base { get; set; }

        /// <summary>
        /// Initialize a new instance of <see cref="Dropdown"/>.
        /// </summary>
        /// <param name="id">The id of <see cref="SSDropdownSetting"/>. value "null" and is not supported, even if you can set it to null, and it will result of client crash.</param>
        /// <param name="label">The label of <see cref="Dropdown"/>. displayed at left in the row.</param>
        /// <param name="options">All options avaiable.</param>
        /// <param name="onChanged">Triggered when the player selected a new index.</param>
        /// <param name="defaultOptionIndex">Select per default an option, according to <see cref="SSDropdownSetting.Options"/>.</param>
        /// <param name="entryType">The different avaiable types NW gave.</param>
        /// <param name="hint">The hint (located in "?"). If null, no hint will be displayed.</param>
        public Dropdown(int? id, string label, string[] options, Action<ReferenceHub, string, int, SSDropdownSetting> onChanged = null, int defaultOptionIndex = 0, DropdownEntryType entryType = DropdownEntryType.Regular, string hint = null) : base(id, label, options, defaultOptionIndex, entryType, hint)
        {
            Action = onChanged;
            Base = new SSDropdownSetting(id, label, options, defaultOptionIndex, entryType, hint);
        }
    }
}