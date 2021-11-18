using System;
using System.Linq;
using Rocket.API;
using Rocket.Unturned.Player;

namespace RFGarageClassic.Utils
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

        public static int GetGarageSlot(this UnturnedPlayer player)
        {
            var slot = player.GetPermissions()?.FirstOrDefault(p =>
                p.Name.ToLower().StartsWith($"{Plugin.Conf.GarageSlotPermissionPrefix}."))?.Name?.Split('.').LastOrDefault();
            return slot == null ? Plugin.Conf.DefaultGarageSlot : Convert.ToInt32(slot);
        }

        public static int GetGarageSlot(this RocketPlayer player)
        {
            var slot = player.GetPermissions()?.FirstOrDefault(p =>
                p.Name.ToLower().StartsWith($"{Plugin.Conf.GarageSlotPermissionPrefix}."))?.Name?.Split('.').LastOrDefault();
            return slot == null ? Plugin.Conf.DefaultGarageSlot : Convert.ToInt32(slot);
        }
    }
}