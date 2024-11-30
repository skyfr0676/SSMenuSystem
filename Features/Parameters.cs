using System.Collections.Generic;
using System.Linq;
using UserSettings.ServerSpecific;

namespace ServerSpecificSyncer.Features
{
    public static class Parameters
    {
        private static readonly Dictionary<ReferenceHub, List<ServerSpecificSettingBase>> ParametersSync = new();
        public static IReadOnlyCollection<KeyValuePair<ReferenceHub, List<ServerSpecificSettingBase>>> SettingsSync => ParametersSync.ToList().AsReadOnly();
        public static void OnValueReceived(ReferenceHub hub, ServerSpecificSettingBase ss)
        {
            foreach (var settings in Menu.Menus.Select(x => x.Settings))
            {
                foreach (var setting in settings)
                {
                    if (setting.SettingId == ss.SettingId)
                    {
                        if (!ParametersSync.ContainsKey(hub))
                            ParametersSync.Add(hub, new());
                        if (ParametersSync[hub].Any(x => x.SettingId == ss.SettingId))
                            ParametersSync[hub].RemoveAll(x => x.SettingId == ss.SettingId);
                        ParametersSync[hub].Add(ss);
                    }
                }
            }
        }

        public static void GetParameter(ReferenceHub hub)
        {
        }
    }
}