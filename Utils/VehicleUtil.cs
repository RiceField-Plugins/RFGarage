using System.Linq;
using SDG.Unturned;
#pragma warning disable 618

namespace RFGarage.Utils
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

        internal static string TranslateRich(string s, params object[] objects)
        {
            return Plugin.Inst.Translate(s, objects).Replace("-=", "<").Replace("=-", ">");
        }
    }
}