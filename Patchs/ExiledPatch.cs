#if EXILED
using Exiled.API.Features.Core.UserSettings;
using HarmonyLib;

namespace ServerSpecificSyncer.Patchs
{
    [HarmonyPatch(typeof(SettingBase), nameof(SettingBase.OriginalDefinition), MethodType.Getter)]
    public static class ExiledPatch
    {
        public static bool Prefix(SettingBase __instance, ref SettingBase __result)
        {
            __result = null;
            return false;
        }
    }
}
#endif