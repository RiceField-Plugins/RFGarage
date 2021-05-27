using Rocket.API;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using UnityEngine;

namespace RFGarage.Helpers
{
    public static class ChatHelper
    {
        public static void Broadcast(string text, Color color, string iconURL = null)
        {
            ChatManager.serverSendMessage(text, color, null, null, EChatMode.GLOBAL, iconURL, true);
        }
        public static void Say(UnturnedPlayer player, string text, Color color, string iconURL = null)
        {
            ChatManager.serverSendMessage(text, color, null, player.SteamPlayer(), EChatMode.SAY, iconURL, true);
        }
        public static void Say(IRocketPlayer player, string text, Color color, string iconURL = null)
        {
            ChatManager.serverSendMessage(text, color, null, PlayerTool.getSteamPlayer(new CSteamID(ulong.Parse(player.Id))), EChatMode.SAY, iconURL, true);
        }
    }
}