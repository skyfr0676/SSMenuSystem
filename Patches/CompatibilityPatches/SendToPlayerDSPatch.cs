// -----------------------------------------------------------------------
// <copyright file="SendToPlayerDSPatch.cs" company="Skyfr0676 and Redforce04">
// Copyright (c) Skyfr0676 and Redforce04. All rights reserved.
// Licensed under the Undetermined license.
// </copyright>
// -----------------------------------------------------------------------

// ReSharper disable UnusedParameter.Local UnusedMember.Local
namespace SSMenuSystem.Patches.CompatibilityPatches
{
    using System.Collections.Generic;
    using System.Reflection.Emit;

    using HarmonyLib;
    using UserSettings.ServerSpecific;

    /// <summary>
    /// Send to player DS Patch.
    /// </summary>
    [HarmonyPatch(typeof(ServerSpecificSettingsSync), nameof(ServerSpecificSettingsSync.SendToPlayer), typeof(ReferenceHub))]
    internal static class SendToPlayerDSPatch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> transpiler, ILGenerator generator)
        {
            yield return new CodeInstruction(OpCodes.Ret);
        }
    }
}