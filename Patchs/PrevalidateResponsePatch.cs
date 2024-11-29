using HarmonyLib;
using UserSettings.ServerSpecific;

namespace ServerSpecificSyncer.Patchs
{
    /// <summary>
    /// Patch <see cref="ServerSpecificSettingsSync.ServerPrevalidateClientResponse"/> to avoid checking <see cref="ServerSpecificSettingsSync.DefinedSettings"/>.
    /// </summary>
    [HarmonyPatch(typeof(ServerSpecificSettingsSync), nameof(ServerSpecificSettingsSync.ServerPrevalidateClientResponse))]
    internal class PrevalidateResponsePatch
    {
        private static bool Prefix(SSSClientResponse msg, ref bool __result)
        {
            __result = true;
            return false;
        }
    }
}