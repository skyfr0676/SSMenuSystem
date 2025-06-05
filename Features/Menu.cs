// -----------------------------------------------------------------------
// <copyright file="Menu.cs" company="Skyfr0676 and Redforce04">
// Copyright (c) Skyfr0676 and Redforce04. All rights reserved.
// Licensed under the Undetermined license.
// </copyright>
// -----------------------------------------------------------------------
#pragma warning disable SA1018, SA1124, SA1401 //  Nullable symbol should be preceded with a space. Do not use regions. Field should be made private.
namespace SSMenuSystem.Features;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

using MEC;
using Examples;
using Interfaces;
using JetBrains.Annotations;
using Wrappers;
using UnityEngine;
using UserSettings.ServerSpecific;

/// <summary>
/// The Menu system.
/// </summary>
public abstract class Menu
{
#region Fields

    /// <summary>
    /// All synced parameters for a specified <see cref="ReferenceHub"/>.
    /// </summary>
    internal readonly Dictionary<ReferenceHub, List<ServerSpecificSettingBase>> InternalSettingsSync = new ();

    /// <summary>
    /// Gets all settings sent to the refHub. (only in the case of one GetSettings is not null or empty).
    /// </summary>
    internal readonly Dictionary<ReferenceHub, ServerSpecificSettingBase[]> SentSettings = new ();

    private static readonly Dictionary<ReferenceHub, Menu?> MenuSync = new ();
    private static readonly List<Menu> LoadedMenus = new ();
    private static readonly Dictionary<Assembly, SSTextArea[]> Pinned = new ();

    // ReSharper disable once FieldCanBeMadeReadOnly.Local
    private static Queue<Assembly> waitingAssemblies = new ();

    // private static readonly Dictionary<Menu, List<Keybind>> GlobalKeybindingSync = new();
#endregion
#region Properties

    /// <summary>
    /// Gets all loaded menus.
    /// </summary>
    public static List<Menu> Menus => LoadedMenus;

    /// <summary>
    /// Gets all global keybinds registered for each menu.
    /// </summary>
    [PublicAPI]
    public static Dictionary<Menu, List<Keybind>> GlobalKeybindingSync => Menus.ToDictionary(menu => menu, menu => menu.Settings?.Where(x => x is Keybind { IsGlobal: true }).Select(x => x as Keybind).ToList()) !;

    /// <summary>
    /// Gets all global keybinds registered for each menu.
    /// </summary>
    [Obsolete("Use GlobalKeybindingSync instead", true)]
    public static ReadOnlyDictionary<Menu, List<Keybind>> GlobalKeybindings => new (GlobalKeybindingSync);

    /// <summary>
    /// Gets or Sets the id of Menu. Must be above 0 and must not be equal to 0.
    /// </summary>
    public abstract int Id { get; set; }

    /// <summary>
    /// Gets or Sets the name of Menu.
    /// </summary>
    public abstract string Name { get; set; }

    /// <summary>
    /// Gets In-Build Settings.
    /// </summary>
    public abstract ServerSpecificSettingBase[] ? Settings { get; }

    /// <summary>
    /// Gets or Sets if this menu is related to a <see cref="MenuRelated"/> (will be shown as a SubMenu).
    /// </summary>
    public virtual Type? MenuRelated { get; set; } = null;

    /// <summary>
    /// Gets all synced parameters for a specified <see cref="ReferenceHub"/>.
    /// </summary>
    public ReadOnlyDictionary<ReferenceHub, List<ServerSpecificSettingBase>> SettingsSync => new (this.InternalSettingsSync);

    /// <summary>
    /// Gets the Hash of menu, based on <see cref="Name"/>. Mainly used to separate menu settings for client.
    /// </summary>
    public int Hash => Mathf.Abs(this.Name.GetHashCode() % 100000);

    /// <summary>
    /// Gets or Sets the description of Menu. Will be shown as <see cref="ServerSpecificSettingBase.HintDescription"/>.
    /// </summary>
    protected virtual string Description { get; set; } = string.Empty;

#endregion
#region Public Methods

    /// <summary>
    /// Register all menus in the <see cref="Assembly.GetCallingAssembly"/>.
    /// </summary>
    public static void RegisterAll() => Register(Assembly.GetCallingAssembly());

    /// <summary>
    /// Register specific menu.
    /// </summary>
    /// <param name="menu">The target menu.</param>
    /// <exception cref="ArgumentException">One of parameters of target menu is invalid. please check the <see cref="Exception.Message"/> to find the invalid parameter.</exception>
    public static void Register(Menu? menu)
    {
        if (menu is null)
        {
            return;
        }

        if (menu.MenuRelated == typeof(MainExample) && !Plugin.Instance!.Config.EnableExamples)
        {
            return;
        }

        Log.Debug($"loading Server Specific menu {menu.Name}...");
        if (CheckSameId(menu))
        {
            throw new ArgumentException($"another menu with id {menu.Id} is already registered. can't load {menu.Name}.");
        }

        if (menu.Id >= 0)
        {
            throw new ArgumentException($"menus ids must be < 0  (to let space for parameters and 0 is only for Main Menu).");
        }

        if (string.IsNullOrEmpty(menu.Name))
        {
            throw new ArgumentException($"menus name cannot be null or empty.");
        }

        if (Menus.Any(x => x.Name == menu.Name))
        {
            throw new ArgumentException($"two menus can't have the same name.");
        }

        Dictionary<Type, List<int>> ids = new ();

        foreach (ServerSpecificSettingBase action in menu.Settings!)
        {
            if (action is SSGroupHeader)
            {
                continue;
            }

            ServerSpecificSettingBase setting = action;
            if (setting is ISetting isSetting)
            {
                setting = isSetting.Base;
            }

            Type type = setting.GetType();

            ids.GetOrAdd(type, () => new List<int>());

            if (ids[type].Contains(setting.SettingId))
            {
                throw new ArgumentException($"id {setting.SettingId} for menu {menu.Name} is duplicated.");
            }

            if (setting.SettingId < 0)
            {
                throw new ArgumentException($"id above and equal to 0 is reserved for menus and main menu.");
            }

            ids[type].Add(setting.SettingId);
        }

        if (menu.MenuRelated != null)
        {
            if (LoadedMenus.All(m => m.GetType() != menu.MenuRelated))
            {
                throw new ArgumentException($"menu {menu.Name} has a invalid related menu ({menu.MenuRelated.FullName} has not been loaded.");
            }
        }

        LoadedMenus.Add(menu);
        menu.OnRegistered();
        Log.Debug($"Server Specific menu {menu.Name} is now registered!");
    }

    /// <summary>
    /// Calling assembly will be, loaded if SSMenuSystem is already initialized, or queued if not, and loaded when the plugin is initialized.
    /// </summary>
    [PublicAPI]
    public static void QueueOrRegister()
    {
        if (Plugin.Instance?.Config is null)
        {
            Assembly assembly = Assembly.GetCallingAssembly();
            if (!waitingAssemblies.Contains(assembly))
            {
                waitingAssemblies.Enqueue(assembly);
            }
        }
        else
        {
            Register(Assembly.GetCallingAssembly());
        }
    }

    /// <summary>
    /// Unload a menu.
    /// </summary>
    /// <param name="menu">The target menu.</param>
    [PublicAPI]
    public static void Unregister(Menu menu)
    {
        if (LoadedMenus.Contains(menu))
        {
            LoadedMenus.Remove(menu);
        }

        GlobalKeybindingSync.Remove(menu);
        foreach (KeyValuePair<ReferenceHub, Menu?> sync in MenuSync)
        {
            if (sync.Value == menu)
            {
                LoadForPlayer(sync.Key, null);
            }
        }
    }

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
    /// Gets the loaded <see cref="ReferenceHub"/> menu. (menu that been displayed on the <see cref="ReferenceHub"/>).
    /// </summary>
    /// <param name="hub">The target hub.</param>
    /// <returns><see cref="Menu"/> if <see cref="ReferenceHub"/> opened a menu, null if he was on the main menu.</returns>
    public static Menu? GetCurrentPlayerMenu(ReferenceHub hub) => MenuSync.TryGetValue(hub, out Menu? menu) ? menu : null;

    /// <summary>
    /// Gets the first keybind linked to <see cref="ServerSpecificSettingBase"/>.
    /// </summary>
    /// <param name="hub">The target hub.</param>
    /// <param name="ss">The specified <see cref="SSKeybindSetting"/> or <see cref="Keybind"/>.</param>
    /// <param name="menu">The <see cref="ReferenceHub"/> current loaded menu, to get global or local keybinds.</param>
    /// <returns><see cref="Keybind"/> if found or not.</returns>
    public static Keybind? TryGetKeybinding(ReferenceHub hub, ServerSpecificSettingBase ss, Menu? menu = null)
    {
        int id = ss.SettingId;
        if (ss is Keybind)
        {
            id -= 10000;
        }

        foreach (KeyValuePair<Menu, List<Keybind>> bind in GlobalKeybindingSync.Where(x => x.Key.CheckAccess(hub)))
        {
            foreach (Keybind key in bind.Value)
            {
                if (key.SettingId == id)
                {
                    return key;
                }
            }
        }

        if (menu == null)
        {
            return null;
        }

        if (!menu.CheckAccess(hub))
        {
            return null;
        }

        return menu.Settings?.FirstOrDefault(x => x.SettingId == id) as Keybind;
    }

    /// <summary>
    /// Get a menu by <see cref="Type"/>.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns><see cref="Menu"/> if found.</returns>
    public static Menu? GetMenu(Type type) => LoadedMenus.FirstOrDefault(x => x.GetType() == type);

    /// <summary>
    /// This is used to see if <see cref="ReferenceHub"/> can use <see cref="Menu"/> or not.
    /// </summary>
    /// <param name="hub">The target <see cref="ReferenceHub"/>.</param>
    /// <returns>True => Player can view and use the menu, and False => can't saw and use.</returns>
    public virtual bool CheckAccess(ReferenceHub hub) => true;

    /// <summary>
    /// Executed when action is executed on.
    /// </summary>
    /// <param name="hub">The players reference hub.</param>
    /// <param name="setting">The setting being input.</param>
    public virtual void OnInput(ReferenceHub hub, ServerSpecificSettingBase setting)
    {
    }

    /// <summary>
    /// Get settings for the specific <see cref="ReferenceHub"/>.
    /// </summary>
    /// <param name="hub">The target <see cref="ReferenceHub"/>.</param>
    /// <returns>A list of <see cref="ServerSpecificSettingBase"/> that will be sent into the player (like <see cref="Settings"/> but only for the <see cref="ReferenceHub"/>).</returns>
    public virtual ServerSpecificSettingBase[] ? GetSettingsFor(ReferenceHub hub)
    {
        return null;
    }

    /// <summary>
    /// Trys to get the sub menu related to this menu.
    /// </summary>
    /// <param name="id">The sub menu id.</param>
    /// <returns>the sub-<see cref="Menu"/> if found.</returns>
    public Menu? TryGetSubMenu(int id) => LoadedMenus.FirstOrDefault(x => x.Id == id && x.MenuRelated == this.GetType() && x != this);

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
        {
            LoadForPlayer(hub, this);
        }
    }

#endregion
#region Internal Methods

    /// <summary>
    /// Register all waiting assemblies when plugin is loaded.
    /// </summary>
    internal static void RegisterQueuedAssemblies()
    {
        while (waitingAssemblies.TryDequeue(out Assembly assembly))
        {
            Register(assembly);
        }
    }

    /// <summary>
    /// Unload all menus loaded.
    /// </summary>
    internal static void UnregisterAll()
    {
        foreach (KeyValuePair<ReferenceHub, Menu?> sync in MenuSync)
        {
            LoadForPlayer(sync.Key, null);
        }

        LoadedMenus.Clear();
        foreach (Menu menu in LoadedMenus.ToList())
        {
            Unregister(menu);
        }
    }

    /// <summary>
    /// Load <see cref="Menu"/> for <see cref="ReferenceHub"/> (only if he has access).
    /// </summary>
    /// <param name="hub">The players reference hub.</param>
    /// <param name="menu">The menu to load.</param>
    internal static void LoadForPlayer(ReferenceHub hub, Menu? menu)
    {
        GetCurrentPlayerMenu(hub)?.ProperlyDisable(hub);
        Log.Debug("try loading " + (menu?.Name ?? "main menu") + " for player " + hub.nicknameSync.MyNick);
        if (menu != null && !menu.CheckAccess(hub))
        {
            menu = null;
        }

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
        {
            Utils.SendToPlayer(hub, menu, settings.ToArray());
        }

        menu.ProperlyEnable(hub);
    }

    /// <summary>
    /// Only used when player has left the server.
    /// </summary>
    /// <param name="hub">The target hub.</param>
    internal static void DeletePlayer(ReferenceHub hub)
    {
        GetCurrentPlayerMenu(hub)?.ProperlyDisable(hub);
        MenuSync.Remove(hub);
    }

#endregion

#region Protected Methods

    /// <summary>
    /// Executed when <see cref="ReferenceHub"/> opened the menu.
    /// </summary>
    /// <param name="hub">the target hub.</param>
    protected virtual void ProperlyEnable(ReferenceHub hub)
    {
    }

    /// <summary>
    /// Executed when <see cref="ReferenceHub"/> closed the menu.
    /// </summary>
    /// <param name="hub">The target hub.</param>
    protected virtual void ProperlyDisable(ReferenceHub hub)
    {
    }

    /// <summary>
    /// Called when the <see cref="Menu"/> is registered.
    /// </summary>
    protected virtual void OnRegistered()
    {
    }

#endregion
#region Private Methods

    private static bool CheckSameId(Menu menu)
    {
        if (menu.MenuRelated == null)
        {
            return LoadedMenus.Any(x => x.Id == menu.Id && menu.MenuRelated == null);
        }

        return LoadedMenus.Where(x => x.MenuRelated == menu.MenuRelated).Any(x => x.Id == menu.Id);
    }

    /// <summary>
    /// Get the main menu for target hub.
    /// </summary>
    /// <param name="hub">The target referenceHub.</param>
    /// <returns>In-build parameters that will be shown to hub.</returns>
    private static ServerSpecificSettingBase[] GetMainMenu(ReferenceHub hub)
    {
        List<ServerSpecificSettingBase> mainMenu = new ();

        if (Plugin.Instance!.Config.AllowPinnedContent)
        {
            mainMenu.AddRange(Pinned.Values.SelectMany(pin => pin));
        }

        if (LoadedMenus.Where(x => x.CheckAccess(hub)).IsEmpty())
        {
            return mainMenu.ToArray();
        }

        mainMenu.Add(new SSGroupHeader("Main Menu"));
        foreach (Menu menu in LoadedMenus.Where(x => x.CheckAccess(hub)))
        {
            if (menu.MenuRelated == null)
            {
                mainMenu.Add(new SSButton(menu.Id, string.Format(Plugin.Instance.Translation.OpenMenu.Label, menu.Name), Plugin.Instance.Translation.OpenMenu.ButtonText, null, string.IsNullOrEmpty(menu.Description) ? null : menu.Description));
            }
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
    private static ServerSpecificSettingBase[] GetGlobalKeybindings(ReferenceHub hub, Menu? menu)
    {
        List<ServerSpecificSettingBase> keybindings = new ();

        if (GlobalKeybindingSync.Any(x => x.Key.CheckAccess(hub) && x.Key != menu && x.Value.Any()))
        {
            keybindings.Add(new SSGroupHeader(Plugin.Instance!.Translation.GlobalKeybindingTitle.Label, hint: Plugin.Instance.Translation.GlobalKeybindingTitle.Hint));
            foreach (KeyValuePair<Menu, List<Keybind>> menuKeybinds in GlobalKeybindingSync.Where(x => x.Key.CheckAccess(hub) && x.Key != menu))
            {
                foreach (Keybind keybind in menuKeybinds.Value)
                {
                    keybindings.Add(new SSKeybindSetting(keybind.SettingId + menuKeybinds.Key.Hash, keybind.Label, keybind.SuggestedKey, keybind.PreventInteractionOnGUI, keybind.HintDescription));
                }
            }
        }

        return keybindings.ToArray();
    }

    /// <summary>
    /// Register all menus of indicated assembly.
    /// </summary>
    /// <param name="assembly">The target <see cref="Assembly"/>.</param>
    private static void Register(Assembly assembly)
    {
        if (Plugin.Instance?.Config is null)
        {
            // plugin is not loaded.
            if (!waitingAssemblies.Contains(assembly))
            {
                waitingAssemblies.Enqueue(assembly);
            }

            return;
        }

        try
        {
            Log.Debug($"loading assembly {assembly.GetName().Name}...");
            List<Menu> loadedMenus = new ();
            foreach (Type type in assembly.GetTypes())
            {
                // only used for compatibility (throw error when loaded)
                if (type == typeof(AssemblyMenu))
                {
                    continue;
                }

                if (type == typeof(MainExample) && (!Plugin.Instance.Config.EnableExamples))
                {
                    continue;
                }

                if (type.IsAbstract || type.IsInterface)
                {
                    continue;
                }

                if (type.BaseType != typeof(Menu))
                {
                    continue;
                }

                Menu? menu = Activator.CreateInstance(type) as Menu;
                loadedMenus.Add(menu!);
            }

            IOrderedEnumerable<Menu> orderedMenus = loadedMenus.OrderBy(x => x.MenuRelated == null ? 0 : 1).ThenBy(x => x.Id);
            List<Menu> registeredMenus = new ();
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

    private List<ServerSpecificSettingBase> GetSettings(ReferenceHub hub)
    {
        List<ServerSpecificSettingBase> settings = new ();

        if (Plugin.Instance!.Config.AllowPinnedContent)
        {
            settings.AddRange(Pinned.Values.SelectMany(pin => pin));
        }

        if (LoadedMenus.First(x => x.CheckAccess(hub) && x.MenuRelated == null) != this || Plugin.Instance.Config.ForceMainMenuEvenIfOnlyOne)
        {
            settings.Add(this.MenuRelated != null ? new SSButton(0, string.Format(Plugin.Instance.Translation.ReturnTo.Label, GetMenu(this.MenuRelated)?.Name ?? "Unknown"), Plugin.Instance.Translation.ReturnTo.ButtonText) : new SSButton(0, Plugin.Instance.Translation.ReturnToMenu.Label, Plugin.Instance.Translation.ReturnToMenu.ButtonText));
        }

        if (LoadedMenus.First(x => x.CheckAccess(hub) && x.MenuRelated == null) == this && !Plugin.Instance.Config.ForceMainMenuEvenIfOnlyOne)
        {
            settings.Add(new SSGroupHeader(this.Name));
        }
        else
        {
            if (LoadedMenus.Count(x => x.MenuRelated == this.GetType() && x != this) > 0)
            {
                settings.Add(new SSGroupHeader(Plugin.Instance.Translation.SubMenuTitle.Label, hint: Plugin.Instance.Translation.SubMenuTitle.Hint));
            }
        }

        foreach (Menu s in LoadedMenus.Where(x => x.MenuRelated == this.GetType() && x != this))
        {
            settings.Add(new SSButton(s.Id, string.Format(Plugin.Instance.Translation.OpenMenu.Label, s.Name), Plugin.Instance.Translation.OpenMenu.ButtonText, null, string.IsNullOrEmpty(this.Description) ? null : this.Description));
        }

        if (LoadedMenus.First(x => x.CheckAccess(hub) && x.MenuRelated == null) != this || Plugin.Instance.Config.ForceMainMenuEvenIfOnlyOne)
        {
            settings.Add(new SSGroupHeader(this.Name, false, this.Description));
        }

        if (this is AssemblyMenu assemblyMenu &&
            assemblyMenu.ActuallySentToClient.TryGetValue(hub, out ServerSpecificSettingBase[] overrideSettings) && overrideSettings != null)
        {
            if (overrideSettings.IsEmpty())
            {
                settings.RemoveAt(settings.Count - 1);
            }

            settings.AddRange(overrideSettings);
            return settings;
        }

        ServerSpecificSettingBase[] ? oSettings = this.GetSettingsFor(hub);

        if ((this.Settings == null || this.Settings.IsEmpty()) && (oSettings == null || oSettings.IsEmpty()))
        {
            settings.RemoveAt(settings.Count - 1);
            return settings;
        }

        List<ServerSpecificSettingBase> sentSettings = new ();
        if (this.Settings != null)
        {
            foreach (ServerSpecificSettingBase t in this.Settings)
            {
                sentSettings.Add(t);
                if (t is ISetting setting)
                {
                    settings.Add(setting.Base);
                }
                else
                {
                    settings.Add(t);
                }
            }
        }

        if (oSettings != null && !oSettings.IsEmpty())
        {
            foreach (ServerSpecificSettingBase t in oSettings)
            {
                sentSettings.Add(t);
                if (t is ISetting setting)
                {
                    settings.Add(setting.Base);
                }
                else
                {
                    settings.Add(t);
                }
            }
        }

        this.SentSettings[hub] = sentSettings.ToArray();

        return settings;
    }

#endregion
}
#pragma warning restore SA1018, SA1124, SA1401
