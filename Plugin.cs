using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using HarmonyLib;
using ServerSpecificSyncer.Features;
using ServerSpecificSyncer.Test;
using ServerSpecificSyncer.Test.Subs;
using UserSettings.ServerSpecific;
using Log = PluginAPI.Core.Log;

namespace ServerSpecificSyncer
{
    public class Plugin : Plugin<Config>
    {
        public override string Author => "Sky";
        public override string Name => "ServerSpecificSyncer";
        public override Version Version => new Version(1, 0, 0);
        public override string Prefix => "ss_syncer";
        private Harmony _harmony;
        public override void OnEnabled()
        {
            Exiled.Events.Handlers.Player.Verified += Verified;
            Exiled.Events.Handlers.Player.Left += Left;
            ServerSpecificSettingsSync.ServerOnSettingValueReceived += OnReceivingInput;
            _harmony = new Harmony("fr.nova.patches");
            _harmony.PatchAll();
            Menu.Register(new PrimitiveTest());
            Menu.Register(new SubMenuTest());
            Menu.Register(new Bugga());
            base.OnEnabled();
        }

        private void OnReceivingInput(ReferenceHub hub, ServerSpecificSettingBase ss)
        {
            try
            {
                Menu menu = Menu.TryGetCurrentPlayerMenu(hub);
                if (ss.SettingId > 100000 && ss is SSKeybindSetting setting)
                {
                    Keybind loadedKeybind = Menu.TryGetKeybind(hub, ss, menu);
                    if (loadedKeybind != null)
                    {
                        if (setting.SyncIsPressed)
                            loadedKeybind.OnUsed?.Invoke(hub);
                        return;
                    }
                }
                if (ss.SettingId == 0 && menu != null)
                    Menu.LoadForPlayer(hub, null);
                else if (menu != null)
                {
                    if (ss.SettingId < 0)
                        Menu.LoadForPlayer(hub, menu.TryGetSubMenu(ss.SettingId));
                    else
                        menu.OnInput(hub, ss);
                }
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
                ServerSpecificSettingsSync.SendToPlayer(hub, new ServerSpecificSettingBase[]
                {
                    new SSTextArea(-5, $"INTERNAL SERVER ERROR: {e.Message}\n{(hub.serverRoles.RemoteAdmin ? e.ToString() : "insufficient permissions to see the full errors")}", SSTextArea.FoldoutMode.CollapsedByDefault, "INTERNAL SERVER ERROR - EXTEND FOR MORE ERRORS")
                });
                Log.Error(e.ToString());
            }
        }

        private void Verified(VerifiedEventArgs ev) => Menu.LoadForPlayer(ev.Player.ReferenceHub, null);
        private void Left(LeftEventArgs ev) => Menu.DeletePlayer(ev.Player.ReferenceHub);
    }
}