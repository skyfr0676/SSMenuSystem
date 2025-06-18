using System;
using System.Collections.Generic;
using System.Linq;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.CustomHandlers;
using SSMenuSystem.Features;
using SSMenuSystem.Features.Wrappers;
using UserSettings.ServerSpecific;
using MEC;
using Exiled.Events.EventArgs.Player;
using Log = SSMenuSystem.Features.Log;

namespace SSMenuSystem
{
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class EventHandler : CustomEventsHandler
    {
        public override void OnPlayerJoined(PlayerJoinedEventArgs ev) => Timing.RunCoroutine(Parameters.SyncAll(ev.Player.ReferenceHub));
        public override void OnPlayerLeft(PlayerLeftEventArgs ev) => Menu.DeletePlayer(ev.Player.ReferenceHub);
        public override void OnPlayerGroupChanged(PlayerGroupChangedEventArgs ev) =>
            SyncChangedGroup(ev.Player.ReferenceHub);

        private static void SyncChangedGroup(ReferenceHub hub)
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
                    Log.Debug("received value that been flagged as \"SyncCached\". Redirected values to Cache.");
                    return;
                }

                // return to menu
                if (ss.SettingId == -999)
                {
                    Menu.LoadForPlayer(hub, null);
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

                // check permissions
                Menu menu = Menu.GetCurrentPlayerMenu(hub);
                if (!menu?.CheckAccess(hub) ?? false)
                {
                    Log.Warn($"{hub.nicknameSync.MyNick} tried to interact with menu {menu.Name} which is disabled for him.");
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
                    //if (ss.SettingId < 0)
                    if (menu.TryGetSubMenu(ss.SettingId, out Menu subMenu))
                        Menu.LoadForPlayer(hub, subMenu);
                    else
                    {
                        if (menu.InternalSettingsSync[hub].Any(x => x.SettingId == ss.SettingId))
                            menu.InternalSettingsSync[hub][menu.InternalSettingsSync[hub].FindIndex(x => x.SettingId == ss.SettingId)] = ss;
                        else
                            menu.InternalSettingsSync[hub].Add(ss);

                        ServerSpecificSettingBase s = menu.SentSettings.TryGetValue(hub, out ServerSpecificSettingBase[] customSettings) ? customSettings.FirstOrDefault(b => b.SettingId == ss.SettingId) : null;
                        s ??= customSettings?.FirstOrDefault(b => b.SettingId == ss.SettingId - menu.Hash);

                        if (s == null)
                            throw new Exception("Failed to find the sent setting.");

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
                                wYesNo.Action?.Invoke(hub, ((SSTwoButtonsSetting)ss).SyncIsA, (SSTwoButtonsSetting)ss);
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
                Log.Debug(e.ToString());
#endif
                if (Plugin.Instance.Config.ShowErrorToClient)
                {
                    Features.Utils.SendToPlayer(hub, null, new ServerSpecificSettingBase[]
                    {
                        new SSTextArea(-5, $"<color=red><b>{Plugin.Instance.Translation.ServerError}\n{((hub.serverRoles.RemoteAdmin || Plugin.Instance.Config.ShowFullErrorToClient) && Plugin.Instance.Config.ShowFullErrorToModerators ? e.ToString() : Plugin.Instance.Translation.NoPermission)}</b></color>", SSTextArea.FoldoutMode.CollapsedByDefault, Plugin.Instance.Translation.ServerError),
                        new SSButton(-999, Plugin.Instance.Translation.ReloadButton.Label, Plugin.Instance.Translation.ReloadButton.ButtonText)
                    });
                }
            }
        }
    }
}