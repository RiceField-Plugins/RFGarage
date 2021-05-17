using SDG.Unturned;

namespace VirtualGarage.EventListeners
{
    public static class VehicleEvent
    {
        public static void OnExploded(InteractableVehicle vehicle)
        {
            if (!Plugin.Conf.AutoClearDestroyedVehicles)
                return;
            
            VehicleManager.askVehicleDestroy(vehicle);
        }
    }
}