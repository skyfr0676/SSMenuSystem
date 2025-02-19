using System.Linq;
using System.Reflection;
using UserSettings.ServerSpecific;

namespace SSMenuSystem.Features
{
    internal static class Utils
    {
        internal static void SendToPlayer(ReferenceHub hub, Menu relatedMenu, ServerSpecificSettingBase[] collection, int? versionOverride = null)
        {
            if (relatedMenu != null)
            {
                foreach (ServerSpecificSettingBase c in collection)
                {
                    if (c is SSGroupHeader && c.Label == Plugin.GetTranslation().GlobalKeybindingTitle.Label && c.HintDescription == Plugin.GetTranslation().GlobalKeybindingTitle.Hint)
                        break;
                    if (c.SettingId < relatedMenu.Hash)
                        c.SetId(c.SettingId + relatedMenu.Hash, c.Label);
                }
            }
            hub.connectionToClient.Send(new SSSEntriesPack(collection, versionOverride ?? ServerSpecificSettingsSync.Version));
        }

        internal static AssemblyMenu GetMenu(Assembly assembly) => Menu.Menus.OfType<AssemblyMenu>().FirstOrDefault(x => x.Assembly == assembly);
    }
}