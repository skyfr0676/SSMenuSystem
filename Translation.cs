using ServerSpecificSyncer.Configs;
#if EXILED
using Exiled.API.Interfaces;
#endif

namespace ServerSpecificSyncer
{
    public class Translation
#if EXILED
        : ITranslation
#endif
    {
        public LabelButton OpenMenu { get; set; } = new("Open {0}", "Open");
        public LabelButton ReturnToMenu { get; set; } = new("Return to menu", "Return");
        public LabelButton ReturnTo { get; set; } = new("Return to {0}", "Return");
        public LabelButton ReloadButton { get; set; } = new("Reload menus", "Reload");
        public string ServerError { get; set; } = "INTERNAL SERVER ERROR";
        public string NoPermission { get; set; } = "insufficient permissions to see the full errors";
    }
}