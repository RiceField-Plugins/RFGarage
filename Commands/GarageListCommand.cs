using System;
using System.Collections.Generic;
using System.Linq;
using RFGarage.Enums;
using RFGarage.Models;
using RFGarage.Utils;
using Rocket.API;
using Rocket.Core.Logging;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;

namespace RFGarage.Commands
{
    public class GarageListCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;
        public string Name => "garagelist";
        public string Help => "Check your garage's contents.";
        public string Syntax => "/garagelist | /garagelist <garageName>";
        public List<string> Aliases => new List<string> {"glist", "vglist"};
        public List<string> Permissions => new List<string> {"garagelist"};
        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (command.Length > 1)
            {
                caller.SendChat(Plugin.Inst.Translate("rfgarage_command_invalid_parameter"), Plugin.MsgColor, Plugin.Conf.AnnouncerIconUrl);
                return;
            }

            var player = (UnturnedPlayer) caller;
            switch (command.Length)
            {
                case 0:
                {
                    var garages = GarageUtil.GetAllGarages(player);
                    var list = !garages.Any() ? "None" : string.Join(", ", (from t in garages let count = Plugin.DbManager.GetVehicleCount(player.CSteamID.m_SteamID.ToString(), t.Name) select $"{t.Name} ({count}/{t.Slot})").ToArray());
                    caller.SendChat(Plugin.Inst.Translate("rfgarage_command_glist_garages_success", list), Plugin.MsgColor, Plugin.Conf.AnnouncerIconUrl);
                    return;
                }
                case 1:
                {
                    if (!CheckResponse(player, GarageModel.Parse(command[0])))
                        return;
                    var garage = GarageModel.Parse(command[0]);
                    var vgVehicles = Plugin.DbManager.ReadVgVehicleByGarageName(player.CSteamID.m_SteamID.ToString(), garage.Name);
                    var playerVgVehicles = vgVehicles as PlayerSerializableVehicleModel[] ?? vgVehicles.ToArray();
                    foreach (var vgVehicle in playerVgVehicles)
                    {
                        var vg = vgVehicle.Info.ToVgVehicle();
                        var asset = (VehicleAsset) Assets.find(EAssetType.VEHICLE, vg.ID);
                        var list = $"[Name] {vgVehicle.VehicleName}, [VName] {asset.vehicleName}, [ID] {asset.id}";
                        caller.SendChat(Plugin.Inst.Translate("rfgarage_command_glist_garage_success", garage.Name, list), Plugin.MsgColor, Plugin.Conf.AnnouncerIconUrl);
                    }

                    return;
                }
                default:
                    caller.SendChat(Plugin.Inst.Translate("rfgarage_command_invalid_parameter"), Plugin.MsgColor, Plugin.Conf.AnnouncerIconUrl);
                    break;
            }
        }

        private static bool CheckResponse(UnturnedPlayer player, GarageModel garageModel)
        {
            GarageUtil.GarageCheck(player, garageModel, out var responseType, true);
            switch (responseType)
            {
                case EResponseType.GARAGE_NOT_FOUND:
                    player.SendChat(Plugin.Inst.Translate("rfgarage_command_garage_not_found"), Plugin.MsgColor, Plugin.Conf.AnnouncerIconUrl);
                    return false;
                case EResponseType.GARAGE_NO_PERMISSION:
                    player.SendChat(Plugin.Inst.Translate("rfgarage_command_garage_no_permission", garageModel.Name, garageModel.Permission), Plugin.MsgColor, Plugin.Conf.AnnouncerIconUrl);
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