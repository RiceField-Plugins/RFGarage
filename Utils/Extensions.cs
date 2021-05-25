using System;
using System.Linq;
using RFGarage.Serialization;
using Rocket.API;
using Rocket.Core;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using UnityEngine;

namespace RFGarage.Utils
{
    public static class Extensions
    {
        public static string ToBase64(this byte[] byteArray)
        {
            return Convert.ToBase64String(byteArray);
        }
        public static byte[] ToByteArray(this string base64)
        {
            return Convert.FromBase64String(base64);
        }
        
        public static string ToInfo(this SerializableVehicle serializableVehicle)
        {
            var byteArray = serializableVehicle.Serialize();
            return byteArray.ToBase64();
        }
        public static SerializableVehicle ToVgVehicle(this string info)
        {
            var byteArray = info.ToByteArray();
            return byteArray.Deserialize<SerializableVehicle>();
        }
        
        public static bool CheckPermission(this UnturnedPlayer player, string permission)
        {
            return player.HasPermission(permission) || player.IsAdminOrAsterisk();
        }
        public static bool IsAdminOrAsterisk(this UnturnedPlayer player)
        {
            return player.HasPermission("*") || player.IsAdmin;
        }
        
        public static void SendChat(this UnturnedPlayer player, string text, Color color, string iconURL = null)
        {
            ChatManager.serverSendMessage(text, color, null, player.SteamPlayer(), EChatMode.SAY, iconURL, true);
        }
        public static void SendChat(this IRocketPlayer player, string text, Color color, string iconURL = null)
        {
            ChatManager.serverSendMessage(text, color, null, PlayerTool.getSteamPlayer(new CSteamID(ulong.Parse(player.Id))), EChatMode.SAY, iconURL, true);
        }
    }
}