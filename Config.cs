using System.ComponentModel;
using SSMenuSystem.Features;
#if EXILED
using Exiled.API.Interfaces;
#endif

namespace SSMenuSystem
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

        /// <summary>
        /// Gets or sets a value indicating whether pins is allowed or not (pin is a thing that has been displayed on all menus).
        /// </summary>
        [Description("Whether pins is allowed or not (pin is a thing that has been displayed on all menus).")]
        public bool AllowPinnedContent { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether clients (= non-moderators) whould see errors or not.
        /// </summary>
        [Description("Whether clients (= non-moderators) whould see errors or not.")]
        public bool ShowErrorToClient { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether clients (= non-moderators) whould see total errors (= some plugins-content name) or not. HIGLY UNRECOMMENDED TO SET TRUE.
        /// </summary>
        [Description("Whether clients (= non-moderators) whould see total errors (= some plugins-content name) or not. HIGLY UNRECOMMENDED TO SET TRUE.")]
        public bool ShowFullErrorToClient { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether moderators (= has RA access) whould see total errors (= some plugins-content name).
        /// </summary>
        [Description("Whether moderators (= has RA access) whould see total errors (= some plugins-content name).")]
        public bool ShowFullErrorToModerators { get; set; } = true;

        /// <summary>
        /// If there is only one menu registered and this set to false, this menu would be automatiquely displayed. Disabled.
        /// </summary>
        [Description("If there is only one menu registered and this set to false, this menu would be automatiquely displayed. Disabled.")]
        public bool ForceMainMenuEventIfOnlyOne { get; set; }

        /// <summary>
        /// Because GlobalKeybinds is disabled, set this to false to remove the warning displayed.
        /// </summary>
        [Description("because GlobalKeybinds is disabled, set this to false to remove the warning displayed.")]
        public bool ShowGlobalKeybindingsWarning { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether examples is enabled. Warning: if set to true, some content of examples would be Game breaking (speed ability, scan ability, etc...)
        /// </summary>
        [Description("Whether examples is enabled. Warning: if set to true, some content of examples would be Game breaking (speed ability, scan ability, etc...).")]
        public bool EnableExamples { get; set; } = true;

        /// <summary>
        /// The comptability system config.
        /// </summary>
        public ComptabilityConfig ComptabilitySystem { get; set; } = new();

#if NWAPI
        /// <summary>
        /// Plugin translations.
        /// </summary>
        public Translation Translation { get; set; } = new();
#endif
    }
}