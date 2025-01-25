# SSMenuSystem
*from "ServerSpecific-Menu System"*

[![Downloads](https://img.shields.io/github/downloads/skyfr0676/SSMenuSystem/total?style=for-the-badge)](https://github.com/skyfr0676/ServerSpecificsSyncer/releases/latest)
[![NuGet Version](https://img.shields.io/nuget/v/SSMenuSystem?style=for-the-badge)](https://www.nuget.org/packages/SSMenuSystem/)
![Open Issues](https://img.shields.io/github/issues/skyfr0676/SSMenuSystem?style=for-the-badge)
![Open Pull Requests](https://img.shields.io/github/issues-pr/skyfr0676/SSMenuSystem?style=for-the-badge)
![Forks](https://img.shields.io/github/forks/skyfr0676/SSMenuSystem.svg?style=for-the-badge)
![Stars](https://img.shields.io/github/stars/skyfr0676/SSMenuSystem?style=for-the-badge)
![](https://img.shields.io/badge/.NET-4.8-512BD4?logo=dotnet&logoColor=fff&style=for-the-badge)<br/>
SSMenuSystem is a framework that add SS Plugin comptability and a menu system.

## how to use ServerSpecificSyncer as:
### Server Owner
To install `SSMenuSystem` on your server, all you need is:
- For **EXILED**:
    - Install the [latest](https://github.com/skyfr0676/SSMenuSystem/releases/latest/download/SSMenuSystem-EXILED.dll) EXILED version (Name should be `SSMenuSystem-EXILED.dll`)
    - Put it in `.config/EXILED/Plugins` (replace `.config` with `%appdata%` on Windows) *Note: Harmony is already installed on EXILED so you don't have to install the dependency*
    - Restart the server
        - Configs should be on `.config/EXILED/Configs/{port}-config.yml`
        - Translations should be on `.config/EXILED/Configs/{port}-translations.yml`
- For **NWAPI**:
    - Install the [latest](https://github.com/skyfr0676/SSMenuSystem/releases/latest/download/SSMenuSystem-NWAPI.dll) NWAPI version (Name should be  `SSMenuSystem-NWAPI.dll`)
    - Put it in `.config/SCP Secret Laboratory/PluginAPI/Plugins/{port}` (replace `.config` with `%appdata%` on windows)
        - Install the [latest](https://github.com/skyfr0676/SSMenuSystem/releases/latest/download/0Harmony.dll) harmony version used (Name should be `0Harmony.dll`)
    - Put it in `.config/SCP Secret Laboratory/PluginAPI/Plugins/{port}/dependencies` (replace `.config` with `%appdata%` on windows)
    - Restart the server
        - both configs and translations should be on `.config/SCP Seret Laboratory/PluginAPI/Plugins/{port}/SSMenuSystem/config.yml`


Now you know how to install `SSMenuSystem` !<br/>
To help you to configure the plugin, there is a board of configs and translations:
### configuration:
| parameter                    | yml value                         | type                |                                                                                                             description                                                                                                             |          default value          |
|------------------------------|-----------------------------------|---------------------|:-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------:|:-------------------------------:|
| IsEnabled                    | is_enabled                        | bool                |                                                                                               define if the plugin is enabled or not.                                                                                               |              true               |
| Debug                        | debug                             | bool                |                                                                            define if the plugin can log debug info or not (attention: can be intrusive)                                                                             |              false              |
| AllowPinnedContent           | allow_pinned_content              | bool                |                                                                  Describe if pinned contnet is allowed. If set to false, all pinned content will not be displayed.                                                                  |              true               |
| ShowErrorToClient            | show_error_to_client              | bool                | If there is an error in the `OnInput` (mostly because of `Action`, the client will see a message that indicated an internal server error is occured, with a Reload button. If set to false, main menu will be automatically loaded. |              true               |
| ShowFullErrorToClient        | show_full_error_to_client         | bool                |                                                               **HIGHLY UNRECOMMENDED TO SET ON TRUE**<br/> The client will see the error occured if it's set to true.                                                               |              false              |
| ShowFullErrorToModerators    | show_full_error_to_moderators     | bool                |                                                       On the "error" page, if player has `RemoteAdmin` permission (see Remote Admin), the moderator will see the total error.                                                       |              true               |
| ForceMainMenuEventIfOnlyOne  | force_main_menu_event_if_only_one | bool                |                                                **Disabled feature**<br/>*If only one menu is registered, instead of displaying the main menu, it's display the only menu available.*                                                |              false              |
| ShowGlobalKeybindingsWarning | show_global_keybindings_warning   | bool                |                                         **Temporary parameter** due to Global Keybinds issue, a warning is displayed instead of global keybinds. This parameters can disable this warning.                                          |              false              |
| EnableExamples               | enable_examples                   | bool                |                 This plugin contains examples. The parameter simply enable examples.<br/>WARNING: can be game-breaking due to features inside, like human scanner (available even for SCPs, speed boost or others).                 |              false              |
| ComptabilitySystem           | comptability_system               | ComptabilitySystem  |                                                                                             All configs related to Comptability system.                                                                                             | Instance of ComptabilitySystem  |

This board is related to ComptabilitySystem:

| parameter           | yml value                                    | type                |                     description                      |          default value          |
|---------------------|----------------------------------------------|---------------------|:----------------------------------------------------:|:-------------------------------:|
| ComptabilityEnabled | comptability_enabled                         | bool                | Define if the comptability system is enabled or not. |              true               |


Configs written in a YML config:

```yml
is_enabled: true
debug: true
# Whether pins is allowed or not (pin is a thing that has been displayed on all menus).
allow_pinned_content: true
# Whether clients (= non-moderators) whould see errors or not.
show_error_to_client: true
# Whether clients (= non-moderators) whould see total errors (= some plugins-content name) or not. HIGLY UNRECOMMENDED TO SET TRUE.
show_full_error_to_client: false
# Whether moderators (= has RA access) whould see total errors (= some plugins-content name).
show_full_error_to_moderators: true
# If there is only one menu registered and this set to false, this menu would be automatiquely displayed. Disabled.
force_main_menu_event_if_only_one: false
# because GlobalKeybinds is disabled, set this to false to remove the warning displayed.
show_global_keybindings_warning: true
# Whether examples is enabled. Warning: if set to true, some content of examples would be Game breaking (speed ability, scan ability, etc...).
enable_examples: true
comptability_system:
    # If enabled, the comptability system will be enabled and all plugins that use SSSystem will be registered as menu.
    comptability_enabled: true
```

### translations:

| parameter               | yml value               |    type     |                                                     description                                                      | default                                                                                                        |
|-------------------------|-------------------------|:-----------:|:--------------------------------------------------------------------------------------------------------------------:|:---------------------------------------------------------------------------------------------------------------|
| OpenMenu                | open_menu               | LabelButton |                                   Used in the main menu, to display menu buttons.                                    | - "Open {0}"<br/> - Open                                                                                       |
| ReturnToMenu            | return_to_menu          | LabelButton | Used when menu is opened, at the top, to display the "return to menu" button (not into parent menu, see `ReturnTo`). | "Return to menu<br/> - Return                                                                                  |
| ReturnTo                | return_to               | LabelButton |               Used when sub-menu is opened, at the top, to display the "return to parent menu" button.               | - Return to {0}<br/> - Return                                                                                  |
| ReloadButton            | reload_button           | LabelButton |              Only used when a client saw an error, used to show the "return into the main menu" button.              | - Reload menus<br/> - Reload                                                                                   |
| GlobalKeybindingTitle   | global_keybinding_title | GroupHeader |                         Display the "Global keybindings" header. Can change Label and Hint.                          | - Global Keybinding<br/> - don't take a look at this (nah seriously it's just to make some keybindings global) |
| ServerError             | server_error            |   string    |                       Only displayed when error is thrown, to alert client of an actual error.                       | INTERNAL SERVER ERROR                                                                                          |
| SubMenuTitle            | sub_menu_title          |   string    |                    The text will be displayed in the header, if sub-menus exist in a parent menu.                    | Sub-Menus                                                                                                      |

And you will think (i'm sure :kappa:) "But what is "LabelButton" and "GroupHeader" type ?"

It's really simple ! it's just a contraction of two translations:
- for LabelButton:
    - The label
    - The button text
- And for GroupHeader:
    - The label
    - The hint

and there is an example of yml translation format:
```yml
# On the main-menu, button displayed to open a menu where {0} = menu name.
open_menu:
    label: 'Open {0}'
    button_text: 'Open'
# the button displayed when menu is opened.
return_to_menu:
    label: 'Return to menu'
    button_text: 'Return'
# The button that displayed when sub-menu is opened (return to related menu) where {0} = menu name.
return_to:
    label: 'Return to {0}'
    button_text: 'Return'
# The reload button.
reload_button:
    label: 'Reload menus'
    button_text: 'Reload'
global_keybinding_title:
    label: 'Global Keybinding'
    hint: "don't take a look at this (nah seriously it's just to make some keybindings global)"
# Text displayed when an error is occured (to avoid client crash + explain why it's don't work). Can accept TextMeshPro tags.
server_error: 'INTERNAL SERVER ERROR'
# Title of sub-menus when there is one.
sub_menu_title: 'Sub-Menus'
# Translation when player doesn't have permission to see total errors (= see a part of code name).
no_permission: 'insufficient permissions to see the full errors'

```
Now that you understand how to install `SSMenuSystem` on your server, we will now see how to use it as a plugin dependency

### Plugin creator

Reference SSMenuSystem DLL using the [latest](https://github.com/skyfr0676/ServerSpecificsSyncer/releases/latest) version (or use [Nuget package](https://www.nuget.org/packages/SSMenuSystem/)) on your favorite editor

 - Create a new class

this is a example of a menu class (with needed `override`):

```c#
    public class Test : Menu
    {
        protected override ServerSpecificSettingBase[] Settings => new ServerSpecificSettingBase[]
        {

        };

        public override string Name { get; set; } = "Test";
        public override int Id { get; set; } = -1;
    }
```

`SSMenuSystem` use a Wrapper system (not obligatory), where you can directly link an action (see [examples](https://github.com/skyfr0676/SSMenuSystem/tree/master/Examples) for explicit utilization)<br/>
`SSMenuSystem` use a new Keybind system, related to the new system of pages:
 - Basic wrapper system, with an Action linked: related to other wrappers
 - Global setting (**currently disabled due to a desync issue**)
   - Global settings can display (and use) the keybind in all pages (main menu, sub-menu and menu), below the "Global Keybindings" header.

The `Id` **MUST** be less than 0: 0 is restricted to menus button related and positive Ids is restricted to parameters

There is parameters for `Menu`:

### static version:

| parameter name       |                               type                                | description                                                                                                                                          |
|----------------------|:-----------------------------------------------------------------:|------------------------------------------------------------------------------------------------------------------------------------------------------|
| QueueOrRegister      |                         method() => void                          | register all menus from Assembly if `SSMenuSystem` is loaded or Queue assembly if not.                                                               |
| RegisterAll          |                         method() => void                          | register all menus from Assembly. If `SSMenuSystem` is not loaded, then Queue the assembly.                                                          |
| Register             |                       method(Menu) => void                        | register specified menu.                                                                                                                             |
| Unregister           |                       method(Menu) => void                        | Unregister specified menu.                                                                                                                           |
| GetCurrentPlayerMenu |                   method(ReferenceHub) => void                    | Get the current player menu, if there is one.                                                                                                        |
| TryGetKeybinding     | method(ReferenceHub, ServerSpecificSettingBase, Menu?) => Keybind | Get the first linked keybind using a ServerSpecificSettingBase, that contains different information like the label or the id.                        |
| GetMenu              |                       method(Type) => Menu                        | Get the menu registered with the Type.<br/>Usually used to get with MenuRelated.                                                                     |
| RegisterPin          |         method(Array(ServerSpecificSettingBase)) => void          | Register a bundle of pins. Pins are displayed in the top of the menu, above the "Return to something" button. All pins are linked with the assembly. |
| UnregisterPin        |                         method() => void                          | Unregister all pins registered by the calling assembly.                                                                                              |

### instance version:

| parameter name  |                          type                           | description                                                                                                                                             | default value                        |
|-----------------|:-------------------------------------------------------:|---------------------------------------------------------------------------------------------------------------------------------------------------------|:-------------------------------------|
| Name            |                         string                          | The name of the Menu, displayed has `Header` when the menu is displayed, and as `Label` on buttons                                                      |                                      |
| Settings        |            Array(ServerSpecificSettingBase)             | All In-Game settings that will be displayed to the hub                                                                                                  |                                      |
| Id              |                           int                           | The id of the menu, must not be equal to other menus, and need to be above 0.                                                                           |                                      |
| Description     |                         string                          | The description of the menu, displayed has a `Hint`                                                                                                     | null (not displayed)                 |
| MenuRelated     |                          type                           | Specify a another Menu to make current menu has a SubMenu of specified parameter                                                                        | null                                 |
| Hash            |                    int (only getter)                    | The Read-Only Hash from Menu to separate him from another menus.                                                                                        | Automatically specified with `Name`. |
| OnInput         | method(ReferenceHub, ServerSpecificSettingBase) => void | Executed when target hub press on something displayed (that not related to `Return to menu` button for example). Only executed if the menu is displayed | nothing                              |
| SettingsSync    | ReadOnly(ReferenceHub, List(ServerSpecificSettingBase)) | List that contains all synced parameters for `ReferenceHub`.                                                                                            | defined by `SSMenuSystem`            |
| CheckAccess     |              method(ReferenceHub) => bool               | Check if the target hub has a access to the menu. access will be changed if the user change group                                                       | true                                 |
| ProperlyEnable  |              method(ReferenceHub) => void               | Executed when target hub open the menu (not the GUI)                                                                                                    | nothing                              |
| ProperlyDisable |              method(ReferenceHub) => void               | Executed when target hub close the menu (not the GUI)                                                                                                   | nothing                              |
| TryGetSubMenu   |                   method(int) => Menu                   | Get sub-menu from the instance, using the id, if there is one.                                                                                          | Read-Only Method                     |
| Reload          |              method(ReferenceHub) => void               | Reload the menu for player (or return to menu if he doesn't have access anymore.                                                                        | Read-Only Method                     |
| ReloadAll       |                    method() => void                     | Reload the menu for all players that where actually connected on.                                                                                       | Read-Only Method                     |



Now you need, on the start of the server, register `Menus` of your `Assembly`
```c#
Menu.RegisterAll();
```

for EXILED, you can call this when the plugin is enabled:
```c#
public override void OnEnabled()
{
    Menu.RegisterAll();
    base.OnEnabled();
}
```

and for NWAPI,the method is almost the same:
```c#
[PluginAPI.Core.Attributes.PluginEntryPoint("MyAmazingPluginName", "1.0.0", "My Amazing Description", "MyAmazingName")]
public void OnEnabled()
{
    Menu.RegisterAll();
}
```

# if you see a bug, please report this [here](https://github.com/skyfr0676/ServerSpecificsSyncer/issues)
## thank you for your cooperation !