using Exiled.API.Interfaces;

namespace ServerSpecificSyncer;

public class Translation
#if EXILED
    : ITranslation
#endif
{
    public LabelButton OpenMenu { get; set; } = new("Open {0}", "Open");
    public LabelButton ReturnToMenu { get; set; } = new("Return to menu", "Open");
    public LabelButton ReturnTo { get; set; } = new("Return to {0}", "Open");
    public string OpenButton { get; set; } = "Open";
    public string ServerError { get; set; } = "INTERNAL SERVER ERROR";
    public bool ShowErrorToClient { get; set; } = true;
    public bool ShowFullErrorToClient { get; set; } = false;
    public bool ShowFullErrorToModerators { get; set; } = true;
    public string NoPermission { get; set; } = "insufficient permissions to see the full errors";
}

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