using System;
using System.Collections.Generic;
using RFGarage.Enums;
using RFGarage.Models;
using RFGarage.Utils;
using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;

namespace RFGarage.Commands
{
    public class SuperGarageAddCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;
        public string Name => "supergarageadd";
        public string Help => "Add vehicle to specified player virtual garage with Superaccess.";
        public string Syntax => "/supergarageadd <steamID> <garageName> <vehicleName>";
        public List<string> Aliases => new List<string> {"sga"};
        public List<string> Permissions => new List<string> {"supergarageadd"};
        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (command.Length != 3)
            {
                UnturnedChat.Say(caller, Plugin.Inst.Translate("rfgarage_command_invalid_parameter", Syntax), Plugin.MsgColor);
                return;
            }

            var player = (UnturnedPlayer) caller;
            if (!CheckResponse(player, command, out var vehicle, out var vehicleRegion))
                return;
            var garage = GarageModel.Parse(command[1]);
            GarageUtil.SaveVgVehicleToSql(ulong.Parse(command[0]), garage.Name, command[2], vehicle);
            UnturnedChat.Say(caller, Plugin.Inst.Translate("rfgarage_command_sgadd_success", vehicle.asset.vehicleName, vehicle.asset.id, garage.Name), Plugin.MsgColor);
        }

        private static bool CheckResponse(UnturnedPlayer player, string[] commands, out InteractableVehicle vehicle, out BarricadeRegion vehicleRegion)
        {
            GarageUtil.SuperGarageAddChecks(player, commands, out vehicle, out vehicleRegion, out var responseType);
            switch (responseType)
            {
                case EResponseType.GARAGE_NOT_FOUND:
                    UnturnedChat.Say(player, Plugin.Inst.Translate("rfgarage_command_garage_not_found"), Plugin.MsgColor);
                    return false;
                case EResponseType.INVALID_STEAM_ID:
                    UnturnedChat.Say(player, Plugin.Inst.Translate("rfgarage_command_invalid_id"), Plugin.MsgColor);
                    return false;
                case EResponseType.SAME_NAME_AS_GARAGE:
                    UnturnedChat.Say(player, Plugin.Inst.Translate("rfgarage_command_vehicle_name_same_as_garage"), Plugin.MsgColor);
                    return false;
                case EResponseType.SUCCESS:
                    return true;
                case EResponseType.VEHICLE_NOT_FOUND:
                    UnturnedChat.Say(player, Plugin.Inst.Translate("rfgarage_command_vehicle_not_found"), Plugin.MsgColor);
                    return false;
                default:
                    UnturnedChat.Say(player, responseType.ToString(), Plugin.MsgColor);
                    return false;
            }
        }
    }
}