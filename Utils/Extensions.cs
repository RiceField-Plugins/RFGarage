using System;
using System.Linq;
using Rocket.Core;
using Rocket.Unturned.Player;
using VirtualGarage.Serialization;

namespace VirtualGarage.Utils
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
        
        public static string ToInfo(this VgVehicle vgVehicle)
        {
            var byteArray = vgVehicle.Serialize();
            return byteArray.ToBase64();
        }
        public static VgVehicle ToVgVehicle(this string info)
        {
            var byteArray = info.ToByteArray();
            return byteArray.Deserialize<VgVehicle>();
        }
        
        public static bool HasPermission(this UnturnedPlayer player, string permission)
        {
            return R.Permissions.GetPermissions(player).Any(p =>
                string.Equals(p.Name, permission, StringComparison.CurrentCultureIgnoreCase) || player.IsAdminOrAsterisk());
        }
        public static bool IsAdminOrAsterisk(this UnturnedPlayer player)
        {
            return R.Permissions.GetPermissions(player).Any(p =>
                string.Equals(p.Name, "*", StringComparison.CurrentCultureIgnoreCase) || player.IsAdmin);
        }
    }
}