using System.Collections.Generic;
using System.Linq;
using RFGarage.Enums;
using RFGarage.Models;
using RFGarage.Utils;
using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using UnityEngine;

namespace RFGarage.Commands
{
    public class GarageRetrieveAllCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;
        public string Name => "garageretrieveall";
        public string Help => "Retrieve your vehicle from the virtual garage.";
        public string Syntax => "/garageretrieveall <confirm> | /garageretrieveall <garageName>";
        public List<string> Aliases => new List<string> {"grall"};
        public List<string> Permissions => new List<string> {"garageretrieveall"};
        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (command.Length > 1)
            {
                UnturnedChat.Say(caller, Plugin.Inst.Translate("rfgarage_command_invalid_parameter", Syntax), Plugin.MsgColor);
                return;
            }

            var player = (UnturnedPlayer) caller;
            switch (command.Length)
            {
                case 0 when Plugin.GarageRetrieveAllQueueDict[player.CSteamID].Count() != 0:
                    UnturnedChat.Say(caller, Plugin.Inst.Translate("rfgarage_command_gr_all_queue_exists"), Plugin.MsgColor);
                    return;
                case 0:
                {
                    var vgVehicles = Plugin.DbManager.ReadVgVehicleAllWithoutDrown(player.CSteamID.m_SteamID.ToString());
                    Plugin.GarageRetrieveAllQueueDict[player.CSteamID] = vgVehicles;
                
                    UnturnedChat.Say(caller, Plugin.Inst.Translate("rfgarage_command_gr_all_ask_confirm"), Plugin.MsgColor);
                    return;
                }
                case 1:
                {
                    switch (command[0].ToLower())
                    {
                        case "confirm" when !Plugin.GarageRetrieveAllQueueDict[player.CSteamID].Any():
                            UnturnedChat.Say(caller, Plugin.Inst.Translate("rfgarage_command_all_no_queue"), Plugin.MsgColor);
                            return;
                        case "confirm":
                        {
                            UnturnedChat.Say(caller, Plugin.Inst.Translate("rfgarage_command_gr_all_confirm"), Plugin.MsgColor);
                            var successVehicles = new List<InteractableVehicle>();
                            var vehicleIndex = 0;
                            foreach (var playerVgVehicle in Plugin.GarageRetrieveAllQueueDict[player.CSteamID])
                            {
                                var vgVehicle = playerVgVehicle.Info.ToVgVehicle();
                
                                //Vehicle spawned position, based on player
                                var pTransform = player.Player.transform;
                                var point = pTransform.position + pTransform.forward * 6f;
                                point += Vector3.up * 12f;

                                var spawnedVehicle = vgVehicle.SpawnVehicle(player, point, pTransform.rotation);
                                if (spawnedVehicle == null) continue;
                                Plugin.DbManager.DeleteVgVehicle(playerVgVehicle.EntryID);
                                successVehicles.Add(spawnedVehicle);
                                vehicleIndex++;
                            }
                            UnturnedChat.Say(caller, Plugin.Inst.Translate("rfgarage_command_gr_all_success", 
                                successVehicles.Count, Plugin.GarageRetrieveAllQueueDict.Count - successVehicles.Count), Plugin.MsgColor);
                            foreach (var vehicle in successVehicles)
                            {
                                UnturnedChat.Say(caller, Plugin.Inst.Translate("rfgarage_command_all_success_vehicle", 
                                    vehicle.asset.id, vehicle.asset.vehicleName), Plugin.MsgColor);
                            }
                            successVehicles.RemoveRange(0, vehicleIndex);
                            foreach (var vehicle in successVehicles)
                            {
                                UnturnedChat.Say(caller, Plugin.Inst.Translate("rfgarage_command_all_fail_vehicle", 
                                    vehicle.asset.id, vehicle.asset.vehicleName), Plugin.MsgColor);
                            }
                            Plugin.GarageRetrieveAllQueueDict[player.CSteamID] = new List<PlayerSerializableVehicleModel>();
                            return;
                        }
                        case "abort":
                            Plugin.GarageRetrieveAllQueueDict[player.CSteamID] = new List<PlayerSerializableVehicleModel>();
                    
                            UnturnedChat.Say(caller, Plugin.Inst.Translate("rfgarage_command_all_abort"), Plugin.MsgColor);
                            return;
                    }

                    if (Plugin.GarageRetrieveAllQueueDict[player.CSteamID].Count() != 0)
                    {
                        UnturnedChat.Say(caller, Plugin.Inst.Translate("rfgarage_command_gr_all_queue_exists"), Plugin.MsgColor);
                        return;
                    }
                
                    if (!CheckResponse(player, command))
                        return;
                    var garage = GarageModel.Parse(command[0]);
                    var vgVehicles = Plugin.DbManager.ReadVgVehicleByGarageName(player.CSteamID.m_SteamID.ToString(), garage.Name);
                    Plugin.GarageRetrieveAllQueueDict[player.CSteamID] = vgVehicles;
                    UnturnedChat.Say(caller, Plugin.Inst.Translate("rfgarage_command_gr_all_ask_confirm"), Plugin.MsgColor);
                    return;
                }
            }
        }
        
        private static bool CheckResponse(UnturnedPlayer player, string[] commands)
        {
            GarageUtil.GarageRetrieveAllChecks(player, out var responseType, commands);
            GarageModel garageModel;
            switch (responseType)
            {
                case EResponseType.DONT_HAVE_VEHICLE:
                    garageModel = GarageModel.Parse(commands?[0]);
                    UnturnedChat.Say(player, Plugin.Inst.Translate("rfgarage_command_garage_no_vehicle", garageModel.Name), Plugin.MsgColor);
                    return false;
                case EResponseType.GARAGE_NOT_FOUND:
                    UnturnedChat.Say(player, Plugin.Inst.Translate("rfgarage_command_garage_not_found"), Plugin.MsgColor);
                    return false;
                case EResponseType.GARAGE_NO_PERMISSION:
                    garageModel = GarageModel.Parse(commands?[0]);
                    UnturnedChat.Say(player, Plugin.Inst.Translate("rfgarage_command_garage_no_permission", garageModel.Name, garageModel.Permission), Plugin.MsgColor);
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