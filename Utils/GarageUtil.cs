using System;
using System.Collections.Generic;
using System.Linq;
using Rocket.Unturned.Player;
using SDG.Unturned;
using UnityEngine;
using VirtualGarage.Enums;
using VirtualGarage.Models;
using VirtualGarage.Serialization;
using Logger = Rocket.Core.Logging.Logger;

namespace VirtualGarage.Utils
{
    public static class GarageUtil
    {
        public static List<Garage> GetAllGarages(UnturnedPlayer player)
        {
            return Plugin.Conf.VirtualGarages.Where(garage => player.HasPermission(garage.Permission)).ToList();
        }
        public static Garage GetFirstGarage(UnturnedPlayer player)
        {
            return Plugin.Conf.VirtualGarages.FirstOrDefault(garage => player.HasPermission(garage.Permission));
        }
        
        private static bool BlacklistCheck(UnturnedPlayer player, InteractableVehicle vehicle, out EResponseType responseType, out ushort blacklistedID)
        {
            blacklistedID = 0;
            responseType = EResponseType.SUCCESS;
            
            if (Plugin.Conf.BlacklistedVehicles.Any(blacklist => blacklist.Assets.Any(asset =>
                vehicle.id == asset.ID && !player.HasPermission(blacklist.BypassPermission))))
            {
                responseType = EResponseType.BLACKLIST_VEHICLE;
                blacklistedID = vehicle.id;
                return false;
            }
            if (vehicle.trunkItems != null && (vehicle.trunkItems != null || vehicle.trunkItems.items.Count != 0))
                foreach (var item in Plugin.Conf.BlacklistedTrunkItems.SelectMany(blacklist => blacklist.Assets.SelectMany(asset => vehicle.trunkItems.items.Where(item => item.item.id == asset.ID && !player.HasPermission(blacklist.BypassPermission)))))
                {
                    responseType = EResponseType.BLACKLIST_TRUNK_ITEM;
                    blacklistedID = item.item.id;
                    return false;
                }
            if (BarricadeManager.tryGetPlant(vehicle.transform, out _, out _, out _, out var region) && region.barricades != null && region.barricades.Count != 0)
                foreach (var asset in Plugin.Conf.BlacklistedBarricades.SelectMany(blacklist => blacklist.Assets.Where(asset => region.drops.Any(drop =>
                    drop.asset.id == asset.ID && !player.HasPermission(blacklist.BypassPermission)))))
                {
                    responseType = EResponseType.BLACKLIST_BARRICADE;
                    blacklistedID = asset.ID;
                    return false;
                }

            return true;
        }
        public static bool GarageCheck(UnturnedPlayer player, Garage garage, out EResponseType responseType, bool isRetrieveOrList = false, bool isSuper = false)
        {
            if (garage == null)
            {
                responseType = EResponseType.GARAGE_NOT_FOUND;
                return false;
            }
            if (!isSuper && !player.HasPermission(garage.Permission))
            {
                responseType = EResponseType.GARAGE_NO_PERMISSION;
                return false;
            }
            if (!isRetrieveOrList && !isSuper)
                if (!Plugin.DbManager.IsGarageFull(player.CSteamID.m_SteamID.ToString(), garage))
                {
                    responseType = EResponseType.GARAGE_FULL;
                    return false;
                }

            responseType = EResponseType.SUCCESS;
            return true;
        }
        private static bool OwnerCheck(UnturnedPlayer player, InteractableVehicle vehicle)
        {
            var pass = player.CSteamID.m_SteamID == vehicle.lockedOwner.m_SteamID;

            return pass;
        }
        private static bool SelectedGarageCheck(UnturnedPlayer player)
        {
            if (!Plugin.SelectedGarageDict.ContainsKey(player.CSteamID))
                return false;
            
            return Plugin.SelectedGarageDict[player.CSteamID] != null;
        }
        private static bool VehicleCheck(UnturnedPlayer player, out InteractableVehicle vehicle, out BarricadeRegion vehicleRegion)
        {
            vehicle = null;
            vehicleRegion = null;
            if (!VehicleUtil.GetVehicleByLook(player, 2048f, out vehicle, out vehicleRegion))
                VehicleUtil.GetVehicleBySeat(player, out vehicle, out vehicleRegion);
            return vehicle != null;
        }
        
        public static void GarageAddChecks(UnturnedPlayer player, string[] commands, out InteractableVehicle vehicle, out BarricadeRegion vehicleRegion, out EResponseType responseType, out ushort blacklistedID)
        {
            responseType = EResponseType.SUCCESS;
            blacklistedID = 0;
            vehicle = null;
            vehicleRegion = null;
            var oneArg = commands.Length == 1;
            switch (oneArg)
            {
                case true:
                    if (!SelectedGarageCheck(player))
                    {
                        responseType = EResponseType.GARAGE_NOT_SELECTED;
                        return;
                    }
                    if (Plugin.Conf.VirtualGarages.Any(g =>
                        string.Equals(g.Name, commands[0], StringComparison.CurrentCultureIgnoreCase)))
                    {
                        responseType = EResponseType.SAME_NAME_AS_GARAGE;
                        return;
                    }
                    if (!VehicleCheck(player, out vehicle, out vehicleRegion))
                    {
                        responseType = EResponseType.VEHICLE_NOT_FOUND;
                        return;
                    }
                    if (!OwnerCheck(player, vehicle))
                    {
                        responseType = EResponseType.VEHICLE_NOT_OWNER;
                        return;
                    }
                    if (!GarageCheck(player, Plugin.SelectedGarageDict[player.CSteamID], out responseType))
                    {
                        return;
                    }
                    BlacklistCheck(player, vehicle, out responseType, out blacklistedID);
                    break;
                case false:
                    if (Plugin.Conf.VirtualGarages.Any(g =>
                        string.Equals(g.Name, commands[1], StringComparison.CurrentCultureIgnoreCase)))
                    {
                        responseType = EResponseType.SAME_NAME_AS_GARAGE;
                        return;
                    }
                    if (!VehicleCheck(player, out vehicle, out vehicleRegion))
                    {
                        responseType = EResponseType.VEHICLE_NOT_FOUND;
                        return;
                    }
                    if (!OwnerCheck(player, vehicle))
                    {
                        responseType = EResponseType.VEHICLE_NOT_OWNER;
                        return;
                    }
                    if (!GarageCheck(player, Garage.Parse(commands[0]), out responseType))
                    {
                        return;
                    }
                    BlacklistCheck(player, vehicle, out responseType, out blacklistedID);
                    break;
            }
        }
        public static void GarageAddAllChecks(UnturnedPlayer player, InteractableVehicle vehicle, out EResponseType responseType, out ushort blacklistedID)
        {
            responseType = EResponseType.SUCCESS;
            blacklistedID = 0;
            BlacklistCheck(player, vehicle, out responseType, out blacklistedID);
        }
        public static void GarageRetrieveChecks(UnturnedPlayer player, out EResponseType responseType, string[] commands)
        {
            responseType = EResponseType.SUCCESS;
            var oneArg = commands.Length == 1;
            switch (oneArg)
            {
                case true:
                    if (!SelectedGarageCheck(player))
                    {
                        responseType = EResponseType.GARAGE_NOT_SELECTED;
                        return;
                    }
                    if (!GarageCheck(player, Plugin.SelectedGarageDict[player.CSteamID], out responseType, true))
                    {
                        return;
                    }
                    if (!Plugin.DbManager.IsVehicleExist(player.CSteamID.m_SteamID.ToString(), Plugin.SelectedGarageDict[player.CSteamID].Name, commands[0]))
                    {
                        responseType = EResponseType.DONT_HAVE_VEHICLE;
                    }
                    break;
                case false:
                    if (!GarageCheck(player, Garage.Parse(commands[0]), out responseType, true))
                    {
                        return;
                    }
                    if (!Plugin.DbManager.IsVehicleExist(player.CSteamID.m_SteamID.ToString(), Garage.Parse(commands[0]).Name, commands[1]))
                    {
                        responseType = EResponseType.DONT_HAVE_VEHICLE;
                    }
                    break;
            }
        }
        public static void GarageRetrieveAllChecks(UnturnedPlayer player, out EResponseType responseType, string[] commands)
        {
            responseType = EResponseType.SUCCESS;
            if (!GarageCheck(player, Garage.Parse(commands[0]), out responseType, true))
            {
                return;
            }
            if (!Plugin.DbManager.IsVehicleExist(player.CSteamID.m_SteamID.ToString(), Garage.Parse(commands[0]).Name))
            {
                responseType = EResponseType.DONT_HAVE_VEHICLE;
            }
        }
        public static void SuperGarageAddChecks(UnturnedPlayer player, string[] commands, out InteractableVehicle vehicle, out BarricadeRegion vehicleRegion, out EResponseType responseType)
        {
            responseType = EResponseType.SUCCESS;
            vehicle = null;
            vehicleRegion = null;
            if (!ulong.TryParse(commands[0], out var steamID))
            {
                responseType = EResponseType.INVALID_ID;
                return;
            }
            if (Plugin.Conf.VirtualGarages.Any(g =>
                string.Equals(g.Name, commands[2], StringComparison.CurrentCultureIgnoreCase)))
            {
                responseType = EResponseType.SAME_NAME_AS_GARAGE;
                return;
            }
            if (!VehicleCheck(player, out vehicle, out vehicleRegion))
            {
                responseType = EResponseType.VEHICLE_NOT_FOUND;
                return;
            }
            if (!GarageCheck(player, Garage.Parse(commands[1]), out responseType, isSuper: true))
            {
            }
        }
        public static void SuperGarageRetrieveChecks(UnturnedPlayer player, out EResponseType responseType, string[] commands)
        {
            responseType = EResponseType.SUCCESS;
            
            if (!ulong.TryParse(commands[0], out var steamID))
            {
                responseType = EResponseType.INVALID_ID;
                return;
            }
            if (commands[1].ToLower() != "drown" && !GarageCheck(player, Garage.Parse(commands[1]), out responseType, isSuper: true))
            {
                return;
            }
            if (!Plugin.DbManager.IsVehicleExist(commands[0], commands[1].ToLower() != "drown" ? Garage.Parse(commands[1]).Name : "Drown", commands[2]))
            {
                responseType = EResponseType.DONT_HAVE_VEHICLE;
            }
        }
        public static void SuperGarageListCheck(UnturnedPlayer player, string steamID, Garage garage, out EResponseType responseType, bool noGarage = false)
        {
            responseType = EResponseType.SUCCESS;
            
            if (!ulong.TryParse(steamID, out var value))
            {
                responseType = EResponseType.INVALID_ID;
                return;
            }
            if (UnturnedPlayer.FromName(steamID) == null)
            {
                responseType = EResponseType.PLAYER_NOT_ONLINE;
                return;
            }
            if (!noGarage && !GarageCheck(player, garage, out responseType, isSuper: true))
            {
                return;
            }
        }
        
        public static void LoadVgVehicleFromSql(UnturnedPlayer player, string garageName, string vehicleName, out InteractableVehicle spawnedVehicle, string steamID = "")
        {
            spawnedVehicle = null;
            try
            {
                var playerVgVehicle = Plugin.DbManager.ReadVgVehicleByVehicleName(steamID == "" ? 
                    player.CSteamID.m_SteamID.ToString() : steamID, garageName, vehicleName);
                var vgVehicle = playerVgVehicle.Info.ToVgVehicle();
                
                //Vehicle spawned position, based on player
                var pTransform = player.Player.transform;
                var point = pTransform.position + pTransform.forward * 6f;
                point += Vector3.up * 12f;

                spawnedVehicle = vgVehicle.SpawnVehicle(player, point, pTransform.rotation);
                Plugin.DbManager.DeleteVgVehicle(playerVgVehicle.EntryID);
            }
            catch (Exception e)
            {
                Rocket.Core.Logging.Logger.LogError("[VirtualGarage] LoadError: " + e);
            }
        }
        public static void SaveVgVehicleToSql(ulong steamID, string garageName, string vehicleName, InteractableVehicle vehicle, BarricadeRegion vehicleRegion)
        {
            try
            {
                var info = VgVehicle.Create(vehicle).ToInfo();
                foreach (var currentPlayer in vehicle.passengers.Where(c => c.player != null))
                {
                    vehicle.forceRemovePlayer(out var seat, currentPlayer.player.playerID.steamID, out var point, out var angle);
                    VehicleManager.sendExitVehicle(vehicle, seat, point, angle, true);
                }
                if(BarricadeManager.tryGetPlant(vehicle.transform, out _, out _, out _, out var region))
                {
                    vehicle.trunkItems.items.Clear();
                    region.barricades.Clear();
                    region.drops.Clear();
                }
                Plugin.DbManager.InsertVgVehicle(steamID.ToString(), garageName, vehicleName, info);
                VehicleManager.askVehicleDestroy(vehicle);
            }
            catch (Exception e)
            {
                Logger.LogError("[VirtualGarage] SaveError: " + e);
            }
        }

        public static void SaveAllVehicle()
        {
            
        }
    }
}