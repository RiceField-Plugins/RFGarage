using System;
using System.Threading.Tasks;
using RFGarage.Enums;
using RFRocketLibrary.Plugins;
using RFRocketLibrary.Utils;
using Rocket.Unturned.Player;
using UnityEngine;
using VehicleUtil = RFGarage.Utils.VehicleUtil;

namespace RFGarage.Commands
{
    [AllowedCaller(Rocket.API.AllowedCaller.Player)]
    [CommandName("garageretrieve")]
    [Permissions("garageretrieve")]
    [Aliases("gret", "gr")]
    [CommandInfo("Retrieve vehicle from garage.", "/garageretrieve <vehicleName>")]
    public class GarageRetrieveCommand : RocketCommand
    {
        public override async Task ExecuteAsync(CommandContext context)
        {
            if (context.CommandRawArguments.Length == 0)
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

            Plugin.Inst.IsProcessingGarage[player.CSteamID.m_SteamID] = null;
            
            var vehicleName = string.Join(" ", context.CommandRawArguments);
            var playerGarage = Plugin.Inst.Database.GarageManager.Get(player.CSteamID.m_SteamID, vehicleName);
            if (playerGarage == null)
            {
                await context.ReplyAsync(VehicleUtil.TranslateRich(EResponse.VEHICLE_NOT_FOUND.ToString(), vehicleName),
                    Plugin.MsgColor, Plugin.Conf.MessageIconUrl);
                return;
            }

            Plugin.Inst.IsProcessingGarage[player.CSteamID.m_SteamID] = DateTime.Now;
            await Plugin.Inst.Database.GarageManager.DeleteAsync(playerGarage.Id);

            //Vehicle spawned position, based on player
            var pTransform = player.Player.transform;
            var point = pTransform.position + pTransform.forward * 6f;
            point += Vector3.up * 12f;
            await ThreadUtil.RunOnGameThreadAsync(() =>
                playerGarage.GarageContent.SpawnVehicle(player, point, pTransform.rotation, true));
            await context.ReplyAsync(
                VehicleUtil.TranslateRich(EResponse.GARAGE_RETRIEVE.ToString(),
                    playerGarage.GarageContent.GetVehicleAsset().vehicleName),
                Plugin.MsgColor, Plugin.Conf.MessageIconUrl);
        }
    }
}