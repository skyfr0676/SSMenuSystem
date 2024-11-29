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
using ServerSpecificSyncer.Features.Wrappers;
using UserSettings.ServerSpecific;

namespace ServerSpecificSyncer
{
    internal class EventHandler
    {
#if EXILED
        internal static void Verified(VerifiedEventArgs ev) => Menu.LoadForPlayer(ev.Player.ReferenceHub, null);
        internal static void Left(LeftEventArgs ev) => Menu.DeletePlayer(ev.Player.ReferenceHub);

#elif NWAPI
    [PluginEvent(ServerEventType.PlayerJoined)]
    public void Verified(Player player) => Menu.LoadForPlayer(player.ReferenceHub, null);

    [PluginEvent(ServerEventType.PlayerLeft)]
    public void Left(Player player) => Menu.DeletePlayer(player.ReferenceHub);
#endif
    
        public static void OnReceivingInput(ReferenceHub hub, ServerSpecificSettingBase ss)
        {
            try
            {
                if (ss.SettingId == -999)
                {
                    Menu.LoadForPlayer(hub, null);
                    return;
                }
                Menu menu = Menu.TryGetCurrentPlayerMenu(hub);
                // global/local keybinds
                if (ss.SettingId > 100000 && ss is SSKeybindSetting setting)
                {
                    Keybind loadedKeybind = Menu.TryGetKeybinding(hub, ss, menu);
                    if (loadedKeybind != null)
                    {
                        if (setting.SyncIsPressed)
                            loadedKeybind.Action?.Invoke(hub);
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
                    {
                        ServerSpecificSettingBase s = menu.Settings.FirstOrDefault(s => s.SettingId == ss.SettingId);
                        if (s is Button wBtn)
                            wBtn.Action?.Invoke(hub, wBtn);
                        else if (s is Dropdown wDropdown)
                            wDropdown.Action?.Invoke(hub, wDropdown, ((SSDropdownSetting)ss).SyncSelectionText);
                        else if (s is Plaintext wPlaintext)
                            wPlaintext.OnChanged?.Invoke(hub, ((SSPlaintextSetting)ss).SyncInputText, wPlaintext);
                        else if (s is Slider wSlider)
                            wSlider.Action?.Invoke(hub, ((SSSliderSetting)ss).SyncFloatValue, wSlider);
                        else if (s is YesNoButton wYesNo)
                            wYesNo.Action?.Invoke(hub, ((SSTwoButtonsSetting)ss).SyncIsB, wYesNo);
                        else
                            menu.OnInput(hub, ss);
                    }
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
                if (Plugin.StaticConfig.ShowErrorToClient)
                {
                    ServerSpecificSettingsSync.SendToPlayer(hub, new ServerSpecificSettingBase[]
                    {
                        new SSTextArea(-5, $"<color=red><b>{Plugin.GetTranslation().ServerError}\n{((hub.serverRoles.RemoteAdmin || Plugin.StaticConfig.ShowFullErrorToClient) && Plugin.StaticConfig.ShowFullErrorToModerators ? e.ToString() : Plugin.GetTranslation().NoPermission)}</b></color>", SSTextArea.FoldoutMode.CollapsedByDefault, Plugin.GetTranslation().ServerError),
                        new SSButton(-999, Plugin.GetTranslation().ReloadButton.Label, Plugin.GetTranslation().ReloadButton.ButtonText)
                    });
                }
                Log.Error($"there is a error while receiving input {ss.SettingId} ({ss.Label}): {e.Message}\nActivate Debugger to show full details.");
#if DEBUG
                Log.Error(e.ToString());
#else
                Log.Debug(e.ToString());
#endif
            }
        }
    }
}