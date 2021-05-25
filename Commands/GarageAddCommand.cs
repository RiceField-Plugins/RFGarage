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
    public class GarageAddCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;
        public string Name => "garageadd";
        public string Help => "Add vehicle to your virtual garage.";
        public string Syntax => "/garageadd <garageName> <vehicleName> | /garageadd <vehicleName>";
        public List<string> Aliases => new List<string> {"gadd", "vgadd"};
        public List<string> Permissions => new List<string> {"garageadd"};
        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (command.Length > 2 || command.Length == 0)
            {
                caller.SendChat(Plugin.Inst.Translate("virtualgarage_command_invalid_parameter", Syntax), Plugin.MsgColor, Plugin.Conf.AnnouncerIconUrl);
                return;
            }

            var player = (UnturnedPlayer) caller;
            switch (command.Length)
            {
                case 1 when Plugin.Conf.VirtualGarages.Any(g => string.Equals(g.Name, command[0], StringComparison.CurrentCultureIgnoreCase)):
                    caller.SendChat(Plugin.Inst.Translate("virtualgarage_command_invalid_parameter", Syntax), Plugin.MsgColor, Plugin.Conf.AnnouncerIconUrl);
                    return;
                case 1 when !Plugin.Conf.VirtualGarages.Any(g => string.Equals(g.Name, command[0], StringComparison.CurrentCultureIgnoreCase)):
                {
                    if (!CheckResponse(player, command, out var vehicle, out var vehicleRegion))
                        return;
                    var garage = Plugin.SelectedGarageDict[player.CSteamID];
                    GarageUtil.SaveVgVehicleToSql(player.CSteamID.m_SteamID, garage.Name, command[0], vehicle, vehicleRegion);
                    caller.SendChat(Plugin.Inst.Translate("virtualgarage_command_gadd_success", vehicle.asset.vehicleName, vehicle.asset.id, garage.Name), Plugin.MsgColor, Plugin.Conf.AnnouncerIconUrl);
                    return;
                }
                case 1:
                    caller.SendChat(Plugin.Inst.Translate("virtualgarage_command_invalid_parameter"), Plugin.MsgColor, Plugin.Conf.AnnouncerIconUrl);
                    return;
                case 2:
                {
                    if (!CheckResponse(player, command, out var vehicle, out var vehicleRegion))
                        return;
                    var garage = GarageModel.Parse(command[0]);
                    GarageUtil.SaveVgVehicleToSql(player.CSteamID.m_SteamID, garage.Name, command[1], vehicle, vehicleRegion);
                    caller.SendChat(Plugin.Inst.Translate("virtualgarage_command_gadd_success", vehicle.asset.vehicleName, vehicle.asset.id, garage.Name), Plugin.MsgColor, Plugin.Conf.AnnouncerIconUrl);
                    return;
                }
                default:
                    caller.SendChat(Plugin.Inst.Translate("virtualgarage_command_invalid_parameter"), Plugin.MsgColor, Plugin.Conf.AnnouncerIconUrl);
                    break;
            }
        }

        private static bool CheckResponse(UnturnedPlayer player, string[] commands, out InteractableVehicle vehicle, out BarricadeRegion vehicleRegion)
        {
            GarageUtil.GarageAddChecks(player, commands, out vehicle, out vehicleRegion, out var responseType,
                out var blacklistedID);
            GarageModel garageModel;
            switch (responseType)
            {
                case EResponseType.VEHICLE_NOT_FOUND:
                    player.SendChat(Plugin.Inst.Translate("virtualgarage_command_vehicle_not_found"), Plugin.MsgColor, Plugin.Conf.AnnouncerIconUrl);
                    return false;
                case EResponseType.GARAGE_NOT_FOUND:
                    player.SendChat(Plugin.Inst.Translate("virtualgarage_command_garage_not_found"), Plugin.MsgColor, Plugin.Conf.AnnouncerIconUrl);
                    return false;
                case EResponseType.VEHICLE_NOT_OWNER:
                    player.SendChat(Plugin.Inst.Translate("virtualgarage_command_gadd_vehicle_not_owner"), Plugin.MsgColor, Plugin.Conf.AnnouncerIconUrl);
                    return false;
                case EResponseType.GARAGE_FULL:
                    garageModel = Plugin.SelectedGarageDict[player.CSteamID];
                    player.SendChat(Plugin.Inst.Translate("virtualgarage_command_gadd_garage_full", garageModel.Name, garageModel.Slot), Plugin.MsgColor, Plugin.Conf.AnnouncerIconUrl);
                    return false;
                case EResponseType.GARAGE_NO_PERMISSION:
                    garageModel = GarageModel.Parse(commands?[0]);
                    player.SendChat(Plugin.Inst.Translate("virtualgarage_command_garage_no_permission", garageModel.Name, garageModel.Permission), Plugin.MsgColor, Plugin.Conf.AnnouncerIconUrl);
                    return false;
                case EResponseType.BLACKLIST_VEHICLE:
                    var vehicleAsset = (VehicleAsset) Assets.find(EAssetType.VEHICLE, blacklistedID);
                    player.SendChat(Plugin.Inst.Translate("virtualgarage_command_gadd_blacklist_vehicle", vehicleAsset.vehicleName, vehicleAsset.id), Plugin.MsgColor, Plugin.Conf.AnnouncerIconUrl);
                    return false;
                case EResponseType.GARAGE_NOT_SELECTED:
                    player.SendChat(Plugin.Inst.Translate("virtualgarage_command_gadd_garage_not_selected"), Plugin.MsgColor, Plugin.Conf.AnnouncerIconUrl);
                    return false;
                case EResponseType.BLACKLIST_BARRICADE:
                    var barricadeAsset = (ItemBarricadeAsset) Assets.find(EAssetType.ITEM, blacklistedID);
                    player.SendChat(Plugin.Inst.Translate("virtualgarage_command_gadd_blacklist_barricade", barricadeAsset.itemName, barricadeAsset.id), Plugin.MsgColor, Plugin.Conf.AnnouncerIconUrl);
                    return false;
                case EResponseType.BLACKLIST_TRUNK_ITEM:
                    var itemAsset = (ItemAsset) Assets.find(EAssetType.ITEM, blacklistedID);
                    player.SendChat(
                        Plugin.Inst.Translate("virtualgarage_command_gadd_blacklist_trunk_item", itemAsset.itemName,
                            itemAsset.id), Plugin.MsgColor, Plugin.Conf.AnnouncerIconUrl);
                    return false;
                case EResponseType.SAME_NAME_AS_GARAGE:
                    player.SendChat(Plugin.Inst.Translate("virtualgarage_command_gadd_vehicle_name_same_as_garage"), Plugin.MsgColor, Plugin.Conf.AnnouncerIconUrl);
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