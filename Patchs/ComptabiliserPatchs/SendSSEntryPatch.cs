using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Mirror;
using NorthwoodLib.Pools;
using PluginAPI.Core;
using ServerSpecificSyncer.Features;
using static HarmonyLib.AccessTools;
using UserSettings.ServerSpecific;

namespace ServerSpecificSyncer.Patchs.ComptabiliserPatchs;

[HarmonyPatch(typeof(NetworkConnection), nameof(NetworkConnection.Send), typeof(SSSEntriesPack), typeof(int))]
public static class SendSSEntryPatch
{
    private static Assembly _assembly;
    public static Assembly Assembly => _assembly ??= Assembly.GetExecutingAssembly();
    
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> transpiler,
        ILGenerator generator)
    {
        List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent(transpiler);

        Label continueLabel = generator.DefineLabel();
        newInstructions[0].WithLabels(continueLabel);
        
        
        newInstructions.InsertRange(0, new CodeInstruction[]
        {
            // // Assembly.GetCallingAssembly() should always return the Assembly-CSharp.
            // if (Assembly.GetCallingAssembly() != SendSSEntryPatch.Assembly) return;
            new(OpCodes.Call, Method(typeof(SendSSEntryPatch), nameof(Assembly))),
            new(OpCodes.Call, Method(typeof(Assembly), nameof(Assembly.GetCallingAssembly))),
            new(OpCodes.Brtrue_S, continueLabel),
            new(OpCodes.Ret),
        });

        foreach (CodeInstruction z in newInstructions)
            yield return z;

        ListPool<CodeInstruction>.Shared.Return(newInstructions);
    }
}