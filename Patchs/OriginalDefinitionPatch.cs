using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using NorthwoodLib.Pools;
using ServerSpecificSyncer.Features;
using ServerSpecificSyncer.Features.Interfaces;
using UserSettings.ServerSpecific;
using static HarmonyLib.AccessTools;

namespace ServerSpecificSyncer.Patchs
{
    [HarmonyPatch(typeof(ServerSpecificSettingBase), nameof(ServerSpecificSettingBase.OriginalDefinition), MethodType.Getter)]
    public class OriginalDefinitionPatch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions,
            ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent();
            
            newInstructions.AddRange(new CodeInstruction[]
            {
                new (OpCodes.Ldarg_0),
                new (OpCodes.Callvirt, PropertyGetter(typeof(ServerSpecificSettingBase), nameof(ServerSpecificSettingBase.SettingId))),
                new (OpCodes.Call, Method(typeof(OriginalDefinitionPatch), nameof(GetFirstSetting))),
                new (OpCodes.Ret),
            });
            
            for (int z = 0; z < newInstructions.Count; z++)
                yield return newInstructions[z];
            ListPool<CodeInstruction>.Shared.Return(newInstructions);
        }

        public static ServerSpecificSettingBase GetFirstSetting(int id)
        {
            foreach (var menu in Menu.Menus)
            {
                foreach (var ss in menu.Settings)
                {
                    int settingId = ss.SettingId + menu.Hash;
                    if (settingId != id) continue;
                    if (ss is ISetting setting)
                        return setting.Base;
                    return ss;
                }
            }
            return null;
        }
    }
}