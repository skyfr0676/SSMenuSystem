using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NorthwoodLib.Pools;
using SSMenuSystem.Features;
using UserSettings.ServerSpecific;
using static HarmonyLib.AccessTools;

namespace SSMenuSystem.Patchs.CompatibilizerPatchs
{
    [HarmonyPatch(typeof(ServerSpecificSettingsSync), nameof(ServerSpecificSettingsSync.DefinedSettings), MethodType.Getter)]
    internal class CompatibilizerGetter
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions,
            ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent();

            newInstructions.InsertRange(0, new CodeInstruction[]
            {
                // CompatibilizerGetter.Get(Assembly.GetCallingAssembly());
                new(OpCodes.Call, Method(typeof(Assembly), nameof(Assembly.GetCallingAssembly))),
                new(OpCodes.Call, Method(typeof(CompatibilizerGetter), nameof(Get))),
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