using System.Linq;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using UnityEngine;

namespace RFGarage.Utils
{
    public static class VehicleUtil
    {
        public static void ClearItems(InteractableVehicle vehicle)
        {
            if (!BarricadeManager.tryGetPlant(vehicle.transform, out _, out _, out _, out var region)) return;
            vehicle.trunkItems?.items?.Clear();
            region.barricades?.Clear();
            region.drops.Clear();
        }
        public static void ForceExitPassenger(InteractableVehicle vehicle)
        {
            foreach (var currentPlayer in vehicle.passengers.Where(c => c.player != null))
            {
                vehicle.forceRemovePlayer(out var seat, currentPlayer.player.playerID.steamID, out var point, out var angle);
                VehicleManager.sendExitVehicle(vehicle, seat, point, angle, true);
            }
        }
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
    }
}