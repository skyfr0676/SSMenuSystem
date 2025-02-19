using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using UserSettings.ServerSpecific;

namespace SSMenuSystem.Patchs.CompatibiliserPatchs
{

    [HarmonyPatch(typeof(ServerSpecificSettingsSync), nameof(ServerSpecificSettingsSync.SendToPlayer), new[] { typeof(ReferenceHub) })]
    internal class SendToPlayerDSPatch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> transpiler,
            ILGenerator generator)
        {
            yield return new CodeInstruction(OpCodes.Ret);
        }
    }
}