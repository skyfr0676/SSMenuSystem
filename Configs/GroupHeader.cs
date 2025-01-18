namespace SSMenuSystem.Configs
{
    /// <summary>
    /// Button labels config.
    /// </summary>
    public class GroupHeader
    {
        /// <summary>
        /// The label of button (displayed at the left).
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// The Button content (displayed on the button).
        /// </summary>
        public string Hint { get; set; }

        /// <summary>
        /// Initialize a new instance of <see cref="LabelButton"/>
        /// </summary>
        /// <param name="label">the label text.</param>
        /// <param name="hint">the button text.</param>
        public GroupHeader(string label, string hint)
        {
            Label = label;
            Hint = hint;
        }

        /// <summary>
        /// Default constructor of <see cref="LabelButton"/>.
        /// </summary>
        public GroupHeader()
        {
            Label = "MISSING_LABEL";
            Hint = "MISSING_HINT";
        }
    }
}