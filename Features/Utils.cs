// -----------------------------------------------------------------------
// <copyright file="Utils.cs" company="Skyfr0676 and Redforce04">
// Copyright (c) Skyfr0676 and Redforce04. All rights reserved.
// Licensed under the Undetermined license.
// </copyright>
// -----------------------------------------------------------------------

namespace SSMenuSystem.Features;

using System.Linq;
using System.Reflection;

using UserSettings.ServerSpecific;

/// <summary>
/// Utility Methods.
/// </summary>
internal static class Utils
{
    /// <summary>
    /// Sends a server specific setting to a player.
    /// </summary>
    /// <param name="hub">The players reference hub.</param>
    /// <param name="relatedMenu">The related menu.</param>
    /// <param name="collection">The collection of server specific settings to send.</param>
    /// <param name="versionOverride">The version to override.</param>
    internal static void SendToPlayer(ReferenceHub hub, Menu? relatedMenu, ServerSpecificSettingBase[] collection, int? versionOverride = null)
    {
        if (relatedMenu != null)
        {
            foreach (ServerSpecificSettingBase c in collection)
            {
                if (c is SSGroupHeader && c.Label == Plugin.Instance!.Translation.GlobalKeybindingTitle.Label && c.HintDescription == Plugin.Instance.Translation.GlobalKeybindingTitle.Hint)
                {
                    break;
                }

                if (c.SettingId < relatedMenu.Hash)
                {
                    c.SetId(c.SettingId + relatedMenu.Hash, c.Label);
                }
            }
        }

        hub.connectionToClient.Send(new SSSEntriesPack(collection, versionOverride ?? ServerSpecificSettingsSync.Version));
    }

    /// <summary>
    /// Gets a menu from an assembly.
    /// </summary>
    /// <param name="assembly">The assembly to search.</param>
    /// <returns>The menu if found or null.</returns>
    internal static AssemblyMenu? GetMenu(Assembly assembly) => Menu.Menus.OfType<AssemblyMenu>().FirstOrDefault(x => x.Assembly == assembly);
}