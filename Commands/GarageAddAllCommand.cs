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
    public class GarageAddAllCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;
        public string Name => "garageaddall";
        public string Help => "Add all locked vehicle to your virtual garage.";
        public string Syntax => "/garageaddall <confirm>";
        public List<string> Aliases => new List<string> {"gaall"};
        public List<string> Permissions => new List<string> {"garageaddall"};
        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (command.Length > 1)
            {
                UnturnedChat.Say(caller, Plugin.Inst.Translate("virtualgarage_command_invalid_parameter", Syntax), Plugin.MsgColor);
                return;
            }
            
            var player = (UnturnedPlayer) caller;
            switch (command.Length)
            {
                case 0:
                    Plugin.GarageAddAllQueueDict[player.CSteamID] = true;
                
                    UnturnedChat.Say(caller, Plugin.Inst.Translate("virtualgarage_command_gadd_all_ask_confirm"), Plugin.MsgColor);
                    return;
                case 1:
                    switch (command[0].ToLower())
                    {
                        case "confirm" when !Plugin.GarageAddAllQueueDict[player.CSteamID]:
                            UnturnedChat.Say(caller, Plugin.Inst.Translate("virtualgarage_command_all_no_queue"), Plugin.MsgColor);
                            return;
                        case "confirm":
                        {
                            UnturnedChat.Say(caller, Plugin.Inst.Translate("virtualgarage_command_gadd_all_confirm"), Plugin.MsgColor);
                            // Tuple < availableGarage, availableSlot >
                            var passedVehicles = new List<InteractableVehicle>();
                            var blacklistedVehicles = new List<InteractableVehicle>();
                            var availableGarages = GarageUtil.GetAllGarages(player);
                            foreach (var interactableVehicle in VehicleManager.vehicles.Where(interactableVehicle => interactableVehicle.lockedOwner.m_SteamID == player.CSteamID.m_SteamID))
                            {
                                GarageUtil.GarageAddAllChecks(player, interactableVehicle, out var response, out _);
                                switch (response)
                                {
                                    case EResponseType.BLACKLIST_BARRICADE:
                                    case EResponseType.BLACKLIST_TRUNK_ITEM:
                                    case EResponseType.BLACKLIST_VEHICLE:
                                        blacklistedVehicles.Add(interactableVehicle);
                                        break;
                                    case EResponseType.SUCCESS:
                                        passedVehicles.Add(interactableVehicle);
                                        break;
                                    default:
                                        throw new ArgumentOutOfRangeException();
                                }
                            }

                            var tupleGarages = (from garage in availableGarages let garageOccupiedSlot = Plugin.DbManager.GetVehicleCount(player.CSteamID.m_SteamID.ToString(), garage.Name) where garage.Slot > garageOccupiedSlot select new Tuple<Garage, uint>(garage, garageOccupiedSlot)).ToList();
                            var vehicleIndex = 0;
                            var successVehicles = new List<InteractableVehicle>();
                            foreach (var (garage, occupiedSlot) in tupleGarages)
                            {
                                var i = 0;
                                while (i < (garage.Slot - occupiedSlot) && vehicleIndex < passedVehicles.Count)
                                {
                                    GarageUtil.SaveVgVehicleToSql(player.CSteamID.m_SteamID, garage.Name, 
                                        passedVehicles[vehicleIndex].asset.name, passedVehicles[vehicleIndex], 
                                        BarricadeManager.getRegionFromVehicle(passedVehicles[vehicleIndex]));
                                    successVehicles.Add(passedVehicles[vehicleIndex]);
                                    vehicleIndex++;
                                    i++;
                                }
                            }

                            UnturnedChat.Say(caller, Plugin.Inst.Translate("virtualgarage_command_gadd_all_success", 
                                vehicleIndex, passedVehicles.Count - vehicleIndex), Plugin.MsgColor);
                            foreach (var vehicle in successVehicles)
                            {
                                UnturnedChat.Say(caller, Plugin.Inst.Translate("virtualgarage_command_all_success_vehicle", 
                                    vehicle.asset.id, vehicle.asset.vehicleName), Plugin.MsgColor);
                            }
                            foreach (var vehicle in blacklistedVehicles)
                            {
                                UnturnedChat.Say(caller, Plugin.Inst.Translate("virtualgarage_command_gadd_all_blacklist_vehicle", 
                                    vehicle.asset.id, vehicle.asset.vehicleName), Plugin.MsgColor);
                            }
                            passedVehicles.RemoveRange(0, vehicleIndex);
                            foreach (var vehicle in passedVehicles)
                            {
                                UnturnedChat.Say(caller, Plugin.Inst.Translate("virtualgarage_command_all_fail_vehicle", 
                                    vehicle.asset.id, vehicle.asset.vehicleName), Plugin.MsgColor);
                            }
                            return;
                        }
                        case "abort":
                            Plugin.GarageAddAllQueueDict[player.CSteamID] = false;
                    
                            UnturnedChat.Say(caller, Plugin.Inst.Translate("virtualgarage_command_all_abort"), Plugin.MsgColor);
                            return;
                    }

                    break;
            }
        }
    }
}