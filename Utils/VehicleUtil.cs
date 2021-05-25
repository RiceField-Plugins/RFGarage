using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using UnityEngine;

namespace RFGarage.Utils
{
    public static class VehicleUtil
    {
        public static bool GetVehicleByLook(UnturnedPlayer player, float distance, out InteractableVehicle lookVehicle, out BarricadeRegion lookVehicleBarricadeRegion)
        {
            lookVehicle = null;
            lookVehicleBarricadeRegion = null;
            var raycastInfo = DamageTool.raycast(new Ray(player.Player.look.aim.position, player.Player.look.aim.forward), distance, RayMasks.VEHICLE);
            if (raycastInfo.vehicle == null) 
                return false;
            
            lookVehicle = raycastInfo.vehicle;
            lookVehicleBarricadeRegion = BarricadeManager.getRegionFromVehicle(lookVehicle);
            return true;
        }
        public static bool GetVehicleBySeat(UnturnedPlayer player, out InteractableVehicle currentVehicle, out BarricadeRegion vehicleBarricadeRegion)
        {
            currentVehicle = player.CurrentVehicle;
            vehicleBarricadeRegion = null;
            if (currentVehicle == null)
                return false;
            
            vehicleBarricadeRegion = BarricadeManager.getRegionFromVehicle(currentVehicle);
            return true;
        }
        public static bool VehicleHasOwner(InteractableVehicle vehicle)
        {
            return vehicle.lockedOwner.m_SteamID != 0 || vehicle.lockedOwner != CSteamID.Nil;
        }
        public static bool VehicleHasPassenger(InteractableVehicle vehicle)
        {
            return vehicle.passengers.Length != 0;
        }
    }
}