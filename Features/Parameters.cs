using System.Collections.Generic;
using System.Linq;
using UserSettings.ServerSpecific;

namespace ServerSpecificSyncer.Features
{
    //TODO: MAKE THE PARAMETERS
    public static class Parameters
    {
        private static readonly Dictionary<ReferenceHub, List<ServerSpecificSettingBase>> ParametersSync = new();
        public static IReadOnlyCollection<KeyValuePair<ReferenceHub, List<ServerSpecificSettingBase>>> SettingsSync => ParametersSync.ToList().AsReadOnly();

        public static void GetParameter(ReferenceHub hub)
        {
        }
    }
}