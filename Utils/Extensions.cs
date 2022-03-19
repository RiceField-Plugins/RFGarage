using System;
using System.Linq;
using Rocket.API;
using Rocket.Core.Logging;
using Rocket.Unturned.Player;

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


        internal static int GetGarageSlot(this UnturnedPlayer player)
        {
            return GetGarageSlot((IRocketPlayer) player);
        }
        
        internal static int GetGarageSlot(this IRocketPlayer player)
        {
            var permissions = player.GetPermissions().Select(a => a.Name).Where(p =>
                p.ToLower().StartsWith($"{Plugin.Conf.GarageSlotPermissionPrefix}."));
            var enumerable = permissions as string[] ?? permissions.ToArray();
            if (enumerable.Length == 0)
                return Plugin.Conf.DefaultGarageSlot;

            var slot = 0;
            foreach (var s in enumerable)
            {
                var split = s.Split('.');
                if (split.Length != 2)
                {
                    Logger.LogError($"[{Plugin.Inst.Name}] Error: GarageSlotPermissionPrefix must not contain '.'");
                    Logger.LogError($"[{Plugin.Inst.Name}] Invalid permission format: {s}");
                    Logger.LogError($"[{Plugin.Inst.Name}] Correct format: 'permPrefix'.'slot'");
                    continue;
                }

                byte.TryParse(split[1], out var result);
                if (result > slot)
                    slot = result;
            }

            return slot;
        }
    }
}