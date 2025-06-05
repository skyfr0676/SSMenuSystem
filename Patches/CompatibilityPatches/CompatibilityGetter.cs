// -----------------------------------------------------------------------
// <copyright file="CompatibilityGetter.cs" company="Skyfr0676 and Redforce04">
// Copyright (c) Skyfr0676 and Redforce04. All rights reserved.
// Licensed under the Undetermined license.
// </copyright>
// -----------------------------------------------------------------------

// ReSharper disable UnusedParameter.Local UnusedMember.Local
#pragma warning disable SA1010, SA1011, SA1118 // Square brackets should be spaced correctly. Opening Square brackets should not be proceeded with a space. Parameter must not span multiple lines.
namespace SSMenuSystem.Patches.CompatibilityPatches;

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NorthwoodLib.Pools;
using Features;
using UserSettings.ServerSpecific;
using static HarmonyLib.AccessTools;

/// <summary>
/// A compatibility getter patch.
/// </summary>
[HarmonyPatch(typeof(ServerSpecificSettingsSync), nameof(ServerSpecificSettingsSync.DefinedSettings), MethodType.Getter)]
internal static class CompatibilityGetter
{
    /// <summary>
    /// Gets the Server Specific Setting for an assembly.
    /// </summary>
    /// <param name="assembly">The assembly to search.</param>
    /// <returns>The server specific settings for an assembly.</returns>
    public static ServerSpecificSettingBase[] Get(Assembly assembly)
    {
        if (assembly == typeof(ReferenceHub).Assembly)
        {
            return [];
        }

        if (Menu.Menus.OfType<AssemblyMenu>().All(x => x.Assembly != assembly))
        {
            return [];
        }

        AssemblyMenu m = Menu.Menus.OfType<AssemblyMenu>().First(x => x.Assembly == assembly);
        return m.OverrideSettings ?? [];
    }

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent();

        newInstructions.InsertRange(
            0,
            [

            // CompatibilizerGetter.Get(Assembly.GetCallingAssembly());
            new (OpCodes.Call, Method(typeof(Assembly), nameof(Assembly.GetCallingAssembly))),
            new (OpCodes.Call, Method(typeof(CompatibilityGetter), nameof(Get))),
            new (OpCodes.Ret),
        ]);

        foreach (CodeInstruction z in newInstructions)
        {
            yield return z;
        }

        ListPool<CodeInstruction>.Shared.Return(newInstructions);
    }
}
#pragma warning restore SA1010, SA1011, SA1118
