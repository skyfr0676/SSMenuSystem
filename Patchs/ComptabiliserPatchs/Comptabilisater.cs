using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Mirror;
using NorthwoodLib.Pools;
using PluginAPI.Core;
using ServerSpecificSyncer.Features;
using UnityEngine;
using UserSettings.ServerSpecific;
using static HarmonyLib.AccessTools;

namespace ServerSpecificSyncer.Patchs.ComptabiliserPatchs
{
    [HarmonyPatch(typeof(ServerSpecificSettingsSync), nameof(ServerSpecificSettingsSync.DefinedSettings), MethodType.Setter)]
    public static class Comptabilisater
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions,
            ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent();
            newInstructions.InsertRange(0, new CodeInstruction[]
            {
                // Comptabilisater.Load(Assembly.GetCallingAssembly(), value);
                new(OpCodes.Call, Method(typeof(Assembly), nameof(Assembly.GetCallingAssembly))),
                new(OpCodes.Ldarg_0),
                new(OpCodes.Call, Method(typeof(Comptabilisater), nameof(Load))),
                new(OpCodes.Ret),
            });
        
            foreach (CodeInstruction z in newInstructions)
                yield return z;
        
            ListPool<CodeInstruction>.Shared.Return(newInstructions);
        }

        private static readonly HashSet<Assembly> LockedAssembly = new();
    
        public static void Load(Assembly assembly, ServerSpecificSettingBase[] settings)
        {
            if (LockedAssembly.Contains(assembly))
                return;

            if (Menu.Menus.OfType<AssemblyMenu>().Any(x => x.Assembly == assembly))
            {
                AssemblyMenu m = Menu.Menus.OfType<AssemblyMenu>().First(x => x.Assembly == assembly);
                m.OverrideSettings = settings;
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
            Log.Debug($"Started comptabilisation for assembly {menu.Name}.");

#if EXILED
        if (Exiled.Loader.Loader.Plugins.Any(x => x.Assembly == assembly))
            menu.Name = Exiled.Loader.Loader.Plugins.First(x => x.Assembly == assembly).Name;
#else
            if (PluginAPI.Loader.AssemblyLoader.Plugins.TryGetValue(assembly, out Dictionary<Type, PluginHandler> plugin))
                menu.Name = plugin.First().Value.PluginName;
#endif
            if (Menu.Menus.Any(x => x.Name == menu.Name))
            {
                Log.Warning($"assembly {name} tried to register by compatibilisation menu {menu.Name} but a menu already exist with this name. using assembly name...");
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
        }
    }
}