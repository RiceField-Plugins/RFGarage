using System;
using System.Collections.Generic;
using System.Linq;
using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using VirtualGarage.Enums;
using VirtualGarage.Models;
using VirtualGarage.Utils;

namespace VirtualGarage.Commands
{
    public class SuperGarageRetrieveCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;
        public string Name => "supergarageretrieve";
        public string Help => "Retrieve vehicle from the virtual garage with Superaccess.";
        public string Syntax => "/supergarageretrieve <steamID> <garageName> <vehicleName>";
        public List<string> Aliases => new List<string> {"sgr"};
        public List<string> Permissions => new List<string> {"supergarageretrieve"};
        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (command.Length != 3)
            {
                UnturnedChat.Say(caller, Plugin.Inst.Translate("virtualgarage_command_invalid_parameter", Syntax), Plugin.MsgColor);
                return;
            }

            var player = (UnturnedPlayer) caller;
            string garage;
            if (command[1].ToLower() != "drown")
            {
                if (!CheckResponse(player, command))
                    return;
                garage = Garage.Parse(command[1]).Name;
            }
            else
            {
                garage = "Drown";
            }
            GarageUtil.LoadVgVehicleFromSql(player, garage, command[1].ToLower() != "drown" ? command[2] : "Drowned", out var vehicle, command[0]);
            UnturnedChat.Say(caller, Plugin.Inst.Translate("virtualgarage_command_gr_success", vehicle.asset.vehicleName, vehicle.asset.id, garage), Plugin.MsgColor);
        }
        
        private static bool CheckResponse(UnturnedPlayer player, string[] commands)
        {
            GarageUtil.SuperGarageRetrieveChecks(player, out var responseType, commands);

            switch (responseType)
            {
                case EResponseType.DONT_HAVE_VEHICLE:
                    var garage = Garage.Parse(commands?[1]);
                    UnturnedChat.Say(player, Plugin.Inst.Translate("virtualgarage_command_sgr_garage_no_vehicle", commands?[0], garage.Name), Plugin.MsgColor);
                    return false;
                case EResponseType.GARAGE_NOT_FOUND:
                    UnturnedChat.Say(player, Plugin.Inst.Translate("virtualgarage_command_garage_not_found"), Plugin.MsgColor);
                    return false;
                case EResponseType.INVALID_STEAMID:
                    UnturnedChat.Say(player, Plugin.Inst.Translate("virtualgarage_command_invalid_id"), Plugin.MsgColor);
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