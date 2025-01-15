namespace ServerSpecificSyncer.Configs
{
    public class LabelButton
    {
        /// <summary>
        /// The label of button (displayed at the left).
        /// </summary>
        public string Label { get; set; }
        
        /// <summary>
        /// The Button content (displayed on the button).
        /// </summary>
        public string ButtonText { get; set; }
        
        /// <summary>
        /// Initialize a new instance of <see cref="LabelButton"/>
        /// </summary>
        /// <param name="label">the label text.</param>
        /// <param name="buttonText">the button text.</param>
        public LabelButton(string label, string buttonText)
        {
            Label = label;
            ButtonText = buttonText;
        }

        /// <summary>
        /// Default constructor of <see cref="LabelButton"/>.
        /// </summary>
        public LabelButton()
        {
            Label = "MISSING_LABEL";
            ButtonText = "MISSING_VALUE";
        }
    }
}