// -----------------------------------------------------------------------
// <copyright file="ExiledPatch.cs" company="Skyfr0676 and Redforce04">
// Copyright (c) Skyfr0676 and Redforce04. All rights reserved.
// Licensed under the Undetermined license.
// </copyright>
// -----------------------------------------------------------------------

#if EXILED

// ReSharper disable RedundantAssignment InconsistentNaming UnusedMember.Local UnusedParameter.Local
#pragma warning disable SA1313, CS8625 // Parameter names should begin with lower-case letter. Cannot convert null literal to non-nullable reference type.
namespace SSMenuSystem.Patches;

using Exiled.API.Features.Core.UserSettings;
using HarmonyLib;

/// <summary>
/// Patch OriginalDefinition from EXILED to avoid NRE.
/// </summary>
[HarmonyPatch(typeof(SettingBase), nameof(SettingBase.OriginalDefinition), MethodType.Getter)]
internal static class ExiledPatch
{
    private static bool Prefix(SettingBase __instance, ref SettingBase __result)
    {
        __result = null;
        return false;
    }
}


#pragma warning restore SA1313, CS8625 // Cannot convert null literal to non-nullable reference type.
#endif
