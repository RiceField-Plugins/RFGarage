using System;
using System.Linq;
using System.Threading.Tasks;
using RFGarage.Enums;
using RFGarage.Models;
using RFGarage.Utils;
using RFRocketLibrary.Models;
using RFRocketLibrary.Plugins;
using RFRocketLibrary.Utils;
using Rocket.API;
using Rocket.Unturned.Player;
using SDG.Unturned;
using UnityEngine;
using AllowedCaller = RFRocketLibrary.Plugins.AllowedCaller;
using VehicleUtil = RFGarage.Utils.VehicleUtil;

namespace RFGarage.Commands
{
    [AllowedCaller(Rocket.API.AllowedCaller.Player)]
    [RFRocketLibrary.Plugins.CommandName("garageadd")]
    [Permissions("garageadd")]
    [Aliases("gadd", "ga")]
    [CommandInfo("Store vehicle to garage.", "/garageadd [vehicleName]")]
    public class GarageAddCommand : RocketCommand
    {
        public override async Task ExecuteAsync(CommandContext context)
        {
            var player = (UnturnedPlayer) context.Player;
            var vehicle = player.CurrentVehicle;
            if (vehicle == null)
            {
                if (!player.GetRaycastHit(Mathf.Infinity, RayMasks.VEHICLE, out var hit) ||
                    hit.transform == null || hit.transform.GetComponent<InteractableVehicle>() == null)
                {
                    await context.ReplyAsync(Plugin.Inst.Translate(EResponse.NO_VEHICLE_INPUT.ToString()),
                        Plugin.MsgColor, Plugin.Conf.AnnouncerIconUrl);
                    return;
                }

                vehicle = hit.transform.GetComponent<InteractableVehicle>();
            }

            if (vehicle.lockedOwner.m_SteamID != player.CSteamID.m_SteamID)
            {
                await context.ReplyAsync(Plugin.Inst.Translate(EResponse.VEHICLE_NOT_OWNED.ToString()),
                    Plugin.MsgColor, Plugin.Conf.AnnouncerIconUrl);
                return;
            }

            var slot = player.GetGarageSlot();
            if (Plugin.Inst.Database.GarageManager.Count(player.CSteamID.m_SteamID) >= slot)
            {
                await context.ReplyAsync(Plugin.Inst.Translate(EResponse.GARAGE_FULL.ToString(), slot),
                    Plugin.MsgColor, Plugin.Conf.AnnouncerIconUrl);
                return;
            }

            if (!Plugin.Conf.AllowTrain && vehicle.asset.engine == EEngine.TRAIN)
            {
                await context.ReplyAsync(Plugin.Inst.Translate(EResponse.TRAIN_NOT_ALLOWED.ToString()),
                    Plugin.MsgColor, Plugin.Conf.AnnouncerIconUrl);
                return;
            }

            if (Plugin.Conf.Blacklists.Any(x =>
                x.Type == EBlacklistType.VEHICLE && !player.HasPermission(x.BypassPermission) &&
                x.IdList.Contains(vehicle.id)))
            {
                await context.ReplyAsync(
                    Plugin.Inst.Translate(EResponse.BLACKLIST_VEHICLE.ToString(), vehicle.asset.vehicleName,
                        vehicle.asset.id), Plugin.MsgColor, Plugin.Conf.AnnouncerIconUrl);
                return;
            }

            foreach (var blacklist in Plugin.Conf.Blacklists.Where(x => x.Type == EBlacklistType.BARRICADE))
            {
                if (player.HasPermission(blacklist.BypassPermission))
                    continue;
                var region = BarricadeManager.getRegionFromVehicle(vehicle);
                if (region == null)
                    continue;
                foreach (var drop in region.drops.Where(drop => blacklist.IdList.Contains(drop.asset.id)))
                {
                    await context.ReplyAsync(
                        Plugin.Inst.Translate(EResponse.BLACKLIST_BARRICADE.ToString(), drop.asset.itemName,
                            drop.asset.id),
                        Plugin.MsgColor, Plugin.Conf.AnnouncerIconUrl);
                    return;
                }
            }

            foreach (var blacklist in Plugin.Conf.Blacklists.Where(x => x.Type == EBlacklistType.ITEM))
            {
                if (player.HasPermission(blacklist.BypassPermission))
                    continue;
                var region = BarricadeManager.getRegionFromVehicle(vehicle);
                if (region == null)
                    continue;
                foreach (var drop in region.drops)
                {
                    if (!(drop.interactable is InteractableStorage storage))
                        continue;
                    foreach (var asset in from id in blacklist.IdList
                        where storage.items.has(id) != null
                        select AssetUtil.GetItemAsset(id))
                    {
                        await context.ReplyAsync(
                            Plugin.Inst.Translate(EResponse.BLACKLIST_ITEM.ToString(),
                                asset.itemName, asset.id),
                            Plugin.MsgColor, Plugin.Conf.AnnouncerIconUrl);
                        return;
                    }
                }
            }

            if (vehicle.trunkItems != null && vehicle.trunkItems.getItemCount() != 0)
            {
                foreach (var blacklist in Plugin.Conf.Blacklists.Where(x => x.Type == EBlacklistType.ITEM))
                {
                    if (player.HasPermission(blacklist.BypassPermission))
                        continue;
                    foreach (var asset in from itemJar in vehicle.trunkItems.items
                        where blacklist.IdList.Contains(itemJar.item.id)
                        select AssetUtil.GetItemAsset(itemJar.item.id))
                    {
                        await context.ReplyAsync(Plugin.Inst.Translate(EResponse.BLACKLIST_ITEM.ToString(),
                            asset.itemName, asset.id), Plugin.MsgColor, Plugin.Conf.AnnouncerIconUrl);
                        return;
                    }
                }
            }

            vehicle.forceRemoveAllPlayers();
            string vehicleName;
            if (context.CommandRawArguments.Length == 0)
            {
                vehicleName = vehicle.asset.vehicleName;
                await Plugin.Inst.Database.GarageManager.AddAsync(new PlayerGarage
                {
                    SteamId = player.CSteamID.m_SteamID,
                    VehicleName = vehicleName,
                    GarageContent = VehicleWrapper.Create(vehicle),
                    LastUpdated = DateTime.Now,
                });
                VehicleUtil.ClearItems(vehicle);
                VehicleManager.askVehicleDestroy(vehicle);
                await context.ReplyAsync(Plugin.Inst.Translate(EResponse.GARAGE_ADDED.ToString(), vehicleName),
                    Plugin.MsgColor, Plugin.Conf.AnnouncerIconUrl);
                return;
            }

            vehicleName = string.Join(" ", context.CommandRawArguments);
            await Plugin.Inst.Database.GarageManager.AddAsync(new PlayerGarage
            {
                SteamId = player.CSteamID.m_SteamID,
                VehicleName = vehicleName,
                GarageContent = VehicleWrapper.Create(vehicle),
                LastUpdated = DateTime.Now,
            });
            VehicleUtil.ClearItems(vehicle);
            VehicleManager.askVehicleDestroy(vehicle);
            await context.ReplyAsync(Plugin.Inst.Translate(EResponse.GARAGE_ADDED.ToString(), vehicleName),
                Plugin.MsgColor, Plugin.Conf.AnnouncerIconUrl);
        }
    }
}