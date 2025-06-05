// -----------------------------------------------------------------------
// <copyright file="PrevalidateResponsePatch.cs" company="Skyfr0676 and Redforce04">
// Copyright (c) Skyfr0676 and Redforce04. All rights reserved.
// Licensed under the Undetermined license.
// </copyright>
// -----------------------------------------------------------------------

// ReSharper disable RedundantAssignment InconsistentNaming UnusedMember.Local UnusedParameter.Local
#pragma warning disable SA1313
namespace SSMenuSystem.Patches;

using HarmonyLib;
using UserSettings.ServerSpecific;

/// <summary>
/// Patch <see cref="ServerSpecificSettingsSync.ServerPrevalidateClientResponse"/> to avoid checking <see cref="ServerSpecificSettingsSync.DefinedSettings"/>.
/// </summary>
[HarmonyPatch(typeof(ServerSpecificSettingsSync), nameof(ServerSpecificSettingsSync.ServerPrevalidateClientResponse))]
internal static class PrevalidateResponsePatch
{
    private static bool Prefix(SSSClientResponse msg, ref bool __result)
    {
        __result = true;
        return false;
    }
}

#pragma warning restore SA1313
