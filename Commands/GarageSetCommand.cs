using System.Collections.Generic;
using RFGarage.Models;
using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using RFGarage.Utils;

namespace RFGarage.Commands
{
    public class GarageSetCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;
        public string Name => "garageset";
        public string Help => "Set your default garage.";
        public string Syntax => "/garageset <garageName>";
        public List<string> Aliases => new List<string> { "gs", "gset"};
        public List<string> Permissions => new List<string> {"garageset"};
        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (command.Length != 1)
            {
                caller.SendChat(Plugin.Inst.Translate("rfgarage_command_invalid_parameter", Syntax), Plugin.MsgColor, Plugin.Conf.AnnouncerIconUrl);
                return;
            }

            var player = (UnturnedPlayer) caller;
            if (!GarageModel.TryParse(command[0], out var garage))
            {
                caller.SendChat(Plugin.Inst.Translate("rfgarage_command_garage_not_found"), Plugin.MsgColor, Plugin.Conf.AnnouncerIconUrl);
                return;
            }
            if (!player.CheckPermission(garage.Permission))
            {
                caller.SendChat(Plugin.Inst.Translate("rfgarage_command_garage_no_permission", garage.Name, garage.Permission), Plugin.MsgColor, Plugin.Conf.AnnouncerIconUrl);
                return;
            }

            if (!Plugin.SelectedGarageDict.ContainsKey(player.CSteamID))
                Plugin.SelectedGarageDict.Add(player.CSteamID, garage);
            else
                Plugin.SelectedGarageDict[player.CSteamID] = garage;
            caller.SendChat(Plugin.Inst.Translate("rfgarage_command_gset_success", garage.Name), Plugin.MsgColor, Plugin.Conf.AnnouncerIconUrl);
        }
    }
}