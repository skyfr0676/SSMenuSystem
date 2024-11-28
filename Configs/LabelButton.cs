namespace ServerSpecificSyncer.Configs
{
    public class LabelButton
    {
        public string Label { get; set; }
        public string ButtonText { get; set; }

        public LabelButton(string label, string buttonText)
        {
            Label = label;
            ButtonText = buttonText;
        }
    }
}