using System;
using System.Collections.Generic;
using RFRocketLibrary;

namespace RFGarage.EventListeners
{
    public static class ServerEvent
    {
        public static void OnPostLevelLoaded(int level)
        {
            Plugin.Inst.IsProcessingGarage = new Dictionary<ulong, DateTime?>();
            Plugin.Inst.BusyVehicle = new HashSet<uint>();
#if RF
            Library.Initialize();
#endif
        }
    }
}