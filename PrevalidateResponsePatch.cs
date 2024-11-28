using HarmonyLib;
using UserSettings.ServerSpecific;

namespace ServerSpecificSyncer
{
    [HarmonyPatch(typeof(ServerSpecificSettingsSync), nameof(ServerSpecificSettingsSync.ServerPrevalidateClientResponse))]
    public class PrevalidateResponsePatch
    {
        public static bool Prefix(SSSClientResponse msg, ref bool __result)
        {
            __result = true;
            return false;
        }
    }
}