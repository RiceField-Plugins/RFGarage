using System;
using System.Collections.Generic;

namespace RFGarage.EventListeners
{
    public static class ServerEvent
    {
        public static void OnPostLeveLoaded(int level)
        {
            Plugin.Inst.IsProcessingGarage = new Dictionary<ulong, DateTime?>();
        }
    }
}