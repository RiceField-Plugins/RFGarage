using System;
using System.Threading.Tasks;
using RFGarage.Enums;
using RFGarage.Utils;
using RFRocketLibrary.Plugins;
using Rocket.Unturned.Player;

namespace RFGarage.Commands
{
    [AllowedCaller(Rocket.API.AllowedCaller.Player)]
    [CommandName("garages")]
    [Aliases("gg")]
    [Permissions("garages")]
    [CommandInfo("Get a list of vehicle in garage.", "/garages")]
    public class GaragesCommand : RocketCommand
    {
        public override async Task ExecuteAsync(CommandContext context)
        {
            if (context.CommandRawArguments.Length != 0)
            {
                await context.ReplyAsync(VehicleUtil.TranslateRich(EResponse.INVALID_PARAMETER.ToString(), Syntax),
                    Plugin.MsgColor, Plugin.Conf.MessageIconUrl);
                return;
            }

            var player = (UnturnedPlayer) context.Player;
            
            if (Plugin.Inst.IsProcessingGarage.TryGetValue(player.CSteamID.m_SteamID, out var lastProcessing) && lastProcessing.HasValue && (DateTime.Now - lastProcessing.Value).TotalSeconds <= 1)
            {
                await context.ReplyAsync(VehicleUtil.TranslateRich(EResponse.PROCESSING_GARAGE.ToString()),
                    Plugin.MsgColor, Plugin.Conf.MessageIconUrl);
                return;
            }

            Plugin.Inst.IsProcessingGarage[player.CSteamID.m_SteamID] = DateTime.Now;
            var playerGarages = Plugin.Inst.Database.GarageManager.Get(player.CSteamID.m_SteamID);
            if (playerGarages == null || playerGarages.Count == 0)
            {
                await context.ReplyAsync(VehicleUtil.TranslateRich(EResponse.NO_VEHICLE.ToString()),
                    Plugin.MsgColor, Plugin.Conf.MessageIconUrl);
                await context.ReplyAsync(
                    VehicleUtil.TranslateRich(EResponse.GARAGE_SLOT.ToString(), 0, player.GetGarageSlot()),
                    Plugin.MsgColor, Plugin.Conf.MessageIconUrl);
                return;
            }

            var count = 0;
            foreach (var playerGarage in playerGarages)
            {
                await context.ReplyAsync(
                    VehicleUtil.TranslateRich(EResponse.GARAGE_LIST.ToString(), ++count, playerGarage.VehicleName,
                        playerGarage.GarageContent.Id, playerGarage.GarageContent.GetVehicleAsset().vehicleName),
                    Plugin.MsgColor, Plugin.Conf.MessageIconUrl);
            }

            await context.ReplyAsync(
                VehicleUtil.TranslateRich(EResponse.GARAGE_SLOT.ToString(), playerGarages.Count, player.GetGarageSlot()),
                Plugin.MsgColor, Plugin.Conf.MessageIconUrl);
        }
    }
}