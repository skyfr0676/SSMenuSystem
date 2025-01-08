using System.Linq;
using System.Reflection;
using UserSettings.ServerSpecific;

namespace ServerSpecificSyncer.Features
{
    public class Utils
    {
        public static void SendToPlayer(ReferenceHub hub, ServerSpecificSettingBase[] collection, int? versionOverride = null)
        {
            hub.connectionToClient.Send(new SSSEntriesPack(collection, versionOverride ?? ServerSpecificSettingsSync.Version));
        }
    
        public static AssemblyMenu GetMenu(Assembly assembly) => Menu.Menus.OfType<AssemblyMenu>().FirstOrDefault(x => x.Assembly == assembly);
    }
}