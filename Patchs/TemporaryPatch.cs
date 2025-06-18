using HarmonyLib;
using UserSettings.ServerSpecific;

namespace SSMenuSystem.Patchs
{
    [HarmonyPatch(typeof(SSPlaintextSetting), nameof(SSPlaintextSetting.CharacterLimitOriginal), MethodType.Getter)]
    internal class TemporaryPatch
    {
        internal static bool Prefix(SSPlaintextSetting __instance, ref int __result)
        {
            __result = 64;
            return false;
        }
    }
}