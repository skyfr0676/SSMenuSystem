using System.Collections.Generic;
using System.Linq;
using MEC;
using PluginAPI.Core;
using SSMenuSystem.Features.Interfaces;
using UserSettings.ServerSpecific;

namespace SSMenuSystem.Features
{
    /// <summary>
    /// Parameters class for getting parameters from <see cref="Menu"/>.
    /// </summary>
    public static class Parameters
    {
        /// <summary>
        /// Get synced parameter value for <see cref="ReferenceHub"/>.
        /// </summary>
        /// <param name="hub">The target hub.</param>
        /// <param name="settingId">The id of setting.</param>
        /// <typeparam name="TMenu">The Menu to get parameter.</typeparam>
        /// <typeparam name="TSs">The setting type.</typeparam>
        /// <returns>An instance of <see cref="ServerSpecificSettingBase"/> That contains synecd value, or null if not found.</returns>
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


        /// <summary>
        /// Sync all paramters for all menus for <see cref="ReferenceHub"/>.
        /// </summary>
        /// <param name="hub">The target hub</param>
        /// <returns>a enumerator for <see cref="Timing.RunCoroutine(System.Collections.Generic.IEnumerator{float})"/></returns>.
        internal static IEnumerator<float> SyncAll(ReferenceHub hub)
        {
            SyncCache.Add(hub, new List<ServerSpecificSettingBase>());
            List<ServerSpecificSettingBase> sendSettings = new();
            float timeout = 0;
            List<Menu> menus = Menu.Menus.ToList();

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

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse // TODO: remove false
            if ((Plugin.StaticConfig.ForceMainMenuEventIfOnlyOne && false) || Menu.Menus.Count(x => x.CheckAccess(hub)) > 1)
                Menu.LoadForPlayer(hub, null);
            else
                Menu.LoadForPlayer(hub, Menu.Menus.First());
        }

        /// <summary>
        /// A cache, updated on <see cref="EventHandler.OnReceivingInput"/> if hub is inside, when syncing parameters.
        /// </summary>
        internal static readonly Dictionary<ReferenceHub, List<ServerSpecificSettingBase>> SyncCache = new();

        /// <summary>
        /// Get ALL synced parameters from ALL menus for <see cref="ReferenceHub"/>
        /// </summary>
        /// <param name="referenceHub">The target hub.</param>
        /// <returns>all synced parameters from ALL menus for <see cref="ReferenceHub"/>.</returns>
        public static List<ServerSpecificSettingBase> GetAllSyncedParameters(ReferenceHub referenceHub)
        {
            List<ServerSpecificSettingBase> toReturn = new();
            foreach (Menu menu in Menu.Menus.Where(x => x.InternalSettingsSync.ContainsKey(referenceHub)))
                toReturn.AddRange(menu.InternalSettingsSync[referenceHub]);
            return toReturn;
        }

        /// <summary>
        /// Get All synced parameter for target menu.
        /// </summary>
        /// <param name="referenceHub">The target hub.</param>
        /// <typeparam name="T">The target menu.</typeparam>
        /// <returns>A list that contains all synced parameters.</returns>
        public static List<ServerSpecificSettingBase> GetMenuSpecificSyncedParameters<T>(ReferenceHub referenceHub) where T : Menu
        {
            List<ServerSpecificSettingBase> toReturn = new();
            foreach (Menu menu in Menu.Menus.Where(x => x.InternalSettingsSync.ContainsKey(referenceHub) && x is T))
                toReturn.AddRange(menu.InternalSettingsSync[referenceHub]);
            return toReturn;
        }

        /// <summary>
        /// Sync all parameters from <see cref="Menu"/> for <see cref="ReferenceHub"/>.
        /// </summary>
        /// <param name="hub">The target hub.</param>
        /// <param name="menu">The target menu.</param>
        /// <param name="toSendWhenEnded">All parameters to send when ended.</param>
        /// <returns>an enumerator for <see cref="Timing.RunCoroutine(System.Collections.Generic.IEnumerator{float})"/></returns>
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
}
