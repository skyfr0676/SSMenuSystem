using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NorthwoodLib.Pools;
using ServerSpecificSyncer.Features;
using UserSettings.ServerSpecific;
using static HarmonyLib.AccessTools;

namespace ServerSpecificSyncer.Patchs.ComptabiliserPatchs
{
    [HarmonyPatch(typeof(ServerSpecificSettingsSync), nameof(ServerSpecificSettingsSync.DefinedSettings), MethodType.Getter)]
    public class ComptabilisaterGetter
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions,
            ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent();

            newInstructions.InsertRange(0, new CodeInstruction[]
            {
                // ComptabilisaterGetter.Get(Assembly.GetCallingAssembly());
                new(OpCodes.Call, Method(typeof(Assembly), nameof(Assembly.GetCallingAssembly))),
                new(OpCodes.Call, Method(typeof(ComptabilisaterGetter), nameof(Get))),
                new(OpCodes.Ret),
            });

            foreach (CodeInstruction z in newInstructions)
                yield return z;

            ListPool<CodeInstruction>.Shared.Return(newInstructions);
        }
    
        public static ServerSpecificSettingBase[] Get(Assembly assembly)
        {
            if (assembly == typeof(ReferenceHub).Assembly)
                return Array.Empty<ServerSpecificSettingBase>();
            if (!Menu.Menus.OfType<AssemblyMenu>().Any(x => x.Assembly == assembly)) return null;
            AssemblyMenu m = Menu.Menus.OfType<AssemblyMenu>().First(x => x.Assembly == assembly);
            return m.OverrideSettings;
            
        }
    }
}