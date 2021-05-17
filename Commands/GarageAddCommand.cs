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
                    if (!CheckResponse(player, command, out var vehicle, out var vehicleRegion))
                        return;
                    var garage = Plugin.SelectedGarageDict[player.CSteamID];
                    GarageUtil.SaveVgVehicleToSql(player.CSteamID.m_SteamID, garage.Name, command[0], vehicle, vehicleRegion);
                    UnturnedChat.Say(caller, Plugin.Inst.Translate("virtualgarage_command_gadd_success", vehicle.asset.vehicleName, vehicle.asset.id, garage.Name), Plugin.MsgColor);
                    return;
                }
                case 1:
                    UnturnedChat.Say(caller, Plugin.Inst.Translate("virtualgarage_command_invalid_parameter"), Plugin.MsgColor);
                    return;
                case 2:
                {
                    if (!CheckResponse(player, command, out var vehicle, out var vehicleRegion))
                        return;
                    var garage = Garage.Parse(command[0]);
                    GarageUtil.SaveVgVehicleToSql(player.CSteamID.m_SteamID, garage.Name, command[1], vehicle, vehicleRegion);
                    UnturnedChat.Say(caller, Plugin.Inst.Translate("virtualgarage_command_gadd_success", vehicle.asset.vehicleName, vehicle.asset.id, garage.Name), Plugin.MsgColor);
                    return;
                }
                default:
                    UnturnedChat.Say(caller, Plugin.Inst.Translate("virtualgarage_command_invalid_parameter"), Plugin.MsgColor);
                    break;
            }
        }

        private static bool CheckResponse(UnturnedPlayer player, string[] commands, out InteractableVehicle vehicle, out BarricadeRegion vehicleRegion)
        {
            GarageUtil.GarageAddChecks(player, commands, out vehicle, out vehicleRegion, out var responseType,
                out var blacklistedID);
            Garage garage;
            switch (responseType)
            {
                case EResponseType.VEHICLE_NOT_FOUND:
                    UnturnedChat.Say(player, Plugin.Inst.Translate("virtualgarage_command_vehicle_not_found"), Plugin.MsgColor);
                    return false;
                case EResponseType.GARAGE_NOT_FOUND:
                    UnturnedChat.Say(player, Plugin.Inst.Translate("virtualgarage_command_garage_not_found"), Plugin.MsgColor);
                    return false;
                case EResponseType.VEHICLE_NOT_OWNER:
                    UnturnedChat.Say(player, Plugin.Inst.Translate("virtualgarage_command_gadd_vehicle_not_owner"), Plugin.MsgColor);
                    return false;
                case EResponseType.GARAGE_FULL:
                    garage = Plugin.SelectedGarageDict[player.CSteamID];
                    UnturnedChat.Say(player, Plugin.Inst.Translate("virtualgarage_command_gadd_garage_full", garage.Name, garage.Slot), Plugin.MsgColor);
                    return false;
                case EResponseType.GARAGE_NO_PERMISSION:
                    garage = Garage.Parse(commands?[0]);
                    UnturnedChat.Say(player, Plugin.Inst.Translate("virtualgarage_command_garage_no_permission", garage.Name, garage.Permission), Plugin.MsgColor);
                    return false;
                case EResponseType.BLACKLIST_VEHICLE:
                    var vehicleAsset = (VehicleAsset) Assets.find(EAssetType.VEHICLE, blacklistedID);
                    UnturnedChat.Say(player, Plugin.Inst.Translate("virtualgarage_command_gadd_blacklist_vehicle", vehicleAsset.vehicleName, vehicleAsset.id), Plugin.MsgColor);
                    return false;
                case EResponseType.GARAGE_NOT_SELECTED:
                    UnturnedChat.Say(player, Plugin.Inst.Translate("virtualgarage_command_gadd_garage_not_selected"), Plugin.MsgColor);
                    return false;
                case EResponseType.BLACKLIST_BARRICADE:
                    var barricadeAsset = (ItemBarricadeAsset) Assets.find(EAssetType.ITEM, blacklistedID);
                    UnturnedChat.Say(player, Plugin.Inst.Translate("virtualgarage_command_gadd_blacklist_barricade", barricadeAsset.itemName, barricadeAsset.id), Plugin.MsgColor);
                    return false;
                case EResponseType.BLACKLIST_TRUNK_ITEM:
                    var itemAsset = (ItemAsset) Assets.find(EAssetType.ITEM, blacklistedID);
                    UnturnedChat.Say(player, Plugin.Inst.Translate("virtualgarage_command_gadd_blacklist_trunk_item", itemAsset.itemName, itemAsset.id), Plugin.MsgColor);
                    return false;
                case EResponseType.SAME_NAME_AS_GARAGE:
                    UnturnedChat.Say(player, Plugin.Inst.Translate("virtualgarage_command_gadd_vehicle_name_same_as_garage"), Plugin.MsgColor);
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