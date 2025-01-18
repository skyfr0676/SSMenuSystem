#if NWAPI
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using MEC;
using NorthwoodLib.Pools;
using PluginAPI.Core;
using SSMenuSystem.Features;
using UnityEngine;
using static HarmonyLib.AccessTools;

namespace SSMenuSystem.Patchs
{
    [HarmonyPatch(typeof(ServerRoles), nameof(ServerRoles.SetGroup))]
    internal class NwapiSetGroup
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions,
            ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent(instructions);
            int offset = 1;
            int index = newInstructions.FindIndex(instruction => instruction.opcode == OpCodes.Ret) + offset;

            newInstructions.InsertRange(index, new[]
            {
                // NwapiSetGroup.OnChangingGroup(this.gameObject);
                new CodeInstruction(OpCodes.Ldarg_0).MoveLabelsFrom(newInstructions[index]),
                new(OpCodes.Call, PropertyGetter(typeof(ServerRoles), nameof(ServerRoles.gameObject))),
                new(OpCodes.Call, Method(typeof(NwapiSetGroup), nameof(OnChangingGroup))),
            });

            foreach (CodeInstruction t in newInstructions)
                yield return t;

            ListPool<CodeInstruction>.Shared.Return(newInstructions);
        }

        public static void OnChangingGroup(GameObject gameObject)
        {
            ReferenceHub hub = Player.Get(gameObject)?.ReferenceHub;
            if (hub != null)
                EventHandler.SyncChangedGroup(hub);
        }
    }
}
#endif