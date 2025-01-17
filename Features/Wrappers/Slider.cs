using System;
using SSMenuSystem.Features.Interfaces;
using UserSettings.ServerSpecific;

namespace SSMenuSystem.Features.Wrappers
{
    /// <summary>
    /// Initialize a new instance of wrapper for <see cref="SSSliderSetting"/>. This setting make a slider. Every time the value is changed (so even if the player continue clicking on the slider), the value will be updated and <see cref="Action"/> triggered.
    /// </summary>
    public class Slider : SSSliderSetting, ISetting
    {
        /// <summary>
        /// The method that will be executed when the value is updated. It's contains three parameters: <br></br><br></br>
        /// - <see cref="ReferenceHub"/>, the player concerned by the change<br></br>
        /// - <see cref="float"/>, The new value specified by <see cref="ReferenceHub"/><br></br>
        /// - <see cref="SSSliderSetting"/>, where it's the class synced.
        /// </summary>
        /// <remarks>No errors will be thrown if <see cref="Action"/> is null.</remarks>

        public Action<ReferenceHub, float, SSSliderSetting> Action { get; }

        /// <summary>
        /// The base instance (sent in to the client).
        /// </summary>
        public ServerSpecificSettingBase Base { get; set; }

        /// <summary>
        /// Initialize a new instance of <see cref="Slider"/>.
        /// </summary>
        /// <param name="id">The id of <see cref="SSSliderSetting"/>. value "null" and is not supported, even if you can set it to null, and it will result of client crash. </param>
        /// <param name="label">The label of <see cref="Slider"/>. displayed at left in the row.</param>
        /// <param name="minValue">The minimum value <see cref="Slider"/> can support. Does not change the size.</param>
        /// <param name="maxValue">The maximum value <see cref="Slider"/> can support. Does not change the size.</param>
        /// <param name="onChanged">Triggered when the player update the value.</param>
        /// <param name="defaultValue">The default value of <see cref="Slider"/>. It is between <see cref="SSSliderSetting.MinValue"/> and <see cref="SSSliderSetting.MaxValue"/></param>
        /// <param name="integer">Declare if the slider can only accept integer (so there is no dot or decimal value on the <see cref="SSSliderSetting.SyncFloatValue"/>.</param>
        /// <param name="valueToStringFormat">The slider contains a little text on the left, that contains the value of the slider. The value returned will be in this format (see <see cref="float"/>::<see cref="float.ToString(string)"/> for more informations).</param>
        /// <param name="finalDisplayFormat">The slider contains a little text on the left, what is the format of this ? (Example: if <see cref="SSSliderSetting.FinalDisplayFormat"/>) is equal to "{0}%", <see cref="SSSliderSetting.ValueToStringFormat"/> is queal to "0.0", and value is 50.647874, this litle plaintext will show "50.7%").</param>
        /// <param name="hint">The hint (located in "?"). If null, no hint will be displayed.</param>
        public Slider(int? id, string label, float minValue, float maxValue, Action<ReferenceHub, float, SSSliderSetting> onChanged = null, float defaultValue = 0, bool integer = false, string valueToStringFormat = "0.##", string finalDisplayFormat = "{0}", string hint = null) : base(id, label, minValue, maxValue, defaultValue, integer, valueToStringFormat, finalDisplayFormat, hint)
        {
            Base = new SSSliderSetting(id, label, minValue, maxValue, defaultValue, integer, valueToStringFormat, finalDisplayFormat,
                hint);
            Action = onChanged;
        }
    }
}