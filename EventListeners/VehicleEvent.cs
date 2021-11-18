using System;
using System.Threading.Tasks;
using RFGarageClassic.Enums;
using RFGarageClassic.Models;
using RFGarageClassic.Utils;
using RFRocketLibrary.Helpers;
using RFRocketLibrary.Models;
using Rocket.API;
using SDG.Unturned;
using Steamworks;
using VehicleUtil = RFGarageClassic.Utils.VehicleUtil;

namespace RFGarageClassic.EventListeners
{
    public static class VehicleEvent
    {
        public static void OnExploded(InteractableVehicle vehicle)
        {
            if (!Plugin.Conf.AutoClearDestroyedVehicles)
                return;

            VehicleManager.askVehicleDestroy(vehicle);
        }

        public static void OnPreVehicleDestroyed(InteractableVehicle vehicle)
        {
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
                ChatHelper.Say(rPlayer, Plugin.Inst.Translate(EResponse.VEHICLE_DROWN.ToString(), vehicle.asset.vehicleName));
        }
    }
}