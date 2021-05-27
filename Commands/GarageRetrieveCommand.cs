using System;
using System.Collections.Generic;
using System.Linq;
using RFGarage.Enums;
using RFGarage.Models;
using RFGarage.Utils;
using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;

namespace RFGarage.Commands
{
    public class GarageRetrieveCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;
        public string Name => "garageretrieve";
        public string Help => "Retrieve your vehicle from the virtual garage.";
        public string Syntax => "/garageretrieve <garageName> <vehicleName> | /garageretrieve <vehicleName>";
        public List<string> Aliases => new List<string> {"gr", "vgr"};
        public List<string> Permissions => new List<string> {"garageretrieve"};
        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (command.Length > 2 || command.Length == 0)
            {
                caller.SendChat(Plugin.Inst.Translate("rfgarage_command_invalid_parameter", Syntax), Plugin.MsgColor, Plugin.Conf.AnnouncerIconUrl);
                return;
            }

            var player = (UnturnedPlayer) caller;
            switch (command.Length)
            {
                case 1 when Plugin.Conf.VirtualGarages.Any(g => string.Equals(g.Name, command[0], StringComparison.CurrentCultureIgnoreCase)):
                    caller.SendChat(Plugin.Inst.Translate("rfgarage_command_invalid_parameter", Syntax), Plugin.MsgColor, Plugin.Conf.AnnouncerIconUrl);
                    return;
                case 1 when !Plugin.Conf.VirtualGarages.Any(g => string.Equals(g.Name, command[0], StringComparison.CurrentCultureIgnoreCase)):
                {
                    if (!CheckResponse(player, command))
                        return;
                    var garage = Plugin.SelectedGarageDict[player.CSteamID];
                    GarageUtil.LoadVgVehicleFromSql(player, garage.Name, command[0], out var vehicle);
                    caller.SendChat(Plugin.Inst.Translate("rfgarage_command_gr_success", vehicle.asset.vehicleName, vehicle.asset.id, garage.Name), Plugin.MsgColor, Plugin.Conf.AnnouncerIconUrl);
                    return;
                }
                case 1:
                    caller.SendChat(Plugin.Inst.Translate("rfgarage_command_gr_invalid_parameter"), Plugin.MsgColor, Plugin.Conf.AnnouncerIconUrl);
                    return;
                case 2 when !CheckResponse(player, command):
                    return;
                case 2:
                {
                    var garage = GarageModel.Parse(command[0]);
                    GarageUtil.LoadVgVehicleFromSql(player, garage.Name, command[1], out var vehicle);
                    caller.SendChat(Plugin.Inst.Translate("rfgarage_command_gr_success", vehicle.asset.vehicleName, vehicle.asset.id, garage.Name), Plugin.MsgColor, Plugin.Conf.AnnouncerIconUrl);
                    return;
                }
                default:
                    caller.SendChat(Plugin.Inst.Translate("rfgarage_command_invalid_parameter"), Plugin.MsgColor, Plugin.Conf.AnnouncerIconUrl);
                    break;
            }
        }
        
        private static bool CheckResponse(UnturnedPlayer player, string[] commands)
        {
            GarageUtil.GarageRetrieveChecks(player, out var responseType, commands);
            GarageModel garageModel;
            switch (responseType)
            {
                case EResponseType.DONT_HAVE_VEHICLE:
                    garageModel = GarageModel.Parse(commands?[0]);
                    player.SendChat(Plugin.Inst.Translate("rfgarage_command_garage_no_vehicle", garageModel.Name), Plugin.MsgColor, Plugin.Conf.AnnouncerIconUrl);
                    return false;
                case EResponseType.GARAGE_NOT_FOUND:
                    player.SendChat(Plugin.Inst.Translate("rfgarage_command_garage_not_found"), Plugin.MsgColor, Plugin.Conf.AnnouncerIconUrl);
                    return false;
                case EResponseType.GARAGE_NO_PERMISSION:
                    garageModel = GarageModel.Parse(commands?[0]);
                    player.SendChat(Plugin.Inst.Translate("rfgarage_command_garage_no_permission", garageModel.Name, garageModel.Permission), Plugin.MsgColor, Plugin.Conf.AnnouncerIconUrl);
                    return false;
                case EResponseType.GARAGE_NOT_SELECTED:
                    player.SendChat(Plugin.Inst.Translate("rfgarage_command_gr_garage_not_selected"), Plugin.MsgColor, Plugin.Conf.AnnouncerIconUrl);
                    return false;
                case EResponseType.SUCCESS:
                    return true;
                default:
                    player.SendChat(responseType.ToString(), Plugin.MsgColor, Plugin.Conf.AnnouncerIconUrl);
                    return false;
            }
        }
    }
}