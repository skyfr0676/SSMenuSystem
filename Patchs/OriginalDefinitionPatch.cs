using System.Linq;
using HarmonyLib;
using ServerSpecificSyncer.Features;
using ServerSpecificSyncer.Features.Interfaces;
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
                    if (ss is ISetting setting)
                    {
                        __result = setting.Base;
                        return false;
                    }
                    __result = ss;
                    return false;
                }
            }
            
            __result = null;
            return false;
        }
    }
}