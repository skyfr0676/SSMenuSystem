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
        
        public bool ShowErrorToClient { get; set; } = true;
        public bool ShowFullErrorToClient { get; set; } = false;
        public bool ShowFullErrorToModerators { get; set; } = true;

#if NWAPI
        public Translation Translation { get; set; }
#endif
    }
}