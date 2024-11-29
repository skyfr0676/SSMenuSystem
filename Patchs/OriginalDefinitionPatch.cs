using System.Linq;
using HarmonyLib;
using ServerSpecificSyncer.Features;
using ServerSpecificSyncer.Features.Wrappers;
using UserSettings.ServerSpecific;

namespace ServerSpecificSyncer.Patchs
{
    [HarmonyPatch(typeof(ServerSpecificSettingBase), nameof(ServerSpecificSettingBase.OriginalDefinition), MethodType.Getter)]
    public class OriginalDefinitionPatch
    {
        public static bool Prefix(ServerSpecificSettingBase __instance, ref ServerSpecificSettingBase __result)
        {
            foreach (var ss in Menu.Menus.Select(x => x.Settings).SelectMany(x => x))
            {
                if (ss.SettingId == __instance.SettingId)
                {
                    if (ss is Plaintext plaintext)
                    {
                        __result = plaintext.Base;
                        return false;
                    }
                }
            }

            return true;
        }
    }
}