using System;
using System.Collections.Generic;
using RFGarage.Enums;
using RFGarage.Utils;
using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;

namespace RFGarage.Commands
{
    public class GarageRetrieveDrownCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;
        public string Name => "garageretrievedrown";
        public string Help => "Retrieve your drowned vehicle from the Drown Garage.";
        public string Syntax => "/garageretrievedrown";
        public List<string> Aliases => new List<string> {"grd"};
        public List<string> Permissions => new List<string> {"garageretrievedrown"};
        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (command.Length > 0)
            {
                UnturnedChat.Say(caller, Plugin.Inst.Translate("rfgarage_command_invalid_parameter", Syntax), Plugin.MsgColor);
                return;
            }

            var player = (UnturnedPlayer) caller;
            if (command.Length != 0) return;
            if (!CheckResponse(player))
                return;
            GarageUtil.LoadVgVehicleFromSql(player, "Drown", "Drowned", out var vehicle);
            UnturnedChat.Say(caller, Plugin.Inst.Translate("rfgarage_command_gr_success", 
                vehicle.asset.vehicleName, vehicle.asset.id, "Drown"), Plugin.MsgColor);
        }
        private static bool CheckResponse(UnturnedPlayer player)
        {
            var responseType = EResponseType.SUCCESS;
            
            if (!Plugin.DbManager.IsVehicleExist(player.CSteamID.m_SteamID.ToString(), "Drown", "Drowned"))
            {
                responseType = EResponseType.DONT_HAVE_VEHICLE;
            }

            switch (responseType)
            {
                case EResponseType.DONT_HAVE_VEHICLE:
                    UnturnedChat.Say(player, Plugin.Inst.Translate("rfgarage_command_garage_no_vehicle", "Drown"), Plugin.MsgColor);
                    return false;
                case EResponseType.SUCCESS:
                    return true;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}