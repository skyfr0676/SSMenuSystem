using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PluginAPI.Core;
using UserSettings.ServerSpecific;

namespace ServerSpecificSyncer.Features
{
    public abstract class Menu
    {
        private static Dictionary<ReferenceHub, Menu> _menuSync = new();
        private static readonly List<Menu> LoadedMenus = new();
        public static List<Menu> Menus => LoadedMenus;
        private static readonly Dictionary<Menu, Keybind> GlobalKeybindingSync = new();
        
        public virtual bool CheckAccess(ReferenceHub hub) => true;

        public static void RegisterAll() => Register(Assembly.GetCallingAssembly());

        public static void Register(Assembly assembly)
        {
            foreach (var type in assembly.GetTypes())
            {
                if (type.IsAbstract || type.IsInterface)
                {
                    
                }
            }
        }
        
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
            
            LoadedMenus.Add(menu);
            Log.Info($"Server Specific menu {menu.Name} is now registered!");
        }

        private static bool CheckSameId(Menu menu)
        {
            if (menu.MenuRelated == null)
                return LoadedMenus.Any(x => x.Id == menu.Id && menu.MenuRelated == null);
            return LoadedMenus.Where(x => x.MenuRelated == menu.MenuRelated).Any(x => x.Id == menu.Id);
        }

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
        public static void UnregisterAll()
        {
            foreach (var sync in _menuSync)
                LoadForPlayer(sync.Key, null);
            LoadedMenus.Clear();
        }
        
        // TODO: DOCS
        public virtual int? MenuRelated { get; set; } = null;
        
        public abstract ServerSpecificSettingBase[] Settings { get; }

        public abstract string Name { get; set; }
        public virtual string Description { get; set; }

        public abstract int Id { get; set; }

        public static void RegisterAll(Menu[] menus)
        {
            LoadedMenus.Clear();
            LoadedMenus.AddRange(menus);
        }

        public static ServerSpecificSettingBase[] GetMainMenu(ReferenceHub hub)
        {
            List<ServerSpecificSettingBase> mainMenu = new() { new SSGroupHeader("Main Menu") };
            foreach (var menu in LoadedMenus)
            {
                if (!menu.MenuRelated.HasValue)
                    mainMenu.Add(new SSButton(menu.Id, menu.Name, "Open", null, string.IsNullOrEmpty(menu.Description) ? null : menu.Description));
            }
            
            mainMenu.AddRange(GetGlboalKeybindings(hub));

            return mainMenu.ToArray();
        }

        public static ServerSpecificSettingBase[] GetGlboalKeybindings(ReferenceHub hub)
        {
            List<ServerSpecificSettingBase> keybindings = new();
            
            if (GlobalKeybindingSync.Any(x => x.Key.CheckAccess(hub)))
            {
                keybindings.Add(new SSGroupHeader("Global Keybinds", hint:"don't take a look at this (nah seriously it's just to make some keybinds global)"));
                foreach (var globalKeybindings in GlobalKeybindingSync.Where(x => x.Key.CheckAccess(hub)))
                    keybindings.Add(new SSKeybindSetting(globalKeybindings.Value.SettingId, globalKeybindings.Value.Label, globalKeybindings.Value.SuggestedKey, globalKeybindings.Value.PreventInteractionOnGUI, globalKeybindings.Value.HintDescription));
            }

            return keybindings.ToArray();
        }
        
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
                new SSButton(0, "Retourner au menu", "Ouvrir"),
            };
            if (LoadedMenus.Count(x => x.MenuRelated == menu.Id && x != menu) > 0)
                settings.Add(new SSGroupHeader("Sub-Menus"));
            foreach (var s in LoadedMenus.Where(x => x.MenuRelated == menu.Id && x != menu))
                settings.Add(new SSButton(s.Id, menu.Name, "Open", null, string.IsNullOrEmpty(menu.Description) ? null : menu.Description));
            settings.Add(new SSGroupHeader(menu.Name, false, menu.Description));
            foreach (var t in menu.Settings)
            {
                if (t is Keybind bind)
                    settings.Add(new SSKeybindSetting(bind.SettingId, bind.Label, bind.SuggestedKey, bind.PreventInteractionOnGUI, bind.HintDescription));
                else
                    settings.Add(t);
            }
            settings.AddRange(GetGlboalKeybindings(hub));
            ServerSpecificSettingsSync.SendToPlayer(hub, settings.ToArray());
            _menuSync[hub] = menu;
            menu.ProperlyEnable(hub);
        }

        public static void DeletePlayer(ReferenceHub hub)
        {
            TryGetCurrentPlayerMenu(hub)?.ProperlyDisable(hub);
            _menuSync.Remove(hub);
        }

        public Menu TryGetSubMenu(int id) => LoadedMenus.FirstOrDefault(x => x.Id == id && x.MenuRelated == Id && x != this);

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
    }
}