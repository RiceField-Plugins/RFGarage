using System;
using System.Threading.Tasks;
using RFGarage.Enums;
using RFGarage.Models;
using RFGarage.Utils;
using RFRocketLibrary.Helpers;
using RFRocketLibrary.Models;
using Rocket.API;
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

        internal static void OnPreVehicleDestroyed(InteractableVehicle vehicle)
        {
            if (vehicle == null)
                return;
            
            if (!vehicle.isDrowned || vehicle.lockedOwner == CSteamID.Nil || vehicle.lockedOwner.m_SteamID == 0)
                return;
            
            var rPlayer = new RocketPlayer(vehicle.lockedOwner.ToString());
            if (!rPlayer.HasPermission(Plugin.Conf.AutoAddOnDrownPermission) ||
                Plugin.Inst.Database.GarageManager.Count(vehicle.lockedOwner.m_SteamID) >=
                rPlayer.GetGarageSlot()) 
                return;
            
            var task = Task.Run(async () => await Plugin.Inst.Database.GarageManager.AddAsync(new PlayerGarage
            {
                VehicleName = vehicle.asset.vehicleName,
                SteamId = vehicle.lockedOwner.m_SteamID,
                GarageContent = VehicleWrapper.Create(vehicle),
                LastUpdated = DateTime.Now
            }));
            task.Wait();
            VehicleUtil.ClearItems(vehicle);
            if (PlayerTool.getPlayer(vehicle.lockedOwner) != null)
                ChatHelper.Say(rPlayer, VehicleUtil.TranslateRich(EResponse.VEHICLE_DROWN.ToString(), vehicle.asset.vehicleName),
                    Plugin.MsgColor, Plugin.Conf.MessageIconUrl);
        }
    }
}