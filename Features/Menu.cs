using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Mirror;
using PluginAPI.Core;
using ServerSpecificSyncer.Features.Interfaces;
using ServerSpecificSyncer.Features.Wrappers;
using UnityEngine;
using UserSettings.ServerSpecific;

namespace ServerSpecificSyncer.Features
{
    /// <summary>
    /// The Menu system.
    /// </summary>
    public abstract class Menu
    {
        private static readonly Dictionary<ReferenceHub, Menu> MenuSync = new();
        private static readonly List<Menu> LoadedMenus = new();
        private static readonly Dictionary<Assembly, ServerSpecificSettingBase[]> Pinned = new();

        /// <summary>
        /// All menus loaded.
        /// </summary>
        public static List<Menu> Menus => LoadedMenus;
        private static readonly Dictionary<Menu, List<Keybind>> GlobalKeybindingSync = new();
        
        internal Dictionary<ReferenceHub, List<ServerSpecificSettingBase>> InternalSettingsSync = new();
        
        public ReadOnlyDictionary<ReferenceHub, List<ServerSpecificSettingBase>> SettingsSync => new(InternalSettingsSync);
        
        /// <summary>
        /// This is used to see if <see cref="hub"/> can use <see cref="Menu"/> or not.
        /// </summary>
        /// <param name="hub">The target <see cref="ReferenceHub"/></param>
        /// <returns>bool</returns>
        public virtual bool CheckAccess(ReferenceHub hub) => true;

        /// <summary>
        /// Register all menus in the <see cref="Assembly.GetCallingAssembly"/>.
        /// </summary>
        public static void RegisterAll() => Register(Assembly.GetCallingAssembly());
        
        /// <summary>
        /// Register all menus of indicated assembly.
        /// </summary>
        /// <param name="assembly">The target <see cref="Assembly"/>.</param>
        private static void Register(Assembly assembly)
        {
            try
            {
                Log.Debug($"loading assembly {assembly.GetName().Name}...", Plugin.StaticConfig?.Debug ?? false);
                List<Menu> loadedMenus = new();
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.IsAbstract || type.IsInterface)
                        continue;
                    if (type.BaseType != typeof(Menu))
                        continue;
                    Menu menu = Activator.CreateInstance(type) as Menu;
                    loadedMenus.Add(menu);
                }

                IOrderedEnumerable<Menu> orderedMenus = loadedMenus.OrderBy(x => x.MenuRelated == null ? 0 : 1).ThenBy(x => x.Id);
                List<Menu> registeredMenus = new();
                foreach (Menu menu in orderedMenus)
                {
                    try
                    {
                        Register(menu);
                        registeredMenus.Add(menu);
                    }
                    catch (Exception e)
                    {
                        Log.Error(
                            $"there is a error while loading menu {menu.Name}: {e.Message}\nEnable Debugger to show full details.");
#if DEBUG
                        Log.Error(e.ToString());
#else
                        Log.Debug(e.ToString(), Plugin.StaticConfig?.Debug ?? false);
#endif
                    }
                }

                Log.Info(
                    $"loaded assembly {assembly.GetName().Name} with {registeredMenus.Count} menus. A total of {LoadedMenus.Count} menus.");
            }
            catch (Exception e)
            {
                Log.Error($"failed to load assembly {assembly.GetName().Name}: {e.Message}");
#if DEBUG
                Log.Error(e.ToString());
#else
                Log.Debug(e.ToString(), Plugin.StaticConfig?.Debug ?? false);
#endif
            }
        }
        
        /// <summary>
        /// Register specific menu.
        /// </summary>
        /// <param name="menu">The target menu.</param>
        /// <exception cref="ArgumentException">One of parameters of target menu is invalid. please check the <see cref="Exception.Message"/> to find the invalid parameter.</exception>
        private static void Register(Menu menu)
        {
            if (menu == null)
                return;
            Log.Debug($"loading Server Specific menu {menu.Name}...", Plugin.StaticConfig.Debug);
            if (CheckSameId(menu))
                throw new ArgumentException($"another menu with id {menu.Id} is already registered. can't load {menu.Name}.");
            if (menu.Id >= 0)
                throw new ArgumentException($"menus ids must be < 0  (to let space for parameters and 0 is only for Main Menu).");
            if (string.IsNullOrEmpty(menu.Name))
                throw new ArgumentException($"menus name cannot be null or empty.");
            if (Menus.Any(x => x.Name == menu.Name))
                throw new ArgumentException($"two menus can't have the same name.");
                
            
            Dictionary<Type, List<int>> ids = new();
            
            foreach (ServerSpecificSettingBase action in menu.Settings)
            {
                ServerSpecificSettingBase setting = action;
                if (setting is ISetting isSetting)
                    setting = isSetting.Base;

                Type type = setting.GetType();
                
                ids.GetOrAdd(type, () => new());

                if (ids[type].Contains(setting.SettingId))
                    throw new ArgumentException($"id {setting.SettingId} for menu {menu.Name} is duplicated.");
                if (setting.SettingId < 0)
                    throw new ArgumentException($"id above and equal to 0 is reserved for menus and main menu.");
                
                ids[type].Add(setting.SettingId);

                if (action is Keybind bind && bind.IsGlobal)
                {
                    if (!GlobalKeybindingSync.ContainsKey(menu))
                        GlobalKeybindingSync[menu] = new List<Keybind>();
                    GlobalKeybindingSync[menu].Add(bind);
                }
                if (action is SSKeybindSetting && action is not Keybind)
                    Log.Warning($"setting {action.SettingId} (label {action.Label}) is registered has {nameof(SSKeybindSetting)}. it's recommended to use {typeof(Keybind).FullName} (especially if you want to create global keybindings) !");
            }

            if (menu.MenuRelated != null)
            {
                if (!LoadedMenus.Any(m => m.GetType() == menu.MenuRelated))
                    throw new ArgumentException($"menu {menu.Name} has a invalid related menu ({menu.MenuRelated.FullName} has not been loaded.");
            }
            
            LoadedMenus.Add(menu);
            Log.Debug($"Server Specific menu {menu.Name} is now registered!", Plugin.StaticConfig.Debug);
        }
        
        private static bool CheckSameId(Menu menu)
        {
            if (menu.MenuRelated == null)
                return LoadedMenus.Any(x => x.Id == menu.Id && menu.MenuRelated == null);
            return LoadedMenus.Where(x => x.MenuRelated == menu.MenuRelated).Any(x => x.Id == menu.Id);
        }

        /// <summary>
        /// Unload a menu.
        /// </summary>
        /// <param name="menu">The target menu.</param>
        public static void Unregister(Menu menu)
        {
            if (LoadedMenus.Contains(menu))
                LoadedMenus.Remove(menu);
            GlobalKeybindingSync.Remove(menu);
            foreach (KeyValuePair<ReferenceHub, Menu> sync in MenuSync)
            {
                if (sync.Value == menu)
                    LoadForPlayer(sync.Key, null);
            }
        }
        
        /// <summary>
        /// Unload all menus loaded.
        /// </summary>
        public static void UnregisterAll()
        {
            foreach (KeyValuePair<ReferenceHub, Menu> sync in MenuSync)
                LoadForPlayer(sync.Key, null);
            LoadedMenus.Clear();
            foreach (var menu in LoadedMenus.ToList())
                Unregister(menu);
        }

#nullable enable
        /// <summary>
        /// Gets or Sets if this menu is related to a <see cref="MenuRelated"/> (will be shown as a SubMenu).
        /// </summary>
        public virtual Type? MenuRelated { get; set; } = null;
#nullable disable
        
        /// <summary>
        /// Gets In-Build Settings.
        /// </summary>
        public abstract ServerSpecificSettingBase[] Settings { get; }

#if DEBUG
        //public int Hash => Mathf.Abs(Name.GetHashCode() % 100000);
        public int Hash => Name.GetStableHashCode();
#endif
        
        /// <summary>
        /// Gets or Sets the name of Menu.
        /// </summary>
        public abstract string Name { get; set; }

        /// <summary>
        /// Gets or Sets the description of Menu. Will be shown as <see cref="ServerSpecificSettingBase.HintDescription"/>.
        /// </summary>
        protected virtual string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or Sets the id of Menu. Must be above 0 and must not be equal to 0. 
        /// </summary>
        public abstract int Id { get; set; }

        /// <summary>
        /// Get the main menu for target hub.
        /// </summary>
        /// <param name="hub">The target referenceHub</param>
        /// <returns>In-build parameters that will be shown to hub.</returns>
        private static ServerSpecificSettingBase[] GetMainMenu(ReferenceHub hub)
        {
            List<ServerSpecificSettingBase> mainMenu = new();

            if (Plugin.StaticConfig.AllowPinnedContent)
                mainMenu.AddRange(Pinned.Values.SelectMany(pin => pin));

            if (LoadedMenus.Where(x => x.CheckAccess(hub)).IsEmpty())
                return mainMenu.ToArray();
            
            mainMenu.Add(new SSGroupHeader("Main Menu"));
            foreach (Menu menu in LoadedMenus.Where(x => x.CheckAccess(hub)))
            {
                if (menu.MenuRelated == null)
                    mainMenu.Add(new SSButton(menu.Id, string.Format(Plugin.GetTranslation().OpenMenu.Label, menu.Name), Plugin.GetTranslation().OpenMenu.ButtonText, null, string.IsNullOrEmpty(menu.Description) ? null : menu.Description));
            }
            
            mainMenu.AddRange(GetGlobalKeybindings(hub, null));

            return mainMenu.ToArray();
        }

        public List<ServerSpecificSettingBase> GetSettings(bool isDefault)
        {
            List<ServerSpecificSettingBase> settings = new();

            if (Plugin.StaticConfig.AllowPinnedContent)
                settings.AddRange(Pinned.Values.SelectMany(pin => pin));

            if (!isDefault)
            {
                if (MenuRelated != null)
                    settings.Add(new SSButton(0, string.Format(Plugin.GetTranslation().ReturnTo.Label, Menu.GetMenu(MenuRelated)?.Name ?? "Unkown"),
                        Plugin.GetTranslation().ReturnTo.ButtonText));
                else
                    settings.Add(new SSButton(0, Plugin.GetTranslation().ReturnToMenu.Label,
                        Plugin.GetTranslation().ReturnToMenu.ButtonText));
            }
            
            if (LoadedMenus.Count(x => x.MenuRelated == GetType() && x != this) > 0)
                settings.Add(new SSGroupHeader("Sub-Menus"));
            foreach (Menu s in LoadedMenus.Where(x => x.MenuRelated == GetType() && x != this))
                settings.Add(new SSButton(s.Id, string.Format(Plugin.GetTranslation().OpenMenu.Label, Name), Plugin.GetTranslation().OpenMenu.ButtonText, null, string.IsNullOrEmpty(Description) ? null : Description));
            settings.Add(new SSGroupHeader(Name, false, Description));

            foreach (ServerSpecificSettingBase t in Settings)
            {
                if (t is ISetting setting)
                    settings.Add(setting.Base);
                else
                    settings.Add(t);
            }
            return settings;
        }

        /// <summary>
        /// Gets global keybindings for hub.
        /// </summary>
        /// <param name="hub">The target hub.</param>
        /// <param name="menu">The loaded menu of ReferenceHub, so keys will not been duplicated.</param>
        /// <returns>In-build parameters that will be shown to hub.</returns>
        private static ServerSpecificSettingBase[] GetGlobalKeybindings(ReferenceHub hub, Menu menu)
        {
            List<ServerSpecificSettingBase> keybindings = new();
            
            if (GlobalKeybindingSync.Any(x => x.Key.CheckAccess(hub) && x.Key != menu))
            {
                keybindings.Add(new SSGroupHeader("Global Keybinding", hint:"don't take a look at this (nah seriously it's just to make some keybindings global)"));
                foreach (var menuKeybinds in GlobalKeybindingSync.Where(x => x.Key.CheckAccess(hub) && x.Key != menu))
                {
                    foreach (var keybind in menuKeybinds.Value)
                        keybindings.Add(new SSKeybindSetting(keybind.SettingId, keybind.Label, keybind.SuggestedKey, keybind.PreventInteractionOnGUI, keybind.HintDescription));
                }
            }

            return keybindings.ToArray();
        }
        
        /// <summary>
        /// Executed when action is executed on
        /// </summary>
        /// <param name="hub"></param>
        /// <param name="setting"></param>
        public virtual void OnInput(ReferenceHub hub, ServerSpecificSettingBase setting) {}

        /// <summary>
        /// Executed when <see cref="hub"/> opened the menu.
        /// </summary>
        /// <param name="hub">the target hub.</param>
        protected virtual void ProperlyEnable(ReferenceHub hub) {}
        
        /// <summary>
        /// Executed when <see cref="hub"/> closed the menu.
        /// </summary>
        /// <param name="hub">The target hub.</param>
        protected virtual void ProperlyDisable(ReferenceHub hub) {}

        /// <summary>
        /// Gets the loaded <see cref="hub"/> menu. (menu that been displayed on the <see cref="hub"/>).
        /// </summary>
        /// <param name="hub">The target hub</param>
        /// <returns><see cref="Menu"/> if <see cref="hub"/> opened a menu, null if he was on the main menu.</returns>
        public static Menu TryGetCurrentPlayerMenu(ReferenceHub hub) => MenuSync.TryGetValue(hub, out Menu menu) ? menu : null;
        
        internal static void LoadForPlayer(ReferenceHub hub, Menu menu)
        {
            TryGetCurrentPlayerMenu(hub)?.ProperlyDisable(hub);

            if (!menu?.CheckAccess(hub) ?? true)
                menu = null;
            
            if (menu == null)
            {
                #if DEBUG
                if (LoadedMenus.Count(x => x.CheckAccess(hub)) == 1 && !Plugin.StaticConfig.ForceMainMenuEventIfOnlyOne)
                {
                    Menu m = LoadedMenus.First(x => x.CheckAccess(hub));
                    List<ServerSpecificSettingBase> s = m.GetSettings(true);
                    s.AddRange(GetGlobalKeybindings(hub, m));
                    MenuSync[hub] = m;
                    Parameters.SyncCache.Add(hub, new());
                    ServerSpecificSettingsSync.SendToPlayer(hub, s.ToArray());
                    Parameters.WaitUntilDone(hub, s);
                    m.ProperlyEnable(hub);
                    return;
                }
                #endif

                ServerSpecificSettingsSync.SendToPlayer(hub, GetMainMenu(hub));
                MenuSync[hub] = null;
                return;
            }

            List<ServerSpecificSettingBase> settings = menu.GetSettings(false);
            settings.AddRange(GetGlobalKeybindings(hub, menu));
            MenuSync[hub] = menu;
#if DEBUG
            Parameters.SyncCache.Add(hub, new());
#endif
            ServerSpecificSettingsSync.SendToPlayer(hub, settings.ToArray());
            menu.ProperlyEnable(hub);
        }

        /// <summary>
        /// Only used when player has left the server
        /// </summary>
        /// <param name="hub">The target hub.</param>
        internal static void DeletePlayer(ReferenceHub hub)
        {
            TryGetCurrentPlayerMenu(hub)?.ProperlyDisable(hub);
            MenuSync.Remove(hub);
        }

        /// <summary>
        /// Try get sub menu related to this menu.
        /// </summary>
        /// <param name="id">The sub menu id.</param>
        /// <returns>the sub-<see cref="Menu"/> if found.</returns>
        public Menu TryGetSubMenu(int id) => LoadedMenus.FirstOrDefault(x => x.Id == id && x.MenuRelated == GetType() && x != this);

        /// <summary>
        /// Gets the first keybind linked to <see cref="ss"/>.
        /// </summary>
        /// <param name="hub">The target hub.</param>
        /// <param name="ss">The specified <see cref="SSKeybindSetting"/> or <see cref="Keybind"/>.</param>
        /// <param name="menu">The <see cref="hub"/> current loaded menu, to get global or local keybinds.</param>
        /// <returns><see cref="Keybind"/> if found or not.</returns>
        public static Keybind TryGetKeybinding(ReferenceHub hub, ServerSpecificSettingBase ss, Menu menu = null)
        {
            int id = ss.SettingId;
            if (ss is Keybind)
                id -= 10000;

            foreach (var bind in GlobalKeybindingSync.Where(x => x.Key.CheckAccess(hub)))
            {
                foreach (var key in bind.Value)
                {
                    if (key.SettingId == id)
                        return key;
                }
            }
            
            if (menu == null)
                return null;
            if (!menu.CheckAccess(hub))
                return null;
            return menu.Settings.FirstOrDefault(x => x.SettingId == id) as Keybind;
        }

        /// <summary>
        /// Get a menu by <see cref="Type"/>.
        /// </summary>
        /// <param name="type">The type</param>
        /// <returns><see cref="Menu"/> if found.</returns>
        public static Menu GetMenu(Type type) => LoadedMenus.FirstOrDefault(x => x.GetType() == type);

        /// <summary>
        /// Reload the menu of the specified <see cref="ReferenceHub"/>.
        /// </summary>
        /// <param name="hub">The target hub.</param>
        public void Reload(ReferenceHub hub) => LoadForPlayer(hub, this);

        public void ReloadAll()
        {
            foreach (ReferenceHub hub in MenuSync.Where(x => x.Value == this).Select(x => x.Key))
                LoadForPlayer(hub, this);
        }
        
        public static void RegisterPin(ServerSpecificSettingBase[] toPin) => Pinned[Assembly.GetCallingAssembly()] = toPin;

        public static void UnregisterPin() => Pinned.Remove(Assembly.GetCallingAssembly());
    }
}