using System;
using System.Threading.Tasks;
using RFGarage.DatabaseManagers;
using RFGarage.Enums;
using RFGarage.Utils;
using Rocket.API;
using Rocket.Unturned.Player;
using RocketExtensions.Models;
using RocketExtensions.Plugins;

namespace RFGarage.Commands
{
    [CommandActor(AllowedCaller.Player)]
#if RELEASEPUNCH
    [CommandAliases("vl")]
#else
    [CommandAliases("gg")]
#endif
    [CommandPermissions("garages")]
    [CommandInfo("Get a list of vehicle in garage.", "/garages")]
    public class GaragesCommand : RocketCommand
    {
        public override async Task Execute(CommandContext context)
        {
            if (context.CommandRawArguments.Length != 0)
            {
                await context.ReplyAsync(VehicleUtil.TranslateRich(EResponse.INVALID_PARAMETER.ToString(), Syntax),
                    RFGarage.Plugin.MsgColor, RFGarage.Plugin.Conf.MessageIconUrl);
                return;
            }

            var player = (UnturnedPlayer) context.Player;
            
            if (RFGarage.Plugin.Inst.IsProcessingGarage.TryGetValue(player.CSteamID.m_SteamID, out var lastProcessing) && lastProcessing.HasValue && (DateTime.Now - lastProcessing.Value).TotalSeconds <= 1)
            {
                await context.ReplyAsync(VehicleUtil.TranslateRich(EResponse.PROCESSING_GARAGE.ToString()),
                    RFGarage.Plugin.MsgColor, RFGarage.Plugin.Conf.MessageIconUrl);
                return;
            }

            RFGarage.Plugin.Inst.IsProcessingGarage[player.CSteamID.m_SteamID] = DateTime.Now;
            var playerGarages = await GarageManager.Get(player.CSteamID.m_SteamID);
            if (playerGarages == null || playerGarages.Count == 0)
            {
                await context.ReplyAsync(VehicleUtil.TranslateRich(EResponse.NO_VEHICLE.ToString()),
                    RFGarage.Plugin.MsgColor, RFGarage.Plugin.Conf.MessageIconUrl);
                await context.ReplyAsync(
                    VehicleUtil.TranslateRich(EResponse.GARAGE_SLOT.ToString(), 0, player.GetGarageSlot()),
                    RFGarage.Plugin.MsgColor, RFGarage.Plugin.Conf.MessageIconUrl);
                return;
            }

            var count = 0;
            foreach (var playerGarage in playerGarages)
            {
                await context.ReplyAsync(
                    VehicleUtil.TranslateRich(EResponse.GARAGE_LIST.ToString(), ++count, playerGarage.VehicleName,
                        playerGarage.GarageContent.Id, playerGarage.GarageContent.GetVehicleAsset().vehicleName),
                    RFGarage.Plugin.MsgColor, RFGarage.Plugin.Conf.MessageIconUrl);
            }

            await context.ReplyAsync(
                VehicleUtil.TranslateRich(EResponse.GARAGE_SLOT.ToString(), playerGarages.Count, player.GetGarageSlot()),
                RFGarage.Plugin.MsgColor, RFGarage.Plugin.Conf.MessageIconUrl);
        }
    }
}