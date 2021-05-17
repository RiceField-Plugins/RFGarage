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
    public class SuperGarageListCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;
        public string Name => "supergaragelist";
        public string Help => "Check other player garage's contents.";
        public string Syntax => "/supergaragelist <steamID> | /supergaragelist <steamID> <garageName>";
        public List<string> Aliases => new List<string> {"sglist"};
        public List<string> Permissions => new List<string> {"supergaragelist"};
        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (command.Length < 1 || command.Length > 2)
            {
                UnturnedChat.Say(caller, Plugin.Inst.Translate("virtualgarage_command_invalid_parameter"), Plugin.MsgColor);
                return;
            }

            var player = (UnturnedPlayer) caller;
            switch (command.Length)
            {
                case 1 when !CheckResponse(player, command[0], null):
                    return;
                case 1:
                {
                    var target = UnturnedPlayer.FromName(command[0]);
                    var garages = GarageUtil.GetAllGarages(target);
                    var list = string.Join(", ", (from t in garages let count = Plugin.DbManager.GetVehicleCount(command[0], t.Name) select $"{t.Name} ({count}/{t.Slot})").ToArray());
                    if (!garages.Any())
                        list = "None";
                    UnturnedChat.Say(caller, Plugin.Inst.Translate("virtualgarage_command_glist_garages_success", list), Plugin.MsgColor);
                    return;
                }
                case 2 when !CheckResponse(player, command[0], Garage.Parse(command[1])):
                    return;
                case 2:
                {
                    var garage = Garage.Parse(command[1]);
                    var vgVehicles = Plugin.DbManager.ReadVgVehicleByGarageName(command[0], garage.Name);
                    var playerVgVehicles = vgVehicles as PlayerVgVehicle[] ?? vgVehicles.ToArray();
                    foreach (var vgVehicle in playerVgVehicles)
                    {
                        var vg = vgVehicle.Info.ToVgVehicle();
                        var asset = (VehicleAsset) Assets.find(EAssetType.VEHICLE, vg.ID);
                        var list = $"[Name] {vgVehicle.VehicleName}, [VName] {asset.vehicleName}, [ID] {asset.id}";
                        UnturnedChat.Say(caller, Plugin.Inst.Translate("virtualgarage_command_glist_garage_success", garage.Name, list), Plugin.MsgColor);
                    }

                    break;
                }
            }
        }

        private static bool CheckResponse(UnturnedPlayer player, string steamID, Garage garage)
        {
            GarageUtil.SuperGarageListCheck(player, steamID, garage, out var responseType, garage == null);
            switch (responseType)
            {
                case EResponseType.GARAGE_NOT_FOUND:
                    UnturnedChat.Say(player, Plugin.Inst.Translate("virtualgarage_command_garage_not_found"), Plugin.MsgColor);
                    return false;
                case EResponseType.INVALID_STEAMID:
                    UnturnedChat.Say(player, Plugin.Inst.Translate("virtualgarage_command_invalid_id"), Plugin.MsgColor);
                    return false;
                case EResponseType.PLAYER_NOT_ONLINE:
                    UnturnedChat.Say(player, Plugin.Inst.Translate("virtualgarage_command_player_not_online"), Plugin.MsgColor);
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