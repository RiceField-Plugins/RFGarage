using System.Linq;
using SDG.Unturned;
#pragma warning disable 618

namespace RFGarageClassic.Utils
{
    internal static class VehicleUtil
    {
        internal static void ClearItemsAndBarricades(InteractableVehicle vehicle)
        {
            var region = BarricadeManager.findRegionFromVehicle(vehicle);
            if (region == null)
                return;
            foreach (var drop in region.drops)
                if (drop.interactable is InteractableStorage storage)
                    storage.items.clear();
            region.barricades.Clear();
        }
        internal static void ClearItems(InteractableVehicle vehicle)
        {
            var region = BarricadeManager.findRegionFromVehicle(vehicle);
            if (region == null)
                return;
            foreach (var drop in region.drops)
                if (drop.interactable is InteractableStorage storage)
                    storage.items.clear();
        }
        // internal static void ForceExitPassenger(InteractableVehicle vehicle)
        // {
        //     foreach (var currentPlayer in vehicle.passengers.Where(c => c.player != null))
        //     {
        //         vehicle.forceRemovePlayer(out var seat, currentPlayer.player.playerID.steamID, out var point, out var angle);
        //         VehicleManager.sendExitVehicle(vehicle, seat, point, angle, true);
        //     }
        // }
    }
}