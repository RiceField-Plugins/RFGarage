using System;
using System.Linq;
using System.Threading.Tasks;
using RFGarage.DatabaseManagers;
using RFGarage.Enums;
using RFGarage.Models;
using RFGarage.Utils;
using RFRocketLibrary.Models;
using RFRocketLibrary.Utils;
using Rocket.API;
using Rocket.Unturned.Player;
using RocketExtensions.Models;
using RocketExtensions.Plugins;
using RocketExtensions.Utilities;
using SDG.Unturned;
using RaycastInfo = RFRocketLibrary.Models.RaycastInfo;
using VehicleUtil = RFGarage.Utils.VehicleUtil;

namespace RFGarage.Commands
{
    [CommandActor(AllowedCaller.Player)]
    [CommandPermissions("garageadd")]
#if RELEASEPUNCH
    [CommandAliases("vc")]
#else
    [CommandAliases("gadd", "ga")]
#endif
    [CommandInfo("Store vehicle to garage.", "/garageadd [vehicleName]", AllowSimultaneousCalls = false)]
    public class GarageAddCommand : RocketCommand
    {
        public override async Task Execute(CommandContext context)
        {
            var player = (UnturnedPlayer) context.Player;
            var vehicle = player.CurrentVehicle;

            if (RFGarage.Plugin.Inst.IsProcessingGarage.TryGetValue(player.CSteamID.m_SteamID,
                    out var lastProcessing) && lastProcessing.HasValue &&
                (DateTime.Now - lastProcessing.Value).TotalSeconds <= 1)
            {
                await context.ReplyAsync(VehicleUtil.TranslateRich(EResponse.PROCESSING_GARAGE.ToString()),
                    RFGarage.Plugin.MsgColor, RFGarage.Plugin.Conf.MessageIconUrl);
                return;
            }

            RFGarage.Plugin.Inst.IsProcessingGarage[player.CSteamID.m_SteamID] = null;
            if (vehicle == null)
            {
                var raycastInfo = await ThreadTool.RunOnGameThreadAsync(() =>
                    RaycastInfo.FromPlayerLook(player.Player, RayMasks.VEHICLE));
                if (raycastInfo?.Vehicle == null)
                {
                    await context.ReplyAsync(VehicleUtil.TranslateRich(EResponse.NO_VEHICLE_INPUT.ToString()),
                        RFGarage.Plugin.MsgColor, RFGarage.Plugin.Conf.MessageIconUrl);
                    return;
                }

                vehicle = raycastInfo.Vehicle;
            }

            if (vehicle.isDead || vehicle.isExploded)
            {
                await context.ReplyAsync(VehicleUtil.TranslateRich(EResponse.NO_VEHICLE_INPUT.ToString()),
                    RFGarage.Plugin.MsgColor, RFGarage.Plugin.Conf.MessageIconUrl);
                return;
            }

            if (vehicle.lockedOwner.m_SteamID != player.CSteamID.m_SteamID)
            {
                await context.ReplyAsync(VehicleUtil.TranslateRich(EResponse.VEHICLE_NOT_OWNED.ToString()),
                    RFGarage.Plugin.MsgColor, RFGarage.Plugin.Conf.MessageIconUrl);
                return;
            }

            var slot = player.GetGarageSlot();
            if (GarageManager.Count(player.CSteamID.m_SteamID) >= slot)
            {
                await context.ReplyAsync(VehicleUtil.TranslateRich(EResponse.GARAGE_FULL.ToString(), slot),
                    RFGarage.Plugin.MsgColor, RFGarage.Plugin.Conf.MessageIconUrl);
                return;
            }

            if (!RFGarage.Plugin.Conf.AllowTrain && vehicle.asset.engine == EEngine.TRAIN)
            {
                await context.ReplyAsync(VehicleUtil.TranslateRich(EResponse.TRAIN_NOT_ALLOWED.ToString()),
                    RFGarage.Plugin.MsgColor, RFGarage.Plugin.Conf.MessageIconUrl);
                return;
            }

            if (RFGarage.Plugin.Conf.Blacklists.Any(x =>
                    x.Type == EBlacklistType.VEHICLE && !player.HasPermission(x.BypassPermission) &&
                    x.IdList.Contains(vehicle.id)))
            {
                await context.ReplyAsync(
                    VehicleUtil.TranslateRich(EResponse.BLACKLIST_VEHICLE.ToString(), vehicle.asset.vehicleName,
                        vehicle.asset.id), RFGarage.Plugin.MsgColor, RFGarage.Plugin.Conf.MessageIconUrl);
                return;
            }

            foreach (var blacklist in RFGarage.Plugin.Conf.Blacklists.Where(x => x.Type == EBlacklistType.BARRICADE))
            {
                if (player.HasPermission(blacklist.BypassPermission))
                    continue;

                var region = BarricadeManager.getRegionFromVehicle(vehicle);
                if (region == null)
                    continue;

                foreach (var drop in region.drops.Where(drop => blacklist.IdList.Contains(drop.asset.id)))
                {
                    await context.ReplyAsync(
                        VehicleUtil.TranslateRich(EResponse.BLACKLIST_BARRICADE.ToString(), drop.asset.itemName,
                            drop.asset.id), RFGarage.Plugin.MsgColor, RFGarage.Plugin.Conf.MessageIconUrl);
                    return;
                }
            }

            foreach (var blacklist in RFGarage.Plugin.Conf.Blacklists.Where(x => x.Type == EBlacklistType.ITEM))
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
                            VehicleUtil.TranslateRich(EResponse.BLACKLIST_ITEM.ToString(),
                                asset.itemName, asset.id), RFGarage.Plugin.MsgColor,
                            RFGarage.Plugin.Conf.MessageIconUrl);
                        return;
                    }
                }
            }

            if (vehicle.trunkItems != null && vehicle.trunkItems.getItemCount() != 0)
            {
                foreach (var blacklist in RFGarage.Plugin.Conf.Blacklists.Where(x => x.Type == EBlacklistType.ITEM))
                {
                    if (player.HasPermission(blacklist.BypassPermission))
                        continue;

                    foreach (var asset in from itemJar in vehicle.trunkItems.items
                             where blacklist.IdList.Contains(itemJar.item.id)
                             select AssetUtil.GetItemAsset(itemJar.item.id))
                    {
                        await context.ReplyAsync(VehicleUtil.TranslateRich(EResponse.BLACKLIST_ITEM.ToString(),
                            asset.itemName, asset.id), RFGarage.Plugin.MsgColor, RFGarage.Plugin.Conf.MessageIconUrl);
                        return;
                    }
                }
            }

            await ThreadTool.RunOnGameThreadAsync(() => { vehicle.forceRemoveAllPlayers(); });
            RFGarage.Plugin.Inst.IsProcessingGarage[player.CSteamID.m_SteamID] = DateTime.Now;
            RFGarage.Plugin.Inst.BusyVehicle.Add(vehicle.instanceID);
            var vehicleName = context.CommandRawArguments.Length == 0 ? vehicle.asset.vehicleName : string.Join(" ", context.CommandRawArguments);
            var garageContent = VehicleWrapper.Create(vehicle);
            await ThreadTool.RunOnGameThreadAsync(() =>
            {
                VehicleUtil.ClearTrunkAndBarricades(vehicle);
                VehicleManager.askVehicleDestroy(vehicle);
            });
            await DatabaseManager.Queue.Enqueue(async () =>
                await GarageManager.AddAsync(new PlayerGarage
                {
                    SteamId = player.CSteamID.m_SteamID,
                    VehicleName = vehicleName,
                    GarageContent = garageContent,
                    LastUpdated = DateTime.Now,
                })
            )!;
            RFGarage.Plugin.Inst.BusyVehicle.Remove(vehicle.instanceID);
            await context.ReplyAsync(VehicleUtil.TranslateRich(EResponse.GARAGE_ADDED.ToString(), vehicleName),
                RFGarage.Plugin.MsgColor, RFGarage.Plugin.Conf.MessageIconUrl);
            return;
        }
    }
}