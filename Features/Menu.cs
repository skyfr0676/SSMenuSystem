using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PluginAPI.Core;
using UserSettings.ServerSpecific;

namespace ServerSpecificSyncer.Features
{
    /// <summary>
    /// The Menu system.
    /// </summary>
    public abstract class Menu
    {
        private static Dictionary<ReferenceHub, Menu> _menuSync = new();
        private static readonly List<Menu> LoadedMenus = new();

        /// <summary>
        /// All menus loaded.
        /// </summary>
        public static List<Menu> Menus => LoadedMenus;
        private static readonly Dictionary<Menu, Keybind> GlobalKeybindingSync = new();
        
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
        public static void Register(Assembly assembly)
        {
            foreach (var type in assembly.GetTypes())
            {
                if (type.IsAbstract || type.IsInterface)
                {
                    
                }
            }
        }
        
        /// <summary>
        /// Register specific menu.
        /// </summary>
        /// <param name="menu">The target menu.</param>
        /// <exception cref="ArgumentException">One of parameters of target menu is invalid. please check the <see cref="Exception.Message"/> to find the invalid parameter.</exception>
        public static void Register(Menu menu)
        {
            Log.Debug($"loading Server Specific menu {menu.Name}...");
            if (CheckSameId(menu))
                throw new ArgumentException($"another menu with id {menu.Id} is already registered. can't load {menu.Name}.");
            if (menu.Id >= 0)
                throw new ArgumentException($"menus ids must be < 0  (to let space for parameters and 0 is only for Main Menu).");
            if (string.IsNullOrEmpty(menu.Name))
                throw new ArgumentException($"menus name cannot be null or empty.");

            List<int> ids = new();
            foreach (var setting in menu.Settings)
            {
                if (ids.Contains(setting.SettingId))
                    throw new ArgumentException($"id {setting.SettingId} for menu {menu.Name} is duplicated.");
                if (setting.SettingId < 0)
                    throw new ArgumentException($"id above and equal to 0 is reserved for menus and main menu.");
                    
                ids.Add(setting.SettingId);
                
                if (setting is Keybind bind && bind.IsGlobal)
                    GlobalKeybindingSync[menu] = bind;
            }

            if (menu.MenuRelated != null)
            {
                if (!LoadedMenus.Any(m => m.GetType() == menu.MenuRelated))
                    throw new ArgumentException($"menu {menu.Name} has a invalid related menu ({menu.MenuRelated.FullName} has not been loaded.");
            }
            
            LoadedMenus.Add(menu);
            Log.Info($"Server Specific menu {menu.Name} is now registered!");
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
            foreach (var sync in _menuSync)
            {
                if (sync.Value == menu)
                    LoadForPlayer(sync.Key, null);
            }
            if (LoadedMenus.Contains(menu))
                LoadedMenus.Remove(menu);
        }
        
        /// <summary>
        /// Unload all menus loaded.
        /// </summary>
        public static void UnregisterAll()
        {
            foreach (var sync in _menuSync)
                LoadForPlayer(sync.Key, null);
            LoadedMenus.Clear();
        }
        
        /// <summary>
        /// Gets or Sets if this menu is related to a <see cref="MenuRelated"/> (will be shown as a SubMenu).
        /// </summary>
        public virtual Type? MenuRelated { get; set; } = null;
        
        /// <summary>
        /// Gets In-Build Settings.
        /// </summary>
        public abstract ServerSpecificSettingBase[] Settings { get; }

        /// <summary>
        /// Gets or Sets the the name of Menu.
        /// </summary>
        public abstract string Name { get; set; }
        
        /// <summary>
        /// Gets or Sets the description of Menu. Will be shown as <see cref="ServerSpecificSettingBase.HintDescription"/>.
        /// </summary>
        public virtual string Description { get; set; }

        /// <summary>
        /// Gets or Sets the id of Menu. Must be above 0 and must not be equal to 0. 
        /// </summary>
        public abstract int Id { get; set; }

        /// <summary>
        /// Get the main menu for target hub.
        /// </summary>
        /// <param name="hub">The target referenceHub</param>
        /// <returns>In-build parameters that will be shown to hub.</returns>
        public static ServerSpecificSettingBase[] GetMainMenu(ReferenceHub hub)
        {
            List<ServerSpecificSettingBase> mainMenu = new() { new SSGroupHeader("Main Menu") };
            foreach (var menu in LoadedMenus)
            {
                if (menu.MenuRelated == null)
                    mainMenu.Add(new SSButton(menu.Id, menu.Name, "Open", null, string.IsNullOrEmpty(menu.Description) ? null : menu.Description));
            }
            
            mainMenu.AddRange(GetGlobalKeybindings(hub, null));

            return mainMenu.ToArray();
        }

        /// <summary>
        /// Gets global keybindings for hub.
        /// </summary>
        /// <param name="hub">The target hub.</param>
        /// <param name="menu">The loaded menu of ReferenceHub, so keys will not been duplicated.</param>
        /// <returns>In-build parameters that will be shown to hub.</returns>
        public static ServerSpecificSettingBase[] GetGlobalKeybindings(ReferenceHub hub, Menu menu)
        {
            List<ServerSpecificSettingBase> keybindings = new();
            
            if (GlobalKeybindingSync.Any(x => x.Key.CheckAccess(hub)))
            {
                keybindings.Add(new SSGroupHeader("Global Keybinds", hint:"don't take a look at this (nah seriously it's just to make some keybinds global)"));
                keybindings.AddRange(GlobalKeybindingSync.Where(x => x.Key.CheckAccess(hub) && x.Key != menu).Select(globalKeybindings => new SSKeybindSetting(globalKeybindings.Value.SettingId, globalKeybindings.Value.Label, globalKeybindings.Value.SuggestedKey, globalKeybindings.Value.PreventInteractionOnGUI, globalKeybindings.Value.HintDescription)).Cast<ServerSpecificSettingBase>());
            }

            return keybindings.ToArray();
        }
        
        /// <summary>
        /// Executed when action is executed on
        /// </summary>
        /// <param name="hub"></param>
        /// <param name="setting"></param>
        public virtual void OnInput(ReferenceHub hub, ServerSpecificSettingBase setting) {}
        public virtual void ProperlyEnable(ReferenceHub hub) {}
        public virtual void ProperlyDisable(ReferenceHub hub) {}

        public static Menu TryGetCurrentPlayerMenu(ReferenceHub hub) => _menuSync.TryGetValue(hub, out Menu menu) ? menu : null;

        public static void LoadForPlayer(ReferenceHub hub, Menu menu)
        {
            TryGetCurrentPlayerMenu(hub)?.ProperlyDisable(hub);
            if (menu == null)
            {
                ServerSpecificSettingsSync.SendToPlayer(hub, GetMainMenu(hub));
                _menuSync[hub] = null;
                return;
            }

            List<ServerSpecificSettingBase> settings = new()
            {
                new SSButton(0, "Return to menu", "Open"),
            };
            if (LoadedMenus.Count(x => x.MenuRelated == menu.GetType() && x != menu) > 0)
                settings.Add(new SSGroupHeader("Sub-Menus"));
            foreach (var s in LoadedMenus.Where(x => x.MenuRelated == menu.GetType() && x != menu))
                settings.Add(new SSButton(s.Id, menu.Name, "Open", null, string.IsNullOrEmpty(menu.Description) ? null : menu.Description));
            settings.Add(new SSGroupHeader(menu.Name, false, menu.Description));
            foreach (var t in menu.Settings)
            {
                if (t is Keybind bind)
                    settings.Add(new SSKeybindSetting(bind.SettingId, bind.Label, bind.SuggestedKey, bind.PreventInteractionOnGUI, bind.HintDescription));
                else
                    settings.Add(t);
            }
            settings.AddRange(GetGlobalKeybindings(hub, menu));
            ServerSpecificSettingsSync.SendToPlayer(hub, settings.ToArray());
            _menuSync[hub] = menu;
            menu.ProperlyEnable(hub);
        }

        public static void DeletePlayer(ReferenceHub hub)
        {
            TryGetCurrentPlayerMenu(hub)?.ProperlyDisable(hub);
            _menuSync.Remove(hub);
        }

        public Menu TryGetSubMenu(int id) => LoadedMenus.FirstOrDefault(x => x.Id == id && x.MenuRelated == GetType() && x != this);

        public static Keybind TryGetKeybind(ReferenceHub hub, ServerSpecificSettingBase ss, Menu menu = null)
        {
            if (GlobalKeybindingSync.Any(x => x.Key.CheckAccess(hub) && x.Value.SettingId == ss.SettingId))
                return GlobalKeybindingSync.First(x => x.Key.CheckAccess(hub) && x.Value.SettingId == ss.SettingId).Value;
            if (menu == null)
                return null;
            if (!menu.CheckAccess(hub))
                return null;
            return menu.Settings.FirstOrDefault(x => x.SettingId == ss.SettingId) as Keybind;
        }

        public static Menu GetMenu(Type type) => LoadedMenus.FirstOrDefault(x => x.GetType() == type);
    }
}