using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using NorthwoodLib.Pools;
using UnityEngine;
using UserSettings.ServerSpecific;
using static HarmonyLib.AccessTools;

namespace SSMenuSystem.Patchs.CompatibilizerPatchs
{
    [HarmonyPatch(typeof(ServerSpecificSettingBase), nameof(ServerSpecificSettingBase.SetId))]
    internal class SetIdPatch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions,
            ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent(instructions);

            int index = newInstructions.FindIndex(x => x.Is(OpCodes.Call, Method(typeof(Mirror.Extensions), nameof(Mirror.Extensions.GetStableHashCode)))) + 1;

            newInstructions.Insert(index, new CodeInstruction(OpCodes.Call, Method(typeof(Mathf), nameof(Mathf.Abs), new[] {typeof(int)})));

            foreach (CodeInstruction z in newInstructions)
                yield return z;

            ListPool<CodeInstruction>.Shared.Return(newInstructions);
        }
    }
}