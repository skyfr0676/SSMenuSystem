using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NorthwoodLib.Pools;
using ServerSpecificSyncer.Features;
using UserSettings.ServerSpecific;

namespace ServerSpecificSyncer.Patchs.ComptabiliserPatchs
{
    
    [HarmonyPatch(typeof(ServerSpecificSettingsSync), nameof(ServerSpecificSettingsSync.SendToPlayer), new[] { typeof(ReferenceHub) })]
    public class SendToPlayerDSPatch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> transpiler,
            ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent();
            
            newInstructions.Add(new CodeInstruction(OpCodes.Ret));

            foreach (CodeInstruction z in newInstructions)
                yield return z;

            ListPool<CodeInstruction>.Shared.Return(newInstructions);
        }
    }
}