using System;
using CommandSystem;
using PluginAPI.Core;
using ServerSpecificSyncer.Features;
using UserSettings.ServerSpecific;

namespace ServerSpecificSyncer
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class TestCommand : ICommand
    {
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            foreach (var paramter in Parameters.GetAllSyncedParameters(Player.Get(sender).ReferenceHub))
                sender.Respond($"{paramter.Label}: {paramter.DebugValue}");
            response = $"ok";
            return true;
        }

        public string Command => "negro";
        public string[] Aliases => Array.Empty<string>();
        public string Description => "ne";
    }
}