using System.Threading.Tasks;
using RFGarage.Enums;
using RFGarage.Utils;
using RFRocketLibrary.Plugins;
using Rocket.Unturned.Player;

namespace RFGarage.Commands
{
    [AllowedCaller(Rocket.API.AllowedCaller.Player)]
    [CommandName("garages")]
    [Permissions("garages")]
    [CommandInfo("Get a list of vehicle in garage.", "/garages")]
    public class GaragesCommand : RocketCommand
    {
        public override async Task ExecuteAsync(CommandContext context)
        {
            if (context.CommandRawArguments.Length != 0)
            {
                await context.ReplyAsync(Plugin.Inst.Translate(EResponse.INVALID_PARAMETER.ToString(), Syntax),
                    Plugin.MsgColor, Plugin.Conf.AnnouncerIconUrl);
                return;
            }

            var player = (UnturnedPlayer) context.Player;
            var playerGarages = Plugin.Inst.Database.GarageManager.Get(player.CSteamID.m_SteamID);
            if (playerGarages == null || playerGarages.Count == 0)
            {
                await context.ReplyAsync(Plugin.Inst.Translate(EResponse.NO_VEHICLE.ToString()),
                    Plugin.MsgColor, Plugin.Conf.AnnouncerIconUrl);
                await context.ReplyAsync(
                    Plugin.Inst.Translate(EResponse.GARAGE_SLOT.ToString(), 0, player.GetGarageSlot()),
                    Plugin.MsgColor, Plugin.Conf.AnnouncerIconUrl);
                return;
            }

            var count = 0;
            foreach (var playerGarage in playerGarages)
            {
                await context.ReplyAsync(
                    Plugin.Inst.Translate(EResponse.GARAGE_LIST.ToString(), ++count, playerGarage.VehicleName,
                        playerGarage.GarageContent.Id, playerGarage.GarageContent.GetVehicleAsset().vehicleName),
                    Plugin.MsgColor, Plugin.Conf.AnnouncerIconUrl);
            }

            await context.ReplyAsync(
                Plugin.Inst.Translate(EResponse.GARAGE_SLOT.ToString(), playerGarages.Count, player.GetGarageSlot()),
                Plugin.MsgColor, Plugin.Conf.AnnouncerIconUrl);
        }
    }
}