using System.ComponentModel;
using SSMenuSystem.Configs;
#if EXILED
using Exiled.API.Interfaces;
#endif

namespace SSMenuSystem
{
    /// <summary>
    /// The translation configs.
    /// </summary>
    public class Translation
#if EXILED
        : ITranslation
#endif
    {
        /// <summary>
        /// On the main-menu, button displayed to open a menu where {0} = menu name. 
        /// </summary>
        [Description("On the main-menu, button displayed to open a menu where {0} = menu name.")]
        public LabelButton OpenMenu { get; set; } = new("Open {0}", "Open");
        
        /// <summary>
        /// the button displayed when menu is opened.
        /// </summary>
        [Description("the button displayed when menu is opened.")]
        public LabelButton ReturnToMenu { get; set; } = new("Return to menu", "Return");
        
        /// <summary>
        /// The button that displayed when sub-menu is opened (return to related menu) where {0} = menu name.
        /// </summary>
        [Description("The button that displayed when sub-menu is opened (return to related menu) where {0} = menu name.")]
        public LabelButton ReturnTo { get; set; } = new("Return to {0}", "Return");
        
        /// <summary>
        /// The reload button.
        /// </summary>
        [Description("The reload button.")]
        public LabelButton ReloadButton { get; set; } = new("Reload menus", "Reload");
        
        /// <summary>
        /// Text displayed when an error is occured (to avoid client crash + explain why it's don't work). Can accept TextMeshPro tags.
        /// </summary>
        [Description("Text displayed when an error is occured (to avoid client crash + explain why it's don't work). Can accept TextMeshPro tags.")]
        public string ServerError { get; set; } = "INTERNAL SERVER ERROR";
        
        /// <summary>
        /// Title of sub-menus when there is one.
        /// </summary>
        [Description("Title of sub-menus when there is one.")]
        public string SubMenuTitle { get; set; } = "Sub-Menus";

        /// <summary>
        /// Translation when player doesn't have permission to see total errors (= see a part of code name).
        /// </summary>
        [Description("Translation when player doesn't have permission to see total errors (= see a part of code name).")]
        public string NoPermission { get; set; } = "insufficient permissions to see the full errors";
    }
}