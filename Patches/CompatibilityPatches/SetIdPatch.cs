// -----------------------------------------------------------------------
// <copyright file="SetIdPatch.cs" company="Skyfr0676 and Redforce04">
// Copyright (c) Skyfr0676 and Redforce04. All rights reserved.
// Licensed under the Undetermined license.
// </copyright>
// -----------------------------------------------------------------------

// ReSharper disable UnusedMember.Local UnusedParameter.Local
#pragma warning disable SA1010 // Opening square brackets should be spaced correctly
namespace SSMenuSystem.Patches.CompatibilityPatches;

using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using NorthwoodLib.Pools;
using UnityEngine;
using UserSettings.ServerSpecific;
using static HarmonyLib.AccessTools;

/// <summary>
/// Set ID Patch.
/// </summary>
[HarmonyPatch(typeof(ServerSpecificSettingBase), nameof(ServerSpecificSettingBase.SetId))]
internal static class SetIdPatch
{
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent(instructions);

        int index = newInstructions.FindIndex(x => x.Is(OpCodes.Call, Method(typeof(Mirror.Extensions), nameof(Mirror.Extensions.GetStableHashCode)))) + 1;

        newInstructions.Insert(index, new CodeInstruction(OpCodes.Call, Method(typeof(Mathf), nameof(Mathf.Abs), [typeof(int)])));

        foreach (CodeInstruction z in newInstructions)
        {
            yield return z;
        }

        ListPool<CodeInstruction>.Shared.Return(newInstructions);
    }
}
#pragma warning restore SA1010
