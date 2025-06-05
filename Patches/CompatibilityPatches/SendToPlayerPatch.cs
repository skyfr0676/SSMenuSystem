// -----------------------------------------------------------------------
// <copyright file="SendToPlayerPatch.cs" company="Skyfr0676 and Redforce04">
// Copyright (c) Skyfr0676 and Redforce04. All rights reserved.
// Licensed under the Undetermined license.
// </copyright>
// -----------------------------------------------------------------------

// ReSharper disable UnusedMember.Local UnusedParameter.Local
namespace SSMenuSystem.Patches.CompatibilityPatches;

using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NorthwoodLib.Pools;
using Features;
using UserSettings.ServerSpecific;
using static HarmonyLib.AccessTools;

using Log = Features.Log;

/// <summary>
/// Send to players patch.
/// </summary>
[HarmonyPatch(typeof(ServerSpecificSettingsSync), nameof(ServerSpecificSettingsSync.SendToPlayer), typeof(ReferenceHub), typeof(ServerSpecificSettingBase[]), typeof(int?))]
internal static class SendToPlayerPatch
{
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> transpiler, ILGenerator generator)
    {
        List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent();

        newInstructions.AddRange([
            new (OpCodes.Ldarg_0),
            new (OpCodes.Ldarg_1),
            new (OpCodes.Ldarg_2),
            new (OpCodes.Call, Method(typeof(Assembly), nameof(Assembly.GetCallingAssembly))),
            new (OpCodes.Call, Method(typeof(SendToPlayerPatch), nameof(SendToPlayer))),
            new (OpCodes.Ret),
        ]);

    #if false
            Label continueLabel = generator.DefineLabel();
            Label removePlayerLabel = generator.DefineLabel();

            LocalBuilder assembly = generator.DeclareLocal(typeof(Assembly));
            LocalBuilder menu = generator.DeclareLocal(typeof(AssemblyMenu));

            newInstructions.AddRange(new[]
            {
                // Assembly assembly = Assembly.GetCallingAssembly();
                new(OpCodes.Call, Method(typeof(Assembly), nameof(Assembly.GetCallingAssembly))),

                // Menu menu = Features.Utils.GetMenu(assembly);
                new(OpCodes.Stloc_S, assembly.LocalIndex),
                new(OpCodes.Call, Method(typeof(Features.Utils), nameof(Features.Utils.GetMenu))),
                new(OpCodes.Stloc_S, menu.LocalIndex),

                // if (menu == null)
                new(OpCodes.Ldloc_S, menu.LocalIndex),
                new(OpCodes.Ldnull),
                new(OpCodes.Brfalse_S, continueLabel),

                // [menu == null]
                //Log.Warning($"assembly {assembly.GetName().Name} tried to send a couple of {collection.Length} settings but doesn't have a valid/registered menu! creating new one...");
                new(OpCodes.Ldstr, "assembly {0} tried to send a couple of {1} settings but doesn't have a valid/registered menu! creating new one..."),
                new(OpCodes.Ldloc_S, assembly.LocalIndex),
                new(OpCodes.Callvirt, Method(typeof(Assembly), nameof(Assembly.GetName))),
                new(OpCodes.Callvirt, PropertyGetter(typeof(AssemblyName), nameof(AssemblyName.Name))),
                new(OpCodes.Ldarg_1),
                new(OpCodes.Callvirt, PropertyGetter(typeof(Array), nameof(Array.Length))),
                new(OpCodes.Callvirt, Method(typeof(int), nameof(int.ToString))),
                new(OpCodes.Call, Method(typeof(string),nameof(string.Format), new[]{ typeof(string), typeof(object), typeof(object) })),
                new(OpCodes.Ldnull),
                new(OpCodes.Call, Method(typeof(Log), nameof(Log.Warning))),

                // Compatibility.Load(assembly);
                new(OpCodes.Ldloc_S, assembly.LocalIndex),
                new(OpCodes.Ldarg_1),
                new(OpCodes.Call, Method(typeof(Comptabilisater), nameof(Comptabilisater.Load))),

                // [menu != null]
                // if (collection == null) menu.AuthorizedPlayers.Remove(hub);
                new CodeInstruction(OpCodes.Ldarg_0).WithLabels(continueLabel),
                new(OpCodes.Ldnull),
                new(OpCodes.Brtrue_S, removePlayerLabel),
                new(OpCodes.Ldloc_S, menu.LocalIndex),
                new(OpCodes.Callvirt, PropertyGetter(typeof(AssemblyMenu), nameof(AssemblyMenu.AuthorizedPlayers))),
                new(OpCodes.Ldarg_0),
                new(OpCodes.Callvirt, Method(typeof(HashSet<ReferenceHub>), nameof(HashSet<ReferenceHub>.Add))),
                new(OpCodes.Ret),

                // else menu.AuthorizedPlayers.Add(hub);
                new CodeInstruction(OpCodes.Ldloc_S, menu.LocalIndex).WithLabels(removePlayerLabel),
                new(OpCodes.Callvirt, PropertyGetter(typeof(AssemblyMenu), nameof(AssemblyMenu.AuthorizedPlayers))),
                new(OpCodes.Ldarg_0),
                new(OpCodes.Callvirt, Method(typeof(HashSet<ReferenceHub>), nameof(HashSet<ReferenceHub>.Remove))),
                new(OpCodes.Ret),
            });
    #endif

        foreach (CodeInstruction z in newInstructions)
        {
            yield return z;
        }

        ListPool<CodeInstruction>.Shared.Return(newInstructions);
    }

    private static void SendToPlayer(ReferenceHub hub, ServerSpecificSettingBase[] settings, int? versionOverride, Assembly assembly)
    {
        AssemblyMenu? menu = Utils.GetMenu(assembly);
        if (menu is null)
        {
            Log.Warn($"assembly {assembly.GetName().Name} tried to send a couple of {settings.Length} settings but doesn't have a valid/registered menu! creating new one...");
            Compatibility.Load([]);
            menu = Utils.GetMenu(assembly);
        }

        menu!.ActuallySentToClient[hub] = settings;

        if (Menu.GetCurrentPlayerMenu(hub) == menu)
        {
            menu.Reload(hub);
        }
        else
        {
            Menu.LoadForPlayer(hub, null);
        }
    }
}