#if RELEASEPUNCH

using System;
using System.Linq;
using RFGarage.DatabaseManagers;
using RFGarage.Enums;
using RFGarage.Models;
using RFGarage.Utils;
using RFRocketLibrary.Helpers;
using RFRocketLibrary.Models;
using RFRocketLibrary.Utils;
using Rocket.API;
using Rocket.Unturned.Player;
using RocketExtensions.Utilities;
using SDG.Unturned;
using RaycastInfo = RFRocketLibrary.Models.RaycastInfo;
using VehicleUtil = RFGarage.Utils.VehicleUtil;

namespace RFGarage.EventListeners
{
    internal static class PlayerEvent
    {
        internal static void OnPunched(Player player, EPlayerPunch punch)
        {
            var uPlayer = UnturnedPlayer.FromPlayer(player);
            var raycastInfo = RaycastInfo.FromPlayerLook(player, RayMasks.VEHICLE, 1.75f);
            if (raycastInfo.Vehicle == null)
                return;

            var cPlayer = player.GetComponent<PlayerComponent>();
            if (cPlayer.PunchCount == 0)
            {
                cPlayer.PunchCount++;
                cPlayer.ResetPunchCount();
                return;
            }

            cPlayer.PunchCount++;
            if (cPlayer.PunchCount == 3)
            {
                if (cPlayer.ResetPunchCor != null)
                {
                    Plugin.Inst.StopCoroutine(cPlayer.ResetPunchCor);
                    cPlayer.ResetPunchCor = null;
                }

                cPlayer.PunchCount = 0;

                if (Plugin.Inst.IsProcessingGarage.TryGetValue(player.channel.owner.playerID.steamID.m_SteamID, out var lastProcessing) && lastProcessing.HasValue && (DateTime.Now - lastProcessing.Value).TotalSeconds <= 1)
                {
                    ChatHelper.Say(player, VehicleUtil.TranslateRich(EResponse.PROCESSING_GARAGE.ToString()),
                        Plugin.MsgColor, Plugin.Conf.MessageIconUrl);
                    return;
                }
            
                Plugin.Inst.IsProcessingGarage[player.channel.owner.playerID.steamID.m_SteamID] = null;
                
                var vehicle = raycastInfo.Vehicle;
                if (vehicle.isDead || vehicle.isExploded)
                {
                    ChatHelper.Say(player, VehicleUtil.TranslateRich(EResponse.NO_VEHICLE_INPUT.ToString()),
                        Plugin.MsgColor, Plugin.Conf.MessageIconUrl);
                    return;
                }
                
                if (vehicle.lockedOwner.m_SteamID != uPlayer.CSteamID.m_SteamID)
                {
                    ChatHelper.Say(player, VehicleUtil.TranslateRich(EResponse.VEHICLE_NOT_OWNED.ToString()),
                        Plugin.MsgColor, Plugin.Conf.MessageIconUrl);
                    return;
                }

                var slot = uPlayer.GetGarageSlot();
                if (GarageManager.Count(uPlayer.CSteamID.m_SteamID) >= slot)
                {
                    ChatHelper.Say(player, VehicleUtil.TranslateRich(EResponse.GARAGE_FULL.ToString(), slot),
                        Plugin.MsgColor, Plugin.Conf.MessageIconUrl);
                    return;
                }

                if (!Plugin.Conf.AllowTrain && vehicle.asset.engine == EEngine.TRAIN)
                {
                    ChatHelper.Say(player, VehicleUtil.TranslateRich(EResponse.TRAIN_NOT_ALLOWED.ToString()),
                        Plugin.MsgColor, Plugin.Conf.MessageIconUrl);
                    return;
                }

                if (Plugin.Conf.Blacklists.Any(x =>
                        x.Type == EBlacklistType.VEHICLE && !uPlayer.HasPermission(x.BypassPermission) &&
                        x.IdList.Contains(vehicle.id)))
                {
                    ChatHelper.Say(player,
                        VehicleUtil.TranslateRich(EResponse.BLACKLIST_VEHICLE.ToString(), vehicle.asset.vehicleName,
                            vehicle.asset.id), Plugin.MsgColor, Plugin.Conf.MessageIconUrl);
                    return;
                }

                foreach (var blacklist in Plugin.Conf.Blacklists.Where(x => x.Type == EBlacklistType.BARRICADE))
                {
                    if (uPlayer.HasPermission(blacklist.BypassPermission))
                        continue;
                    
                    var region = BarricadeManager.getRegionFromVehicle(vehicle);
                    if (region == null)
                        continue;
                    
                    foreach (var drop in region.drops.Where(drop => blacklist.IdList.Contains(drop.asset.id)))
                    {
                        ChatHelper.Say(player,
                            VehicleUtil.TranslateRich(EResponse.BLACKLIST_BARRICADE.ToString(), drop.asset.itemName,
                                drop.asset.id), Plugin.MsgColor, Plugin.Conf.MessageIconUrl);
                        return;
                    }
                }

                foreach (var blacklist in Plugin.Conf.Blacklists.Where(x => x.Type == EBlacklistType.ITEM))
                {
                    if (uPlayer.HasPermission(blacklist.BypassPermission))
                        continue;
                    
                    var region = BarricadeManager.getRegionFromVehicle(vehicle);
                    if (region == null)
                        continue;
                    
                    foreach (var drop in region.drops)
                    {
                        if (drop.interactable is not InteractableStorage storage)
                            continue;
                        
                        foreach (var asset in from id in blacklist.IdList
                                 where storage.items.has(id) != null
                                 select AssetUtil.GetItemAsset(id))
                        {
                            ChatHelper.Say(player,
                                VehicleUtil.TranslateRich(EResponse.BLACKLIST_ITEM.ToString(),
                                    asset.itemName, asset.id), Plugin.MsgColor, Plugin.Conf.MessageIconUrl);
                            return;
                        }
                    }
                }

                if (vehicle.trunkItems != null && vehicle.trunkItems.getItemCount() != 0)
                {
                    foreach (var blacklist in Plugin.Conf.Blacklists.Where(x => x.Type == EBlacklistType.ITEM))
                    {
                        if (uPlayer.HasPermission(blacklist.BypassPermission))
                            continue;
                        
                        foreach (var asset in from itemJar in vehicle.trunkItems.items
                                 where blacklist.IdList.Contains(itemJar.item.id)
                                 select AssetUtil.GetItemAsset(itemJar.item.id))
                        {
                            ChatHelper.Say(player, VehicleUtil.TranslateRich(EResponse.BLACKLIST_ITEM.ToString(),
                                asset.itemName, asset.id), Plugin.MsgColor, Plugin.Conf.MessageIconUrl);
                            return;
                        }
                    }
                }

                Plugin.Inst.IsProcessingGarage[player.channel.owner.playerID.steamID.m_SteamID] = DateTime.Now;
                vehicle.forceRemoveAllPlayers();
                VehicleUtil.ClearItems(vehicle);
                VehicleManager.askVehicleDestroy(vehicle);
                var garageContent = VehicleWrapper.Create(vehicle);
                DatabaseManager.Queue.Enqueue(async () =>
                {
                    var vehicleName = vehicle.asset.vehicleName;
                    await GarageManager.AddAsync(new PlayerGarage
                    {
                        SteamId = uPlayer.CSteamID.m_SteamID,
                        VehicleName = vehicleName,
                        GarageContent = garageContent,
                        LastUpdated = DateTime.Now,
                    });
                    await ThreadTool.RunOnGameThreadAsync(() =>
                    {
                        ChatHelper.Say(player,
                            VehicleUtil.TranslateRich(EResponse.GARAGE_ADDED.ToString(), vehicleName),
                            Plugin.MsgColor, Plugin.Conf.MessageIconUrl);
                    });
                });
            }
        }
    }
}
#endif