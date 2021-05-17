using System;
using System.Collections.Generic;
using System.Linq;
using Rocket.API;
using Rocket.Core.Logging;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using VirtualGarage.Enums;
using VirtualGarage.Models;
using VirtualGarage.Utils;

namespace VirtualGarage.Commands
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
                UnturnedChat.Say(caller, Plugin.Inst.Translate("virtualgarage_command_invalid_parameter"), Plugin.MsgColor);
                return;
            }

            var player = (UnturnedPlayer) caller;
            switch (command.Length)
            {
                case 0:
                {
                    var garages = GarageUtil.GetAllGarages(player);
                    var list = !garages.Any() ? "None" : string.Join(", ", (from t in garages let count = Plugin.DbManager.GetVehicleCount(player.CSteamID.m_SteamID.ToString(), t.Name) select $"{t.Name} ({count}/{t.Slot})").ToArray());
                    UnturnedChat.Say(caller, Plugin.Inst.Translate("virtualgarage_command_glist_garages_success", list), Plugin.MsgColor);
                    return;
                }
                case 1:
                {
                    Logger.LogError("1");
                    if (!CheckResponse(player, Garage.Parse(command[0])))
                        return;
                    Logger.LogError("2");
                    var garage = Garage.Parse(command[0]);
                    Logger.LogError("3");
                    var vgVehicles = Plugin.DbManager.ReadVgVehicleByGarageName(player.CSteamID.m_SteamID.ToString(), garage.Name);
                    Logger.LogError("4");
                    var playerVgVehicles = vgVehicles as PlayerVgVehicle[] ?? vgVehicles.ToArray();
                    Logger.LogError("5");
                    foreach (var vgVehicle in playerVgVehicles)
                    {
                        var vg = vgVehicle.Info.ToVgVehicle();
                        var asset = (VehicleAsset) Assets.find(EAssetType.VEHICLE, vg.ID);
                        var list = $"[Name] {vgVehicle.VehicleName}, [VName] {asset.vehicleName}, [ID] {asset.id}";
                        UnturnedChat.Say(caller, Plugin.Inst.Translate("virtualgarage_command_glist_garage_success", garage.Name, list), Plugin.MsgColor);
                    }
                    Logger.LogError("6");

                    return;
                }
                default:
                    UnturnedChat.Say(caller, Plugin.Inst.Translate("virtualgarage_command_invalid_parameter"), Plugin.MsgColor);
                    break;
            }
        }

        private static bool CheckResponse(UnturnedPlayer player, Garage garage)
        {
            GarageUtil.GarageCheck(player, garage, out var responseType, true);
            switch (responseType)
            {
                case EResponseType.GARAGE_NOT_FOUND:
                    UnturnedChat.Say(player, Plugin.Inst.Translate("virtualgarage_command_garage_not_found"), Plugin.MsgColor);
                    return false;
                case EResponseType.GARAGE_NO_PERMISSION:
                    UnturnedChat.Say(player, Plugin.Inst.Translate("virtualgarage_command_garage_no_permission", garage.Name, garage.Permission), Plugin.MsgColor);
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