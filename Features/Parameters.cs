using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MEC;
using PluginAPI.Core;
using ServerSpecificSyncer.Features.Interfaces;
using UnityEngine;
using UserSettings.ServerSpecific;

namespace ServerSpecificSyncer.Features
{
    //TODO: MAKE THE PARAMETERS
    public static class Parameters
    {
        public static ServerSpecificSettingBase GetParameter(this ReferenceHub hub,
            ServerSpecificSettingBase settingBase) => Menu.TryGetCurrentPlayerMenu(hub)?.GetParameter(hub, settingBase);
        public static ServerSpecificSettingBase GetParameter(this Menu menu, ReferenceHub hub, ServerSpecificSettingBase setting)
        {
            if (!menu.InternalSettingsSync.TryGetValue(hub, out var value))
                return null;

            return value.FirstOrDefault(x => x.SettingId == setting.SettingId);
        }
        
        public static ServerSpecificSettingBase GetParameter(this ReferenceHub hub, string label)
        {
            foreach (var menu in Menu.Menus)
            {
                if (menu.SettingsSync.TryGetValue(hub, out var settings))
                {
                    foreach (var setting in settings)
                    {
                        if (label == setting.Label)
                            return setting;
                    }
                }
            }

            return null;
        }


        public static IEnumerator<float> SyncAll(ReferenceHub hub)
        {
            SyncCache.Add(hub, new());
            var sendSettings = new List<ServerSpecificSettingBase>();
            float timeout = 0;
            foreach (var menu in Menu.Menus.ToList())
            {
                Log.Debug($"syncing {hub.nicknameSync.MyNick} registered parameters for menu {menu.Name}");
                foreach (var t in menu.Settings)
                {
                    if (t.ResponseMode != ServerSpecificSettingBase.UserResponseMode.AcquisitionAndChange)
                        continue;
                    if (t is ISetting setting)
                        sendSettings.Add(setting.Base);
                    else
                        sendSettings.Add(t);
                }
                
                ServerSpecificSettingsSync.SendToPlayer(hub, sendSettings.ToArray());
                while (SyncCache[hub].Count < sendSettings.Count && timeout < 10)
                {
                    timeout += Time.deltaTime;
                    yield return Timing.WaitForOneFrame;
                }

                if (SyncCache[hub].Count < sendSettings.Count || timeout >= 10)
                {
                    Log.Error($"timeout exceeded to sync value for hub {hub.nicknameSync.MyNick} menu {menu.Name}. Stopping the process.");
                    break;
                }
                menu.InternalSettingsSync[hub] = new(SyncCache[hub]);
                SyncCache[hub].Clear();
                sendSettings.Clear();
                Log.Debug($"synced settings for {hub.nicknameSync.MyNick} to the menu {menu.Name}. {menu.InternalSettingsSync[hub].Count} settings have been synced.");
            }
            SyncCache.Remove(hub);
            Menu.LoadForPlayer(hub, null);
        }
        
        internal static readonly Dictionary<ReferenceHub, List<ServerSpecificSettingBase>> SyncCache = new();

        public static List<ServerSpecificSettingBase> GetAllSyncedParameters(ReferenceHub referenceHub)
        {
            List<ServerSpecificSettingBase> toReturn = new();
            foreach (var menu in Menu.Menus.Where(x => x.InternalSettingsSync.ContainsKey(referenceHub)))
                toReturn.AddRange(menu.InternalSettingsSync[referenceHub]);
            return toReturn;
        }
    }
}