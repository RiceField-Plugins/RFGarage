using System.Collections.Generic;
using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using VirtualGarage.Models;
using VirtualGarage.Utils;

namespace VirtualGarage.Commands
{
    public class GarageSetCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;
        public string Name => "garageset";
        public string Help => "Set your default garage.";
        public string Syntax => "/garageset <garageName>";
        public List<string> Aliases => new List<string> {"gset"};
        public List<string> Permissions => new List<string> {"garageset"};
        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (command.Length != 1)
            {
                UnturnedChat.Say(caller, Plugin.Inst.Translate("virtualgarage_command_invalid_parameter", Syntax), Plugin.MsgColor);
                return;
            }

            var player = (UnturnedPlayer) caller;
            if (!Garage.TryParse(command[0], out var garage))
            {
                UnturnedChat.Say(caller, Plugin.Inst.Translate("virtualgarage_command_garage_not_found"), Plugin.MsgColor);
                return;
            }
            if (!player.HasPermission(garage.Permission))
            {
                UnturnedChat.Say(caller, Plugin.Inst.Translate("virtualgarage_command_garage_no_permission", garage.Name, garage.Permission), Plugin.MsgColor);
                return;
            }

            if (!Plugin.SelectedGarageDict.ContainsKey(player.CSteamID))
                Plugin.SelectedGarageDict.Add(player.CSteamID, garage);
            else
                Plugin.SelectedGarageDict[player.CSteamID] = garage;
            UnturnedChat.Say(caller, Plugin.Inst.Translate("virtualgarage_command_gset_success", garage.Name), Plugin.MsgColor);
        }
    }
}