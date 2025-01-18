#if EXILED
using Exiled.API.Features.Core.UserSettings;
using HarmonyLib;
// ReSharper disable RedundantAssignment
// ReSharper disable InconsistentNaming

namespace SSMenuSystem.Patchs
{
    /// <summary>
    /// Patch OriginalDefinition from EXILED to avoid NRE.
    /// </summary>
    [HarmonyPatch(typeof(SettingBase), nameof(SettingBase.OriginalDefinition), MethodType.Getter)]
    public static class ExiledPatch
    {
        private static bool Prefix(SettingBase __instance, ref SettingBase __result)
        {
            __result = null;
            return false;
        }
    }
}
#endif