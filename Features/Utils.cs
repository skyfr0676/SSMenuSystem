using System.Linq;
using System.Reflection;
using Mirror;
using UserSettings.ServerSpecific;

namespace ServerSpecificSyncer.Features
{
    public class Utils
    {
        public static void SendToPlayer(ReferenceHub hub, Menu relatedMenu, ServerSpecificSettingBase[] collection, int? versionOverride = null)
        {
            if (relatedMenu != null)
            {
                foreach (var c in collection)
                {
                    if (c.SettingId < relatedMenu.Hash)
                        c.SetId(c.SettingId + relatedMenu.Hash, c.Label);
                }
            }
            hub.connectionToClient.Send(new SSSEntriesPack(collection, versionOverride ?? ServerSpecificSettingsSync.Version));
        }
    
        public static AssemblyMenu GetMenu(Assembly assembly) => Menu.Menus.OfType<AssemblyMenu>().FirstOrDefault(x => x.Assembly == assembly);
    }
}