using System.Linq;
using SDG.Unturned;
#pragma warning disable 618

namespace RFGarage.Utils
{
    internal static class VehicleUtil
    {
        internal static void ClearTrunkAndBarricades(InteractableVehicle vehicle)
        {
            vehicle.trunkItems?.clear();

            var region = BarricadeManager.findRegionFromVehicle(vehicle);
            if (region == null)
                return;
            
            foreach (var drop in region.drops.ToList())
            {
                if (drop.interactable is InteractableStorage storage)
                    storage.items.clear();

                BarricadeManager.tryGetPlant(vehicle.transform, out var x, out var y, out var plant, out _);
                BarricadeManager.destroyBarricade(drop, x, y, plant);
            }
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

        internal static string TranslateRich(string s, params object[] objects)
        {
            return Plugin.Inst.Translate(s, objects).Replace("-=", "<").Replace("=-", ">");
        }
    }
}