using System;
using System.Collections.Generic;
using RFGarage.Enums;
using RFGarage.Models;
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
                caller.SendChat(Plugin.Inst.Translate("rfgarage_command_invalid_parameter", Syntax), Plugin.MsgColor, Plugin.Conf.AnnouncerIconUrl);
                return;
            }

            var player = (UnturnedPlayer) caller;
            if (command.Length != 0) return;
            if (!CheckResponse(player))
                return;
            GarageUtil.LoadVgVehicleFromSql(player, "Drown", "Drowned", out var vehicle);
            player.SendChat(Plugin.Inst.Translate("rfgarage_command_gr_success", 
                vehicle.asset.vehicleName, vehicle.asset.id, "Drown"), Plugin.MsgColor, Plugin.Conf.AnnouncerIconUrl);
        }
        private static bool CheckResponse(UnturnedPlayer player)
        {
            GarageUtil.GarageCheck(player, GarageModel.Parse("Drown"), out var responseType, true);
            if (!Plugin.DbManager.IsVehicleExist(player.CSteamID.m_SteamID.ToString(), "Drown", "Drowned"))
            {
                responseType = EResponseType.DONT_HAVE_VEHICLE;
            }
            switch (responseType)
            {
                case EResponseType.DONT_HAVE_VEHICLE:
                    player.SendChat(Plugin.Inst.Translate("rfgarage_command_garage_no_vehicle", "Drown"), Plugin.MsgColor, Plugin.Conf.AnnouncerIconUrl);
                    return false;
                case EResponseType.GARAGE_NOT_FOUND:
                    player.SendChat(Plugin.Inst.Translate("rfgarage_command_garage_not_found"), Plugin.MsgColor, Plugin.Conf.AnnouncerIconUrl);
                    return false;
                case EResponseType.GARAGE_NO_PERMISSION:
                    player.SendChat(Plugin.Inst.Translate("rfgarage_command_garage_no_permission", "Drown", "garage.drown"), Plugin.MsgColor, Plugin.Conf.AnnouncerIconUrl);
                    return false;
                case EResponseType.SUCCESS:
                    return true;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}