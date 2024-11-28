#if EXILED
using Exiled.Events.EventArgs.Player;
#elif NWAPI
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using PluginAPI.Core;
using ServerSpecificSyncer.Features;
using UserSettings.ServerSpecific;

namespace ServerSpecificSyncer;

internal class EventHandler
{
#if EXILED
    internal static void Verified(VerifiedEventArgs ev) => Menu.LoadForPlayer(ev.Player.ReferenceHub, null);
    internal static void Left(LeftEventArgs ev) => Menu.DeletePlayer(ev.Player.ReferenceHub);

#elif NWAPI
    [PluginEvent(ServerEventType.PlayerJoined)]
    public static void Verified(Player player) => Menu.LoadForPlayer(player.ReferenceHub, null);
    
    [PluginEvent(ServerEventType.PlayerLeft)]
    public static void Left(Player player) => Menu.DeletePlayer(player.ReferenceHub);
#endif
    
    public static void OnReceivingInput(ReferenceHub hub, ServerSpecificSettingBase ss)
    {
        try
        {
            Menu menu = Menu.TryGetCurrentPlayerMenu(hub);
            // global/local keybinds
            if (ss.SettingId > 100000 && ss is SSKeybindSetting setting)
            {
                Keybind loadedKeybind = Menu.TryGetKeybinding(hub, ss, menu);
                if (loadedKeybind != null)
                {
                    if (setting.SyncIsPressed)
                        loadedKeybind.OnUsed?.Invoke(hub);
                    return;
                }
            }
            // load main menu
            if (ss.SettingId == 0 && menu != null)
            {
                // return to upper menu (or main menu)
                Menu m = Menu.GetMenu(menu.MenuRelated);
                Menu.LoadForPlayer(hub, m);
            }
            // load method when input is used on specific menu.
            else if (menu != null)
            {
                if (ss.SettingId < 0)
                    Menu.LoadForPlayer(hub, menu.TryGetSubMenu(ss.SettingId));
                else
                    menu.OnInput(hub, ss);
            }
            // load selected menu.
            else
            {
                if (!Menu.Menus.Any(x => x.Id == ss.SettingId))
                    throw new KeyNotFoundException("invalid loaded id. please report this bug to developers.");
                Menu m = Menu.Menus.FirstOrDefault(x => x.Id == ss.SettingId);
                Menu.LoadForPlayer(hub, m);
            }
        }
        catch (Exception e)
        {
            if (Plugin.GetTranslation().ShowErrorToClient)
            {
                ServerSpecificSettingsSync.SendToPlayer(hub, new ServerSpecificSettingBase[]
                {
                    new SSTextArea(-5, $"<color=red><b>{Plugin.GetTranslation().ServerError}\n{((hub.serverRoles.RemoteAdmin || Plugin.GetTranslation().ShowFullErrorToClient) && Plugin.GetTranslation().ShowFullErrorToModerators ? e.ToString() : Plugin.GetTranslation().NoPermission)}</b></color>", SSTextArea.FoldoutMode.CollapsedByDefault, Plugin.GetTranslation().ServerError)
                });
            }
            Log.Error(e.ToString());
        }
    }
}