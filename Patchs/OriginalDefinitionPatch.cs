using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using NorthwoodLib.Pools;
using SSMenuSystem.Features;
using SSMenuSystem.Features.Interfaces;
using UserSettings.ServerSpecific;
using static HarmonyLib.AccessTools;

namespace SSMenuSystem.Patchs
{
    /// <summary>
    /// A patch for OriginalDefinition.
    /// </summary>
    [HarmonyPatch(typeof(ServerSpecificSettingBase), nameof(ServerSpecificSettingBase.OriginalDefinition), MethodType.Getter)]
    internal class OriginalDefinitionPatch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions,
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

        /// <summary>
        /// Get the first setting correspondig to the <see cref="Menu.Id"/>.
        /// </summary>
        /// <param name="id">id of <see cref="ServerSpecificSettingBase"/>.</param>
        /// <returns><see cref="ServerSpecificSettingBase"/> if found, null if not.</returns>
        private static ServerSpecificSettingBase GetFirstSetting(int id)
        {
            foreach (Menu menu in Menu.Menus)
            {
                foreach (ServerSpecificSettingBase ss in menu.Settings)
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