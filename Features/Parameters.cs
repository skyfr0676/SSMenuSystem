#if DEBUG
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MEC;
using PluginAPI.Core;
using ServerSpecificSyncer.Features.Interfaces;
using ServerSpecificSyncer.Features.Wrappers;
using UnityEngine;
using UserSettings.ServerSpecific;

namespace ServerSpecificSyncer.Features
{
    public static class Parameters
    {
        internal static ReferenceHub playerCache;

        public static TSs GetParameter<TMenu, TSs>(this ReferenceHub hub, int settingId)
            where TMenu : Menu
            where TSs : ServerSpecificSettingBase
        {
            if (typeof(TSs).BaseType == typeof(ISetting))
            {
                Log.Error(nameof(TSs) + " need to be of base type (example: Plaintext became SSPlaintextSetting).");
                return default;
            }

            foreach (Menu menu in Menu.Menus.Where(x => x is TMenu))
            {
                if (!menu.SettingsSync.TryGetValue(hub, out List<ServerSpecificSettingBase> settings))
                    continue;
                
                ServerSpecificSettingBase t = settings.Where(x => x is TSs).FirstOrDefault(x => x.SettingId == settingId);
                return t as TSs;
            }
            
            return default;
        }


        public static IEnumerator<float> SyncAll(ReferenceHub hub)
        {
            //ReferenceHub hub = playerCache;
            playerCache = null;
            SyncCache.Add(hub, new List<ServerSpecificSettingBase>());
            List<ServerSpecificSettingBase> sendSettings = new();
            float timeout = 0;
            List<Menu> menus = Menu.Menus.ToList();
            /*foreach (var menu in menus.ToArray())
                menus.AddRange(Menu.Menus.Where(x => x.MenuRelated == menu.GetType() && x != menu));*/
            
            foreach (Menu menu in menus)
            {
                if (!menu.CheckAccess(hub))
                {
                    Log.Debug(hub.nicknameSync.MyNick + " don't have access to " + menu.Name + ". Skipping.", Plugin.StaticConfig.Debug);
                    continue;
                }

                Log.Debug($"syncing {hub.nicknameSync.MyNick} registered parameters for menu {menu.Name} {(menu.MenuRelated != null ? $"SubMenu of {Menu.GetMenu(menu.MenuRelated).Name} ({menu.MenuRelated.Name})" : string.Empty)}.", Plugin.StaticConfig.Debug);
                foreach (ServerSpecificSettingBase t in menu.Settings)
                {
                    if (t.ResponseMode != ServerSpecificSettingBase.UserResponseMode.AcquisitionAndChange)
                        continue;
                    ServerSpecificSettingBase @base;
                    if (t is ISetting setting)
                        @base = setting.Base;
                    else
                        @base = t;

                    sendSettings.Add(@base);
                }

                Utils.SendToPlayer(hub, menu, sendSettings.ToArray());
                const int waitTimeMs = 10;
                while (SyncCache[hub].Count < sendSettings.Count && timeout < 10)
                {
                    timeout += waitTimeMs / 1000f;
                    yield return Timing.WaitForSeconds(10/1000f);
                    //await Task.Delay(waitTimeMs);
                }

                if (SyncCache[hub].Count < sendSettings.Count || timeout >= 10)
                {
                    Log.Error(
                        $"timeout exceeded to sync value for hub {hub.nicknameSync.MyNick} menu {menu.Name}. Stopping the process.");
                    break;
                }

                foreach (ServerSpecificSettingBase setting in SyncCache[hub])
                {
                    if (sendSettings.Any(s => s.SettingId == setting.SettingId))
                    {
                        ServerSpecificSettingBase set = sendSettings.First(s => s.SettingId == setting.SettingId);
                        setting.Label = set.Label;
                        setting.SettingId -= menu.Hash;
                        setting.HintDescription = set.HintDescription;
                    }
                }

                menu.InternalSettingsSync[hub] = new List<ServerSpecificSettingBase>(SyncCache[hub]);
                sendSettings.Clear();
                SyncCache[hub].Clear();
                Log.Debug(
                    $"synced settings for {hub.nicknameSync.MyNick} to the menu {menu.Name}. {menu.InternalSettingsSync[hub].Count} settings have been synced.", Plugin.StaticConfig.Debug);
            }
            SyncCache.Remove(hub);
            
            Log.Debug("Hub Synced parameters. Stat of his cache: " +
                      (SyncCache.ContainsKey(hub) ? "active" : "disabled"), Plugin.StaticConfig.Debug);

            if (Menu.Menus.Where(x => x.CheckAccess(hub)).IsEmpty())
            {
                Log.Warning("no valid menu found for " + hub.nicknameSync.MyNick + ".");
                yield break;
            }

#if DEBUG
            if (Plugin.StaticConfig.ForceMainMenuEventIfOnlyOne || Menu.Menus.Count(x => x.CheckAccess(hub)) > 1)
                Menu.LoadForPlayer(hub, null);
            else
                Menu.LoadForPlayer(hub, Menu.Menus.First());
#else
            Menu.LoadForPlayer(hub, null);
#endif
        }
        
        internal static readonly Dictionary<ReferenceHub, List<ServerSpecificSettingBase>> SyncCache = new();

        public static List<ServerSpecificSettingBase> GetAllSyncedParameters(ReferenceHub referenceHub)
        {
            List<ServerSpecificSettingBase> toReturn = new();
            foreach (Menu menu in Menu.Menus.Where(x => x.InternalSettingsSync.ContainsKey(referenceHub)))
                toReturn.AddRange(menu.InternalSettingsSync[referenceHub]);
            return toReturn;
        }
        
        public static List<ServerSpecificSettingBase> GetMenuSpecificSyncedParameters<T>(ReferenceHub referenceHub) where T : Menu
        {
            List<ServerSpecificSettingBase> toReturn = new();
            foreach (Menu menu in Menu.Menus.Where(x => x.InternalSettingsSync.ContainsKey(referenceHub) && x is T))
                toReturn.AddRange(menu.InternalSettingsSync[referenceHub]);
            return toReturn;
        }
        
#if DEBUG
        internal static IEnumerator<float> Sync(ReferenceHub hub, Menu menu, ServerSpecificSettingBase[] toSendWhenEnded)
        {
            SyncCache.Add(hub, new List<ServerSpecificSettingBase>());
            List<ServerSpecificSettingBase> sendSettings = new();
            float timeout = 0;
            
            if (!menu.CheckAccess(hub))
            {
                Log.Debug(hub.nicknameSync.MyNick + " don't have access to " + menu.Name + ". Skipping.", Plugin.StaticConfig.Debug);
                yield break;
            }

            Log.Debug($"syncing {hub.nicknameSync.MyNick} registered parameters for menu {menu.Name}", Plugin.StaticConfig.Debug);
            foreach (ServerSpecificSettingBase t in menu.Settings)
            {
                if (t.ResponseMode != ServerSpecificSettingBase.UserResponseMode.AcquisitionAndChange)
                    continue;
                ServerSpecificSettingBase @base;
                if (t is ISetting setting)
                    @base = setting.Base;
                else
                    @base = t;

                sendSettings.Add(@base);
            }

            Utils.SendToPlayer(hub, menu, sendSettings.ToArray());
            const int waitTimeMs = 10;
            while (SyncCache[hub].Count < sendSettings.Count && timeout < 10)
            {
                timeout += waitTimeMs / 1000f;
                yield return Timing.WaitForSeconds(10/1000f);
            }

            if (SyncCache[hub].Count < sendSettings.Count || timeout >= 10)
            {
                Log.Error(
                    $"timeout exceeded to sync value for hub {hub.nicknameSync.MyNick} menu {menu.Name}. Stopping the process.");
                yield break;
            }

            foreach (ServerSpecificSettingBase setting in SyncCache[hub])
            {
                if (sendSettings.Any(s => s.SettingId == setting.SettingId))
                {
                    ServerSpecificSettingBase set = sendSettings.First(s => s.SettingId == setting.SettingId);
                    setting.Label = set.Label;
                    setting.SettingId -= menu.Hash;
                    setting.HintDescription = set.HintDescription;
                }
            }

            menu.InternalSettingsSync[hub] = new List<ServerSpecificSettingBase>(SyncCache[hub]);
            sendSettings.Clear();
            SyncCache.Remove(hub);
            Log.Debug(
                $"synced settings for {hub.nicknameSync.MyNick} to the menu {menu.Name}. {menu.InternalSettingsSync[hub].Count} settings have been synced.", Plugin.StaticConfig.Debug);
            Utils.SendToPlayer(hub, menu, toSendWhenEnded);
        }
    }
#endif
}
#endif
