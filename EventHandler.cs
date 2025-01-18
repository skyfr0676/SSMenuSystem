
using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.Events.EventArgs.Player;
using MEC;
using PluginAPI.Core;
using SSMenuSystem.Features;
using SSMenuSystem.Features.Wrappers;
using UserSettings.ServerSpecific;
#if EXILED
#elif NWAPI
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
#endif

namespace SSMenuSystem
{
    internal class EventHandler
    {
#if EXILED
        internal static void Verified(VerifiedEventArgs ev) => Timing.RunCoroutine(Parameters.SyncAll(ev.Player.ReferenceHub));
        internal static void Left(LeftEventArgs ev) => Menu.DeletePlayer(ev.Player.ReferenceHub);
        internal static void ChangingGroup(ChangingGroupEventArgs ev) => SyncChangedGroup(ev.Player.ReferenceHub);
        internal static void ReloadedConfigs()
        {
            Log.Info("reloaded configs.");
            foreach (ReferenceHub hub in ReferenceHub.AllHubs)
                Menu.LoadForPlayer(hub, Menu.GetCurrentPlayerMenu(hub));
        }

#elif NWAPI
        [PluginEvent(ServerEventType.PlayerJoined)]
        public void Verified(Player player) => Timing.RunCoroutine(Parameters.SyncAll(player.ReferenceHub));

        [PluginEvent(ServerEventType.PlayerLeft)]
        public void Left(Player player) => Menu.DeletePlayer(player.ReferenceHub);
#endif


        // ReSharper disable once MemberCanBePrivate.Global
        internal static void SyncChangedGroup(ReferenceHub hub)
        {
            Timing.CallDelayed(0.1f, () =>
            {
                if (Parameters.SyncCache.ContainsKey(hub))
                    return;

                Menu menu = Menu.GetCurrentPlayerMenu(hub);
                menu?.Reload(hub);
                if (menu == null)
                    Menu.LoadForPlayer(hub, null);
            });
        }

        public static void OnReceivingInput(ReferenceHub hub, ServerSpecificSettingBase ss)
        {
            try
            {
                if (Parameters.SyncCache.TryGetValue(hub, out List<ServerSpecificSettingBase> value))
                {
                    value.Add(ss);
                    Log.Debug("received value that been flagged as \"SyncCached\". Redirected values to Cache.", Plugin.StaticConfig.Debug);
                    return;
                }

                if (ss.OriginalDefinition != null)
                {
                    ss.Label = ss.OriginalDefinition.Label;
                    ss.HintDescription = ss.OriginalDefinition.HintDescription;
                    ss.SettingId = ss.OriginalDefinition.SettingId;
                }
                else // is a pin or header
                    ss.SettingId -= Menu.GetCurrentPlayerMenu(hub)?.Hash ?? 0;

                // return to menu
                if (ss.SettingId == -999)
                {
                    Menu.LoadForPlayer(hub, null);
                    return;
                }

                // check permissions
                Menu menu = Menu.GetCurrentPlayerMenu(hub);
                if (!menu?.CheckAccess(hub) ?? false)
                {
                    Log.Warning($"{hub.nicknameSync.MyNick} tried to interact with menu {menu.Name} which is disabled for him.");
                    Menu.LoadForPlayer(hub, null);
                    return;
                }

                // global/local keybinds
                if (ss.SettingId > Keybind.Increment && ss is SSKeybindSetting setting)
                {
                    Keybind loadedKeybind = Menu.TryGetKeybinding(hub, ss, menu);
                    if (loadedKeybind != null)
                    {
                        loadedKeybind.Action?.Invoke(hub, setting.SyncIsPressed);
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
                        if (menu.SettingsSync[hub].Any(x => x.SettingId == ss.SettingId))
                            menu.SettingsSync[hub][menu.SettingsSync[hub].FindIndex(x => x.SettingId == ss.SettingId)] = ss;
                        else
                            menu.SettingsSync[hub].Add(ss);
                        switch (s)
                        {
                            case Button wBtn:
                                wBtn.Action?.Invoke(hub, (SSButton)ss);
                                break;
                            case Dropdown wDropdown:
                                wDropdown.Action?.Invoke(hub, wDropdown.Options[((SSDropdownSetting)ss).SyncSelectionIndexRaw], ((SSDropdownSetting)ss).SyncSelectionIndexRaw, (SSDropdownSetting)ss);
                                break;
                            case Plaintext wPlaintext:
                                wPlaintext.OnChanged?.Invoke(hub, ((SSPlaintextSetting)ss).SyncInputText, (SSPlaintextSetting)ss);
                                break;
                            case Slider wSlider:
                                wSlider.Action?.Invoke(hub, ((SSSliderSetting)ss).SyncFloatValue, (SSSliderSetting)ss);
                                break;
                            case YesNoButton wYesNo:
                                wYesNo.Action?.Invoke(hub, ((SSTwoButtonsSetting)ss).SyncIsB, (SSTwoButtonsSetting)ss);
                                break;
                        }

                        if (ss.SettingId > menu.Hash)
                            ss.SettingId -= menu.Hash;
                        menu.OnInput(hub, ss);
                    }
                }
                // load selected menu.
                else
                {
                    if (!Menu.Menus.Any(x => x.Id == ss.SettingId))
                        throw new KeyNotFoundException($"invalid loaded id ({ss.SettingId}). please report this bug to developers.");
                    Menu m = Menu.Menus.FirstOrDefault(x => x.Id == ss.SettingId);
                    Menu.LoadForPlayer(hub, m);
                }
            }
            catch (Exception e)
            {
                Log.Error($"there is a error while receiving input {ss.SettingId} ({ss.Label}): {e.Message}\nActivate Debugger to show full details.");
#if DEBUG
                Log.Error(e.ToString());
#else
                Log.Debug(e.ToString(), Plugin.StaticConfig.Debug);
#endif
                if (Plugin.StaticConfig.ShowErrorToClient)
                {
                    Features.Utils.SendToPlayer(hub, null, new ServerSpecificSettingBase[]
                    {
                        new SSTextArea(-5, $"<color=red><b>{Plugin.GetTranslation().ServerError}\n{((hub.serverRoles.RemoteAdmin || Plugin.StaticConfig.ShowFullErrorToClient) && Plugin.StaticConfig.ShowFullErrorToModerators ? e.ToString() : Plugin.GetTranslation().NoPermission)}</b></color>", SSTextArea.FoldoutMode.CollapsedByDefault, Plugin.GetTranslation().ServerError),
                        new SSButton(-999, Plugin.GetTranslation().ReloadButton.Label, Plugin.GetTranslation().ReloadButton.ButtonText)
                    });
                }
            }
        }
    }
}