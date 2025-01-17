using System;
using SSMenuSystem.Features.Interfaces;
using UserSettings.ServerSpecific;

namespace SSMenuSystem.Features.Wrappers
{
    /// <summary>
    /// Initialize a new instance of wrapper for <see cref="SSButton"/>. This setting make a dropdown where player can select an input.
    /// </summary>
    public class Button : SSButton, ISetting
    {
        /// <summary>
        /// The method that will be executed when the index is updated. It's contains two parameters: <br></br><br></br>
        /// - <see cref="ReferenceHub"/>, the player concerned by the press.<br></br>
        /// - <see cref="SSButton"/>, where it's the class synced.
        /// </summary>
        public Action<ReferenceHub, SSButton> Action { get; }

        /// <summary>
        /// The base instance (sent in to the client).
        /// </summary>
        public ServerSpecificSettingBase Base { get; }

        /// <summary>
        /// Initialize a new instance of <see cref="Button"/>.
        /// </summary>
        /// <param name="id">The id of <see cref="SSButton"/>. value "null" and is not supported, even if you can set it to null, and it will result of client crash.</param>
        /// <param name="label">The label of <see cref="Button"/>. displayed at left in the row.</param>
        /// <param name="buttonText">The text displayed on the button.</param>
        /// <param name="onClick">Triggered when the player press the button.</param>
        /// <param name="holdTimeSeconds">When value is not equal to null, player will need to press specified time in seconds before the click will actually happen.</param>
        /// <param name="hint">The hint (located in "?"). If null, no hint will be displayed.</param>
        public Button(int? id, string label, string buttonText, Action<ReferenceHub, SSButton> onClick, float? holdTimeSeconds = null, string hint = null) : base(id, label, buttonText, holdTimeSeconds, hint)
        {
            Action = onClick;
            Base = new SSButton(id, label, buttonText, holdTimeSeconds, hint);
        }
    }
}