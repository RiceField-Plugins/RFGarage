using System;
using System.Linq;
using System.Threading.Tasks;
using RFGarage.DatabaseManagers;
using RFGarage.Enums;
using RFGarage.Models;
using RFGarage.Utils;
using RFRocketLibrary.Helpers;
using RFRocketLibrary.Models;
using RFRocketLibrary.Utils;
using Rocket.API;
using Rocket.Core.Logging;
using SDG.Unturned;
using Steamworks;
using VehicleUtil = RFGarage.Utils.VehicleUtil;

namespace RFGarage.EventListeners
{
    internal static class VehicleEvent
    {
        internal static void OnExploded(InteractableVehicle vehicle)
        {
            if (!Plugin.Conf.AutoClearDestroyedVehicles)
                return;

            VehicleManager.askVehicleDestroy(vehicle);
        }

        internal static void OnPreVehicleDestroyed(InteractableVehicle vehicle, ref bool shouldallow)
        {
            if (!shouldallow)
                return;
            
            if (!vehicle.isDrowned || vehicle.lockedOwner == CSteamID.Nil || vehicle.lockedOwner.m_SteamID == 0)
                return;
            
            if (Plugin.Inst.BusyVehicle.Contains(vehicle.instanceID))
                return;
            
            var rPlayer = new RocketPlayer(vehicle.lockedOwner.ToString());
            if (!rPlayer.HasPermission(Plugin.Conf.AutoAddOnDrownPermission) ||
                GarageManager.Count(vehicle.lockedOwner.m_SteamID) >= rPlayer.GetGarageSlot()) 
                return;
            
            if (Plugin.Conf.Blacklists.Any(x =>
                    x.Type == EBlacklistType.VEHICLE && !rPlayer.HasPermission(x.BypassPermission) &&
                    x.IdList.Contains(vehicle.id)))
            {
                ChatHelper.Say(rPlayer, VehicleUtil.TranslateRich(EResponse.BLACKLIST_VEHICLE.ToString(), vehicle.asset.vehicleName,
                    vehicle.asset.id), Plugin.MsgColor, Plugin.Conf.MessageIconUrl);
                return;
            }
            
            foreach (var blacklist in Plugin.Conf.Blacklists.Where(x => x.Type == EBlacklistType.BARRICADE))
            {
                if (rPlayer.HasPermission(blacklist.BypassPermission))
                    continue;

                var region = BarricadeManager.getRegionFromVehicle(vehicle);
                if (region == null)
                    continue;

                foreach (var drop in region.drops.Where(drop => blacklist.IdList.Contains(drop.asset.id)))
                {
                    ChatHelper.Say(rPlayer, 
                        VehicleUtil.TranslateRich(EResponse.BLACKLIST_BARRICADE.ToString(), drop.asset.itemName,
                            drop.asset.id), Plugin.MsgColor, Plugin.Conf.MessageIconUrl);
                    return;
                }
            }

            foreach (var blacklist in Plugin.Conf.Blacklists.Where(x => x.Type == EBlacklistType.ITEM))
            {
                if (rPlayer.HasPermission(blacklist.BypassPermission))
                    continue;

                var region = BarricadeManager.getRegionFromVehicle(vehicle);
                if (region == null)
                    continue;

                foreach (var drop in region.drops)
                {
                    if (!(drop.interactable is InteractableStorage storage))
                        continue;
                    foreach (var asset in from id in blacklist.IdList
                             where storage.items.has(id) != null
                             select AssetUtil.GetItemAsset(id))
                    {
                        ChatHelper.Say(rPlayer, 
                            VehicleUtil.TranslateRich(EResponse.BLACKLIST_ITEM.ToString(),
                                asset.itemName, asset.id), Plugin.MsgColor,
                            Plugin.Conf.MessageIconUrl);
                        return;
                    }
                }
            }

            if (vehicle.trunkItems != null && vehicle.trunkItems.getItemCount() != 0)
            {
                foreach (var blacklist in Plugin.Conf.Blacklists.Where(x => x.Type == EBlacklistType.ITEM))
                {
                    if (rPlayer.HasPermission(blacklist.BypassPermission))
                        continue;

                    foreach (var asset in from itemJar in vehicle.trunkItems.items
                             where blacklist.IdList.Contains(itemJar.item.id)
                             select AssetUtil.GetItemAsset(itemJar.item.id))
                    {
                        ChatHelper.Say(rPlayer, VehicleUtil.TranslateRich(EResponse.BLACKLIST_ITEM.ToString(),
                            asset.itemName, asset.id), Plugin.MsgColor, Plugin.Conf.MessageIconUrl);
                        return;
                    }
                }
            }

            var garageContent = VehicleWrapper.Create(vehicle);
            VehicleUtil.ClearTrunkAndBarricades(vehicle);
            DatabaseManager.Queue.Enqueue(async () => await GarageManager.AddAsync(new PlayerGarage
            {
                VehicleName = vehicle.asset.vehicleName,
                SteamId = vehicle.lockedOwner.m_SteamID,
                GarageContent = garageContent,
                LastUpdated = DateTime.Now
            }));
            if (PlayerTool.getPlayer(vehicle.lockedOwner) != null)
                ChatHelper.Say(rPlayer, VehicleUtil.TranslateRich(EResponse.VEHICLE_DROWN.ToString(), vehicle.asset.vehicleName),
                    Plugin.MsgColor, Plugin.Conf.MessageIconUrl);
        }
    }
}