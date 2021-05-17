using System.Collections.Generic;
using Rocket.Unturned.Player;
using VirtualGarage.Models;
using VirtualGarage.Utils;

namespace VirtualGarage.EventListeners
{
    public static class PlayerEvent
    {
        public static void OnConnected(UnturnedPlayer player)
        {
            if (!Plugin.SelectedGarageDict.ContainsKey(player.CSteamID))
                Plugin.SelectedGarageDict.Add(player.CSteamID, GarageUtil.GetFirstGarage(player));
            
            if (!Plugin.GarageAddAllQueueDict.ContainsKey(player.CSteamID))
                Plugin.GarageAddAllQueueDict.Add(player.CSteamID, false);
            
            if (!Plugin.GarageRetrieveAllQueueDict.ContainsKey(player.CSteamID))
                Plugin.GarageRetrieveAllQueueDict.Add(player.CSteamID, new List<PlayerVgVehicle>());
        }
        public static void OnDisconnected(UnturnedPlayer player)
        {
            if (Plugin.GarageAddAllQueueDict.ContainsKey(player.CSteamID))
                Plugin.GarageAddAllQueueDict[player.CSteamID] = false;

            if (!Plugin.GarageRetrieveAllQueueDict.ContainsKey(player.CSteamID))
                Plugin.GarageRetrieveAllQueueDict[player.CSteamID] = new List<PlayerVgVehicle>();
        }
    }
}