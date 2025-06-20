using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using MEC;
using SSMenuSystem.Examples;
using SSMenuSystem.Features.Interfaces;
using SSMenuSystem.Features.Wrappers;
using UnityEngine;
using UserSettings.ServerSpecific;

namespace SSMenuSystem.Features
{
    /// <summary>
    /// The Menu system.
    /// </summary>
    public abstract class Menu
    {
        private static readonly Dictionary<ReferenceHub, Menu> MenuSync = new();
        private static readonly List<Menu> LoadedMenus = new();
        private static readonly Dictionary<Assembly, SSTextArea[]> Pinned = new();

        /// <summary>
        /// All menus loaded.
        /// </summary>
        public static List<Menu> Menus => LoadedMenus;
        // private static readonly Dictionary<Menu, List<Keybind>> GlobalKeybindingSync = new();

        /// <summary>
        /// All Global Keybinds registered for each menu.
        /// </summary>
        public static Dictionary<Menu, List<Keybind>> GlobalKeybindingSync => Menus.ToDictionary(
            menu => menu,
            menu => menu.Settings.Where(x => x is Keybind k && k.IsGlobal).Select(x => x as Keybind).ToList());

        /// <summary>
        /// All Global Keybinds registered for each menu.
        /// </summary>
        [Obsolete("Use GlobalKeybindingSync instead", true)]
        public static ReadOnlyDictionary<Menu, List<Keybind>> GlobalKeybindings => new(GlobalKeybindingSync);

        /// <summary>
        /// All synced parameters for a specified <see cref="ReferenceHub"/>.
        /// </summary>
        internal readonly Dictionary<ReferenceHub, List<ServerSpecificSettingBase>> InternalSettingsSync = new();

        /// <summary>
        /// All synced parameters for a specified <see cref="ReferenceHub"/>.
        /// </summary>
        public ReadOnlyDictionary<ReferenceHub, List<ServerSpecificSettingBase>> SettingsSync => new(InternalSettingsSync);

        /// <summary>
        /// This is used to see if <see cref="ReferenceHub"/> can use <see cref="Menu"/> or not.
        /// </summary>
        /// <param name="hub">The target <see cref="ReferenceHub"/></param>
        /// <returns>True => Player can view and use the menu, and False => can't saw and use.</returns>
        public virtual bool CheckAccess(ReferenceHub hub) => true;

        private static Queue<Assembly> _waitingAssemblies = new();

        /// <summary>
        /// Register all waiting assemblies when plugin is loaded.
        /// </summary>
        internal static void RegisterQueuedAssemblies()
        {
            while (_waitingAssemblies.TryDequeue(out Assembly assembly))
                Register(assembly);
        }

        /// <summary>
        /// Calling assembly will be, loaded if SSMenuSystem is already initialized, or queued if not, and loaded when the plugin is initialized.
        /// </summary>
        public static void QueueOrRegister()
        {
            if (Plugin.Instance?.Config is null)
            {
                Assembly assembly = Assembly.GetCallingAssembly();
                if (!_waitingAssemblies.Contains(assembly))
                    _waitingAssemblies.Enqueue(assembly);
            }
            else
                Register(Assembly.GetCallingAssembly());
        }

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
            if (Plugin.Instance?.Config is null) // plugin is not loaded.
            {
                if (!_waitingAssemblies.Contains(assembly))
                    _waitingAssemblies.Enqueue(assembly);
                return;
            }
            try
            {
                Log.Debug($"loading assembly {assembly.GetName().Name}...");
                List<Menu> loadedMenus = new();
                foreach (Type type in assembly.GetTypes())
                {
                    if (type == typeof(AssemblyMenu)) // only used for comptability (throw error when loaded)
                        continue;

                    if (type == typeof(MainExample) && (!Plugin.Instance.Config.EnableExamples))
                        continue;

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
                        Log.Error("menu path: " + menu.GetType().FullName);
#else
                        Log.Debug(e.ToString());
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
                Log.Debug(e.ToString());
#endif
            }
        }

        /// <summary>
        /// Register specific menu.
        /// </summary>
        /// <param name="menu">The target menu.</param>
        /// <exception cref="ArgumentException">One of parameters of target menu is invalid. please check the <see cref="Exception.Message"/> to find the invalid parameter.</exception>
        public static void Register(Menu menu)
        {
            if (menu == null)
                return;
            if (menu.MenuRelated == typeof(MainExample) && !Plugin.Instance.Config.EnableExamples)
                return;

            Log.Debug($"loading Server Specific menu {menu.Name}...");
            if (CheckSameId(menu))
                throw new ArgumentException($"another menu with id {menu.Id} is already registered. can't load {menu.Name}.");
            if (menu.Id == 0)
                throw new ArgumentException("Menus id must not be equal to 0. 0 is reserved for Main Menu.");
            /*if (menu.Id >= 0)
                //throw new ArgumentException($"menus ids must be < 0  (to let space for parameters and 0 is only for Main Menu).");*/
            if (string.IsNullOrEmpty(menu.Name))
                throw new ArgumentException($"menus name cannot be null or empty.");
            if (Menus.Any(x => x.Name == menu.Name))
                throw new ArgumentException($"two menus can't have the same name.");


            Dictionary<Type, List<int>> ids = new();

            foreach (ServerSpecificSettingBase action in menu.Settings)
            {
                if (action is SSGroupHeader)
                    continue;
                ServerSpecificSettingBase setting = action;
                if (setting is ISetting isSetting)
                    setting = isSetting.Base;

                Type type = setting.GetType();

                ids.GetOrAdd(type, () => new List<int>());

                if (ids[type].Contains(setting.SettingId))
                    throw new ArgumentException($"id {setting.SettingId} for menu {menu.Name} is duplicated.");
                if (setting.SettingId < 0)
                    throw new ArgumentException($"id above and equal to 0 is reserved for menus and main menu.");

                ids[type].Add(setting.SettingId);
            }

            if (menu.MenuRelated != null)
            {
                if (!LoadedMenus.Any(m => m.GetType() == menu.MenuRelated))
                    throw new ArgumentException($"menu {menu.Name} has a invalid related menu ({menu.MenuRelated.FullName} has not been loaded.");
            }

            LoadedMenus.Add(menu);
            menu.OnRegistered();
            Log.Debug($"Server Specific menu {menu.Name} is now registered!");
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
        internal static void UnregisterAll()
        {
            foreach (KeyValuePair<ReferenceHub, Menu> sync in MenuSync)
                LoadForPlayer(sync.Key, null);
            LoadedMenus.Clear();
            foreach (Menu menu in LoadedMenus.ToList())
                Unregister(menu);
        }

        /// <summary>
        /// Gets or Sets if this menu is related to a <see cref="MenuRelated"/> (will be shown as a SubMenu).
        /// </summary>
        #nullable enable
        public virtual Type? MenuRelated { get; set; } = null;
        #nullable disable

        /// <summary>
        /// Gets In-Build Settings.
        /// </summary>
        public abstract ServerSpecificSettingBase[] Settings { get; }

        /// <summary>
        /// Gets all settings sent to the refHub. (only in the case of one GetSettings is not null or empty)
        ///
        /// </summary>
        internal readonly Dictionary<ReferenceHub, ServerSpecificSettingBase[]> SentSettings = new();

        /// <summary>
        /// Gets the Hash of menu, based on <see cref="Name"/>. Mainly used to seperate menu settings for client.
        /// </summary>
        public int Hash => Mathf.Abs(Name.GetHashCode() % 100000);

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

            if (Plugin.Instance.Config.AllowPinnedContent)
                mainMenu.AddRange(Pinned.Values.SelectMany(pin => pin));

            if (LoadedMenus.Where(x => x.CheckAccess(hub)).IsEmpty())
                return mainMenu.ToArray();

            mainMenu.Add(new SSGroupHeader("Main Menu"));
            foreach (Menu menu in LoadedMenus.Where(x => x.CheckAccess(hub) && x.MenuRelated == null))
                mainMenu.Add(new SSButton(menu.Id, string.Format(Plugin.Instance.Translation.OpenMenu.Label, menu.Name), Plugin.Instance.Translation.OpenMenu.ButtonText, null, string.IsNullOrEmpty(menu.Description) ? null : menu.Description));

            mainMenu.AddRange(GetGlobalKeybindings(hub, null));

            return mainMenu.ToArray();
        }

        private List<ServerSpecificSettingBase> GetSettings(ReferenceHub hub)
        {
            List<ServerSpecificSettingBase> settings = new();

            if (Plugin.Instance.Config.AllowPinnedContent)
                settings.AddRange(Pinned.Values.SelectMany(pin => pin));

            if (LoadedMenus.First(x => x.CheckAccess(hub) && x.MenuRelated == null) != this || Plugin.Instance.Config.ForceMainMenuEvenIfOnlyOne)
            {
                if (MenuRelated != null)
                    settings.Add(new SSButton(0, string.Format(Plugin.Instance.Translation.ReturnTo.Label, Menu.GetMenu(MenuRelated)?.Name ?? "Unknown"),
                        Plugin.Instance.Translation.ReturnTo.ButtonText));
                else
                    settings.Add(new SSButton(0, Plugin.Instance.Translation.ReturnToMenu.Label,
                        Plugin.Instance.Translation.ReturnToMenu.ButtonText));
            }

            if (LoadedMenus.First(x => x.CheckAccess(hub) && x.MenuRelated == null) == this && !Plugin.Instance.Config.ForceMainMenuEvenIfOnlyOne)
                settings.Add(new SSGroupHeader(Name));
            else
            {
                if (LoadedMenus.Count(x => x.MenuRelated == GetType() && x != this) > 0)
                    settings.Add(new SSGroupHeader(Plugin.Instance.Translation.SubMenuTitle.Label, hint: Plugin.Instance.Translation.SubMenuTitle.Hint));
            }

            foreach (Menu s in LoadedMenus.Where(x => x.MenuRelated == GetType() && x != this))
                settings.Add(new SSButton(s.Id, string.Format(Plugin.Instance.Translation.OpenMenu.Label, s.Name), Plugin.Instance.Translation.OpenMenu.ButtonText, null, string.IsNullOrEmpty(Description) ? null : Description));

            if (LoadedMenus.First(x => x.CheckAccess(hub) && x.MenuRelated == null) != this || Plugin.Instance.Config.ForceMainMenuEvenIfOnlyOne)
                settings.Add(new SSGroupHeader(Name, false, Description));

            if (this is AssemblyMenu assemblyMenu &&
                assemblyMenu.ActuallySendedToClient.TryGetValue(hub, out ServerSpecificSettingBase[] overrideSettings) && overrideSettings != null)
            {
                if (overrideSettings.IsEmpty())
                    settings.RemoveAt(settings.Count - 1);
                settings.AddRange(overrideSettings);
                return settings;
            }

            ServerSpecificSettingBase[] oSettings = GetSettingsFor(hub);

            if ((Settings == null || Settings.IsEmpty()) && (oSettings == null || oSettings.IsEmpty()))
            {
                settings.RemoveAt(settings.Count - 1);
                return settings;
            }

            List<ServerSpecificSettingBase> sentSettings = new();
            if (Settings != null)
            {
                foreach (ServerSpecificSettingBase t in Settings)
                {
                    sentSettings.Add(t);
                    if (t is ISetting setting)
                        settings.Add(setting.Base);
                    else
                        settings.Add(t);
                }
            }

            if (oSettings != null && !oSettings.IsEmpty())
            {
                foreach (ServerSpecificSettingBase t in oSettings)
                {
                    sentSettings.Add(t);
                    if (t is ISetting setting)
                        settings.Add(setting.Base);
                    else
                        settings.Add(t);
                }
            }

            SentSettings[hub] = sentSettings.ToArray();

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

            if (GlobalKeybindingSync.Any(x => x.Key.CheckAccess(hub) && x.Key != menu && x.Value.Any()))
            {
                keybindings.Add(new SSGroupHeader(Plugin.Instance.Translation.GlobalKeybindingTitle.Label, hint:Plugin.Instance.Translation.GlobalKeybindingTitle.Hint));
                foreach (KeyValuePair<Menu, List<Keybind>> menuKeybinds in GlobalKeybindingSync.Where(x => x.Key.CheckAccess(hub) && x.Key != menu))
                {
                    foreach (Keybind keybind in menuKeybinds.Value)
                        keybindings.Add(new SSKeybindSetting(keybind.SettingId + menuKeybinds.Key.Hash, keybind.Label, keybind.SuggestedKey, keybind.PreventInteractionOnGUI, keybind.AllowSpectatorTrigger, keybind.HintDescription));
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
        /// Executed when <see cref="ReferenceHub"/> opened the menu.
        /// </summary>
        /// <param name="hub">the target hub.</param>
        protected virtual void ProperlyEnable(ReferenceHub hub) {}

        /// <summary>
        /// Executed when <see cref="ReferenceHub"/> closed the menu.
        /// </summary>
        /// <param name="hub">The target hub.</param>
        protected virtual void ProperlyDisable(ReferenceHub hub) {}

        /// <summary>
        /// Gets the loaded <see cref="ReferenceHub"/> menu. (menu that been displayed on the <see cref="ReferenceHub"/>).
        /// </summary>
        /// <param name="hub">The target hub</param>
        /// <returns><see cref="Menu"/> if <see cref="ReferenceHub"/> opened a menu, null if he was on the main menu.</returns>
        public static Menu GetCurrentPlayerMenu(ReferenceHub hub) => MenuSync.TryGetValue(hub, out Menu menu) ? menu : null;

        /// <summary>
        /// Load <see cref="Menu"/> for <see cref="ReferenceHub"/> (only if he has access).
        /// </summary>
        /// <param name="hub"></param>
        /// <param name="menu"></param>
        internal static void LoadForPlayer(ReferenceHub hub, Menu menu)
        {
            GetCurrentPlayerMenu(hub)?.ProperlyDisable(hub);
            Log.Debug("try loading " + (menu?.Name ?? "main menu") + " for player " + hub.nicknameSync.MyNick);
            if (menu != null && !menu.CheckAccess(hub))
                menu = null;

            if (menu == null && LoadedMenus.Count(x => x.CheckAccess(hub) && x.MenuRelated == null) == 1 && !Plugin.Instance!.Config!.ForceMainMenuEvenIfOnlyOne)
            {
                menu = Menus.First(x => x.CheckAccess(hub) && x.MenuRelated == null);
                Log.Debug($"triggered The only menu registered: {menu.Name}.");
            }

            if (menu == null)
            {
                Utils.SendToPlayer(hub, null, GetMainMenu(hub));
                MenuSync[hub] = null;
                return;
            }

            List<ServerSpecificSettingBase> settings = menu.GetSettings(hub);
            settings.AddRange(GetGlobalKeybindings(hub, menu));
            MenuSync[hub] = menu;

            if (!menu.SettingsSync.ContainsKey(hub))
            {
                Timing.RunCoroutine(Parameters.Sync(hub, menu, settings.ToArray()));
            }
            else
                Utils.SendToPlayer(hub, menu, settings.ToArray());

            menu.ProperlyEnable(hub);
        }

        /// <summary>
        /// Only used when player has left the server
        /// </summary>
        /// <param name="hub">The target hub.</param>
        internal static void DeletePlayer(ReferenceHub hub)
        {
            GetCurrentPlayerMenu(hub)?.ProperlyDisable(hub);
            MenuSync.Remove(hub);
        }

        /// <summary>
        /// Try get sub menu related to this menu.
        /// </summary>
        /// <param name="id">The sub menu id.</param>
        /// <returns>the sub-<see cref="Menu"/> if found.</returns>
        [Obsolete("Use TryGetSubMenu(int, out Menu) instead", true)]
        public Menu TryGetSubMenu(int id) => LoadedMenus.FirstOrDefault(x => x.Id == id && x.MenuRelated == GetType() && x != this);

        /// <summary>
        /// Try get sub menu related to this menu.
        /// </summary>
        /// <param name="id">The sub menu id.</param>
        /// <returns>the sub-<see cref="Menu"/> if found.</returns>
        public bool TryGetSubMenu(int id, out Menu menu) => (menu = LoadedMenus.FirstOrDefault(x => x.Id == id && x.MenuRelated == GetType() && x != this)) != null;

        /// <summary>
        /// Gets the first keybind linked to <see cref="ServerSpecificSettingBase"/>.
        /// </summary>
        /// <param name="hub">The target hub.</param>
        /// <param name="ss">The specified <see cref="SSKeybindSetting"/> or <see cref="Keybind"/>.</param>
        /// <param name="menu">The <see cref="ReferenceHub"/> current loaded menu, to get global or local keybinds.</param>
        /// <returns><see cref="Keybind"/> if found or not.</returns>
        public static Keybind TryGetKeybinding(ReferenceHub hub, ServerSpecificSettingBase ss, Menu menu = null)
        {
            int id = ss.SettingId;
            if (ss is Keybind)
                id -= 10000;

            foreach (KeyValuePair<Menu, List<Keybind>> bind in GlobalKeybindingSync.Where(x => x.Key.CheckAccess(hub)))
            {
                foreach (Keybind key in bind.Value)
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

        /// <summary>
        /// Reload all <see cref="ReferenceHub"/> that loaded the <see cref="Menu"/>.
        /// </summary>
        public void ReloadAll()
        {
            foreach (ReferenceHub hub in MenuSync.Where(x => x.Value == this).Select(x => x.Key).ToList())
                LoadForPlayer(hub, this);
        }


        /// <summary>
        /// Reload all <see cref="ReferenceHub"/> for all <see cref="Menu"/>
        /// </summary>
        public static void ReloadAllPlayers()
        {
            foreach (ReferenceHub hub in ReferenceHub.AllHubs)
                LoadForPlayer(hub, GetCurrentPlayerMenu(hub));
        }

        /// <summary>
        /// Reload the menu of the specified <see cref="ReferenceHub"/>.
        /// </summary>
        /// <param name="hub">The target hub.</param>
        public static void ReloadPlayer(ReferenceHub hub) => LoadForPlayer(hub, GetCurrentPlayerMenu(hub));

        /// <summary>
        /// Register <see cref="ServerSpecificSettingBase"/> that will be displayed on the top of all pages.
        /// </summary>
        /// <param name="toPin">the list of <see cref="ServerSpecificSettingBase"/> to pin.</param>
        public static void RegisterPin(SSTextArea[] toPin) =>
            Pinned[Assembly.GetCallingAssembly()] = toPin;

        /// <summary>
        /// Remove pins that was registered by <see cref="Assembly.GetCallingAssembly"/>.
        /// </summary>
        public static void UnregisterPin() => Pinned.Remove(Assembly.GetCallingAssembly());

        /// <summary>
        /// Called when the <see cref="Menu"/> is registered.
        /// </summary>
        protected virtual void OnRegistered(){}

        /// <summary>
        /// Get settings for the specific <see cref="ReferenceHub"/>
        /// <param name="hub">The target <see cref="ReferenceHub"/>.</param>
        /// <returns>A list of <see cref="ServerSpecificSettingBase"/> that will be sent into the player (like <see cref="Settings"/> but only for the <see cref="ReferenceHub"/>)</returns>
        /// </summary>
        public virtual ServerSpecificSettingBase[] GetSettingsFor(ReferenceHub hub)
        {
            return null;
        }

    }
}