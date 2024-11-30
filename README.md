# ServerSpecificsSyncer

[![Downloads](https://img.shields.io/github/downloads/skyfr0676/ServerSpecificsSyncer/total)](https://github.com/skyfr0676/ServerSpecificsSyncer/releases/latest)

Server Specific Syncer is used to sync all server specifics menus on one thing, to avoid compatibilty errors

## how to use ServerSpecificSyncer as:
### Plugin creator

Reference ServerSpecificSyncer DLL using the [latest](https://github.com/skyfr0676/ServerSpecificsSyncer/releases/latest) version on your favorite editor

Create a new class

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

The `Id` **MUST** be above 0: 0 is restricted to menus button related and positive Ids is restricted to parameters

There is parameters for `Menu`:


| parameter | type | description | default |
|-----------------|:------:|:----------------:|:------------|
| Name | string | The name of the Menu, displayed has `Header` when the menu is displayed, and as `Label` on buttons |
| Settings | Array(ServerSpecificSettingBase) | All In-Game settings that will be displayed to the hub |
| Id | int | The id of the menu, must not be equal to other menus, and need to be above 0. |
| Description | string | The description of the menu, displayed has a `Hint` | not displayed |
| MenuRelated | type | Specify a another Menu to make current menu has a SubMenu of specified parameter | null |
| CheckAccess | method(ReferencHub) => bool | Check if the target hub has a access to the menu. access will be changed if the user change group | true |
| PropelyEnable | method(ReferenceHub) => void | Executed when target hub open the menu (not the GUI) | nothing |
| PropelyDisable | method(ReferenceHub) => void | Executed when target hub close the menu (not the GUI) | nothing |
| OnInput | method(ReferenceHub, ServerSpecificSettingBase) => void | Executed when target hub press on something displayed (that not related to `Return to menu` button for exemple). Only executed if the menu is displayed | nothing |

Now you need, on the start of the server, register `Menus` of your `Assembly`
```c#
Menu.RegisterAll();
```

### Server Owner
There is some plugins and Translations for:
 - EXILED

All configs and Translations use the built-in **EXILED** __config__ and __translations__, located in `.config/EXILED/Configs` (windows: `%APPDATA%/EXILED/Configs`)

 - NWAPI

configs use the built-in **NWAPI** __config__  __translations__ is inside __config__. they are located in `.config/SCP Secret Laboratory/PluginAPI/{port}` (windows: `%APPDATA%/SCP Secret Laboratory/PluginAPI/{port}`)

 - All
 
This is a board of configs:

| parameter | type | description | default |
|-----------------|:------:|:----------------:|:------------|
| IsEnabled | bool | Gets or sets a value indicating whether the plugin is enabled. | true |
| Debug | bool | Gets or sets a value indicating whether debug messages should be displayed in the console. | false |
| ShowErrorToClient | bool | Gets or sets a value indicating whether bugs (like `OnInput` errors) would be shown to the client | true |
| ShowFullErrorToClient | bool | Gets or sets a value indicating whether client can see the full stack trace (not recommended), not used if `ShowErrorToClient` is set to `false`. | false |
|ShowFullErrorToModerators | bool |Gets or sets a value indicating whether moderators (like developers) can see the full stack strace, not used if `ShowErrorToClient` is set to `false`. | true |

exemple of configs:

```yml

ss_syncer:
  is_enabled: true
  debug: false
  show_error_to_client: true
  show_full_error_to_client: false
  show_full_error_to_moderators: true

```

and translations:

| parameter | type | description | default |
|-----------------|:------:|:----------------:|:------------|
| OpenMenu | LabelButton | used in the menu buttons | "Open {0}" and "Open" |
| ReturnToMenu | LabelButton | used in the menu, the return button | "Return to menu" and "Return" |
| ReturnTo | LabelButton | used in the menu, the return button (when you are in a sub-menu | "Return to {0}" and "Open" |
| ReloadButton | LabelButton | When there is a error, this is the button to reload the menu., not used if `ShowErrorToClient` is set to `false`. | "Reload" and "Reload" |
| ServerError | string | When there is a server error, this message will be displayed in the collapsed area, not used if `ShowErrorToClient` is set to `false`. | "INTERNAL SERVER ERROR" |
| NoPermission |string |If the used doesn't have the permission to see full errors (not RA authentificated) this is will be displayed , not used if `ShowErrorToClient` is set to `false`. | "insufficient permissions to see the full errors"


```yml

ss_syncer:
  open_menu:
    label: 'Open {0}'
    button_text: 'Open'
  return_to_menu:
    label: 'Return to menu'
    button_text: 'Return'
  return_to:
    label: 'Return to {0}'
    button_text: 'Return'
  reload_button:
    label: 'Reload menus'
    button_text: 'Reload'
  server_error: 'INTERNAL SERVER ERROR'
  no_permission: 'insufficient permissions to see the full errors'

```

# if you see a bug, please report this [here](https://github.com/skyfr0676/ServerSpecificsSyncer/issues)
