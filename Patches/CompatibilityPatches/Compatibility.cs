// -----------------------------------------------------------------------
// <copyright file="Compatibility.cs" company="Skyfr0676 and Redforce04">
// Copyright (c) Skyfr0676 and Redforce04. All rights reserved.
// Licensed under the Undetermined license.
// </copyright>
// -----------------------------------------------------------------------

#pragma warning disable SA1118
namespace SSMenuSystem.Patches.CompatibilityPatches;

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Mirror;
using NorthwoodLib.Pools;
using Features;
using UnityEngine;
using UserSettings.ServerSpecific;
using static HarmonyLib.AccessTools;

using Log = Features.Log;

/// <summary>
/// Patches for compatibility.
/// </summary>
[HarmonyPatch(typeof(ServerSpecificSettingsSync), nameof(ServerSpecificSettingsSync.DefinedSettings), MethodType.Setter)]
internal static class Compatibility
{
    private static readonly HashSet<Assembly> LockedAssembly = new ();

    /// <summary>
    /// Loads the SpecificServerSettings.
    /// </summary>
    /// <param name="settings">The settings to load.</param>
    internal static void Load(ServerSpecificSettingBase[] settings)
    {
        if (!Plugin.Instance!.Config.CompatibilitySystem.CompatibilityEnabled)
        {
            return;
        }

        Assembly assembly = Assembly.GetCallingAssembly();
        Log.Debug(assembly.GetName().Name + " tried to set " + nameof(ServerSpecificSettingsSync.DefinedSettings) + ". Game Assembly: " + typeof(ReferenceHub).Assembly.GetName().Name);
        if (LockedAssembly.Contains(assembly) || assembly == typeof(ReferenceHub).Assembly)
        {
            Log.Debug("Assembly is locked or is a part of base game. Skipping...");
            return;
        }

        if (Menu.Menus.OfType<AssemblyMenu>().Any(x => x.Assembly == assembly))
        {
            AssemblyMenu m = Menu.Menus.OfType<AssemblyMenu>().First(x => x.Assembly == assembly);
            m.OverrideSettings = settings;
            if (m.OverrideSettings?.First() is SSGroupHeader)
            {
                m.Name = m.OverrideSettings.First().Label;
                m.OverrideSettings = m.OverrideSettings.Skip(1).ToArray();
            }

            foreach (ReferenceHub hub in ReferenceHub.AllHubs.Where(x => Menu.GetCurrentPlayerMenu(x) == null))
            {
                Menu.LoadForPlayer(hub, null);
            }

            m.ReloadAll();
            return;
        }

        string name = assembly.GetName().Name;

        AssemblyMenu menu = new ()
        {
            Assembly = assembly,
            OverrideSettings = settings,
            Name = name,
        };

        if (menu.OverrideSettings?.First() is SSGroupHeader)
        {
            menu.Name = menu.OverrideSettings.First().Label;
            menu.OverrideSettings = menu.OverrideSettings.Skip(1).ToArray();
        }
        else if (LabApi.Loader.PluginLoader.Plugins.Any(x => x.Value == assembly))
        {
            menu.Name = LabApi.Loader.PluginLoader.Plugins.First(x => x.Value == assembly).Key.Name;
        }
        else if (LabApi.Loader.PluginLoader.Plugins.Any(x => x.Key.Name == "Exiled Loader") && Exiled.Loader.Loader.Plugins.Any(x => x.Assembly == assembly))
        {
            menu.Name = Exiled.Loader.Loader.Plugins.First(x => x.Assembly == assembly).Name;
        }

        if (Menu.Menus.Any(x => x.Name == menu.Name))
        {
            Log.Warn($"Assembly {name} tried to register with the compatibility interface [menu {menu.Name}] but a menu already exists with this name. Using assembly name...");
            menu.Name = name;
        }

        if (Menu.Menus.Any(x => x.Name == menu.Name))
        {
            Log.Error($"Assembly {name} tried to register with the compatibility interface but a menu was already registered with this name. Aborting.");
            LockedAssembly.Add(assembly);
            return;
        }

        menu.Id = -Mathf.Abs(menu.Name.GetStableHashCode());
        Menu.Register(menu);
        foreach (ReferenceHub hub in ReferenceHub.AllHubs.Where(x => Menu.GetCurrentPlayerMenu(x) == null))
        {
            Menu.LoadForPlayer(hub, null);
        }
    }

    // ReSharper disable UnusedMember.Local UnusedParameter.Local
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent();
        newInstructions.InsertRange(
            0,
            [

                // Compatibility.Load(value);
                new (OpCodes.Ldarg_0),
                new (OpCodes.Call, Method(typeof(Compatibility), nameof(Load))),
                new (OpCodes.Ret),
            ]);

        foreach (CodeInstruction z in newInstructions)
        {
            yield return z;
        }

        ListPool<CodeInstruction>.Shared.Return(newInstructions);
    }
}
#pragma warning restore SA1118
