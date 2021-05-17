using System;
using System.Collections.Generic;
using System.Linq;
using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using VirtualGarage.Enums;
using VirtualGarage.Models;
using VirtualGarage.Utils;

namespace VirtualGarage.Commands
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
                UnturnedChat.Say(caller, Plugin.Inst.Translate("virtualgarage_command_invalid_parameter", Syntax), Plugin.MsgColor);
                return;
            }

            var player = (UnturnedPlayer) caller;
            switch (command.Length)
            {
                case 1 when Plugin.Conf.VirtualGarages.Any(g => string.Equals(g.Name, command[0], StringComparison.CurrentCultureIgnoreCase)):
                    UnturnedChat.Say(caller, Plugin.Inst.Translate("virtualgarage_command_invalid_parameter", Syntax), Plugin.MsgColor);
                    return;
                case 1 when !Plugin.Conf.VirtualGarages.Any(g => string.Equals(g.Name, command[0], StringComparison.CurrentCultureIgnoreCase)):
                {
                    if (!CheckResponse(player, command))
                        return;
                    var garage = Plugin.SelectedGarageDict[player.CSteamID];
                    GarageUtil.LoadVgVehicleFromSql(player, garage.Name, command[0], out var vehicle);
                    UnturnedChat.Say(caller, Plugin.Inst.Translate("virtualgarage_command_gr_success", vehicle.asset.vehicleName, vehicle.asset.id, garage.Name), Plugin.MsgColor);
                    return;
                }
                case 1:
                    UnturnedChat.Say(caller, Plugin.Inst.Translate("virtualgarage_command_gr_invalid_parameter"), Plugin.MsgColor);
                    return;
                case 2 when !CheckResponse(player, command):
                    return;
                case 2:
                {
                    var garage = Garage.Parse(command[0]);
                    GarageUtil.LoadVgVehicleFromSql(player, garage.Name, command[1], out var vehicle);
                    UnturnedChat.Say(caller, Plugin.Inst.Translate("virtualgarage_command_gr_success", vehicle.asset.vehicleName, vehicle.asset.id, garage.Name), Plugin.MsgColor);
                    return;
                }
                default:
                    UnturnedChat.Say(caller, Plugin.Inst.Translate("virtualgarage_command_invalid_parameter"), Plugin.MsgColor);
                    break;
            }
        }
        
        private static bool CheckResponse(UnturnedPlayer player, string[] commands)
        {
            GarageUtil.GarageRetrieveChecks(player, out var responseType, commands);
            Garage garage;
            switch (responseType)
            {
                case EResponseType.DONT_HAVE_VEHICLE:
                    garage = Garage.Parse(commands?[0]);
                    UnturnedChat.Say(player, Plugin.Inst.Translate("virtualgarage_command_garage_no_vehicle", garage.Name), Plugin.MsgColor);
                    return false;
                case EResponseType.GARAGE_NOT_FOUND:
                    UnturnedChat.Say(player, Plugin.Inst.Translate("virtualgarage_command_garage_not_found"), Plugin.MsgColor);
                    return false;
                case EResponseType.GARAGE_NO_PERMISSION:
                    garage = Garage.Parse(commands?[0]);
                    UnturnedChat.Say(player, Plugin.Inst.Translate("virtualgarage_command_garage_no_permission", garage.Name, garage.Permission), Plugin.MsgColor);
                    return false;
                case EResponseType.GARAGE_NOT_SELECTED:
                    UnturnedChat.Say(player, Plugin.Inst.Translate("virtualgarage_command_gr_garage_not_selected"), Plugin.MsgColor);
                    return false;
                case EResponseType.SUCCESS:
                    return true;
                default:
                    UnturnedChat.Say(player, responseType.ToString(), Plugin.MsgColor);
                    return false;
            }
        }
    }
}