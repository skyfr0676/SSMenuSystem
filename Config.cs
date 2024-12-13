using System.ComponentModel;
#if EXILED
using Exiled.API.Interfaces;
#endif

namespace ServerSpecificSyncer
{
    /// <inheritdoc cref="IConfig"/>
    public class Config
#if EXILED
        : IConfig
#endif
    {
        /// <inheritdoc/>
        public bool IsEnabled { get; set; } = true;

        /// <inheritdoc/>
        public bool Debug { get; set; }
        public bool AllowPinnedContent { get; set; } = true;
        public bool ShowErrorToClient { get; set; } = true;
        public bool ShowFullErrorToClient { get; set; } = false;
        public bool ShowFullErrorToModerators { get; set; } = true;

#if DEBUG
        [Description("If there is only one menu registered and this set to false, this menu would be automatiquely displayed.")]
        public bool ForceMainMenuEventIfOnlyOne { get; set; }
#endif

#if NWAPI
        public Translation Translation { get; set; } = new();
#endif
    }
}