using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Mirror;
using NorthwoodLib.Pools;
using SSMenuSystem.Features;
using UnityEngine;
using UserSettings.ServerSpecific;
using static HarmonyLib.AccessTools;
using Log = SSMenuSystem.Features.Log;

namespace SSMenuSystem.Patchs.CompatibilizerPatchs
{
    [HarmonyPatch(typeof(ServerSpecificSettingsSync), nameof(ServerSpecificSettingsSync.DefinedSettings), MethodType.Setter)]
    internal static class Compatibilizer
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions,
            ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent();
            newInstructions.InsertRange(0, new CodeInstruction[]
            {
                // Comptabilisater.Load(value);
                new(OpCodes.Ldarg_0),
                new(OpCodes.Call, Method(typeof(Compatibilizer), nameof(Load))),
                new(OpCodes.Ret),
            });

            foreach (CodeInstruction z in newInstructions)
                yield return z;

            ListPool<CodeInstruction>.Shared.Return(newInstructions);
        }

        private static readonly HashSet<Assembly> LockedAssembly = new();

        internal static void Load(ServerSpecificSettingBase[] settings)
        {
            if (!Plugin.Instance.Config.ComptabilitySystem.ComptabilityEnabled)
                return;
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
                    Menu.LoadForPlayer(hub, null);
                m.ReloadAll();
                return;
            }

            string name = assembly.GetName().Name;

            AssemblyMenu menu = new()
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
                menu.Name = LabApi.Loader.PluginLoader.Plugins.First(x => x.Value == assembly).Key.Name;
            else if (LabApi.Loader.PluginLoader.Plugins.Any(x => x.Key.Name == "Exiled Loader") && Exiled.Loader.Loader.Plugins.Any(x => x.Assembly == assembly))
                menu.Name = Exiled.Loader.Loader.Plugins.First(x => x.Assembly == assembly).Name;

            if (Menu.Menus.Any(x => x.Name == menu.Name))
            {
                Log.Warn($"assembly {name} tried to register by compatibilisation menu {menu.Name} but a menu already exist with this name. using assembly name...");
                menu.Name = name;
            }

            if (Menu.Menus.Any(x => x.Name == menu.Name))
            {
                Log.Error($"assembly {name} tried to register by compatibilisation but a menu was already registered with this name. Aborting needed.");
                LockedAssembly.Add(assembly);
                return;
            }

            menu.Id = -Mathf.Abs(menu.Name.GetStableHashCode());
            Menu.Register(menu);
            foreach (ReferenceHub hub in ReferenceHub.AllHubs.Where(x => Menu.GetCurrentPlayerMenu(x) == null))
                Menu.LoadForPlayer(hub, null);
        }
    }
}