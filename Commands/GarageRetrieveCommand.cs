using System.Threading.Tasks;
using RFGarageClassic.Enums;
using RFRocketLibrary.Plugins;
using RFRocketLibrary.Utils;
using Rocket.Unturned.Player;
using UnityEngine;

namespace RFGarageClassic.Commands
{
    [AllowedCaller(Rocket.API.AllowedCaller.Player)]
    [RFRocketLibrary.Plugins.CommandName("garageretrieve")]
    [Permissions("garageretrieve")]
    [Aliases("gret", "gr")]
    [CommandInfo("Retrieve vehicle from garage.", "/garageretrieve <vehicleName>")]
    public class GarageRetrieveCommand : RocketCommand
    {
        public override async Task ExecuteAsync(CommandContext context)
        {
            if (context.CommandRawArguments.Length == 0)
            {
                await context.ReplyAsync(Plugin.Inst.Translate(EResponse.INVALID_PARAMETER.ToString(), Syntax));
                return;
            }
            
            var player = (UnturnedPlayer) context.Player;
            var vehicleName = string.Join(" ", context.CommandRawArguments);
            var playerGarage = Plugin.Inst.Database.GarageManager.Get(player.CSteamID.m_SteamID, vehicleName);
            if (playerGarage == null)
            {
                await context.ReplyAsync(Plugin.Inst.Translate(EResponse.VEHICLE_NOT_FOUND.ToString(), vehicleName));
                return;
            }
            
            await Plugin.Inst.Database.GarageManager.DeleteAsync(playerGarage.Id);
            
            //Vehicle spawned position, based on player
            var pTransform = player.Player.transform;
            var point = pTransform.position + pTransform.forward * 6f;
            point += Vector3.up * 12f;
            await ThreadUtil.RunOnGameThreadAsync(() => playerGarage.GarageContent.SpawnVehicle(player, point, pTransform.rotation, true));
            await context.ReplyAsync(Plugin.Inst.Translate(EResponse.GARAGE_RETRIEVE.ToString(), vehicleName),
                Plugin.MsgColor, Plugin.Conf.AnnouncerIconUrl);
        }
    }
}