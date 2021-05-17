using System;
using System.Collections.Generic;
using System.Linq;
using Rocket.Unturned.Player;
using SDG.Unturned;
using UnityEngine;
using VirtualGarage.Utils;

namespace VirtualGarage.Serialization
{
    // Big thanks to AdamAdam
    [Serializable]
    public class VgVehicle
    {
        public ushort ID { get; set; }
        public ushort Health { get; set; }
        public ushort Fuel { get; set; }
        public ushort BatteryCharge { get; set; }
        public bool[] Tires { get; set; }
        public byte[][] Turrets { get; set; }
        public List<VgItem> TrunkItems { get; set; } = new List<VgItem>();
        public List<VgBarricade> Barricades { get; set; } = new List<VgBarricade>();

        public VgVehicle()
        {
            
        }
        public static VgVehicle Create(InteractableVehicle vehicle)
        {
            var vehicleTurret = new byte[][] { };
            for (byte index = 0; (int) index < vehicle.turrets.Length; ++index)
                vehicleTurret[index] = vehicle.turrets[index].state;
            
            var result = new VgVehicle
            {
                Health = vehicle.health,
                Fuel = vehicle.fuel,
                BatteryCharge = vehicle.batteryCharge,
                ID = vehicle.id,
                TrunkItems =
                    vehicle.trunkItems?.items?.Select(c => VgItem.Create(vehicle.trunkItems.page, c)).ToList() ??
                    new List<VgItem>(),
                Tires = vehicle.tires?.Select(c => c.isAlive)?.ToArray() ?? new bool[0],
                Turrets = vehicleTurret
            };
            
            if (BarricadeManager.tryGetPlant(vehicle.transform, out _, out _, out _, out var region))
            {
                foreach (var barricade in from data in region.barricades where !data.barricade.isDead let drop = region.drops.FirstOrDefault(c => c.instanceID == data.instanceID) select VgBarricade.Create(drop, data))
                {
                    result.Barricades.Add(barricade);
                }
            }

            return result;
        }
        public InteractableVehicle SpawnVehicle(UnturnedPlayer player, Vector3 position, Quaternion rotation)
        {
            // Spawn Vehicle
            var vehicle = VehicleManager.spawnLockedVehicleForPlayerV2(ID, position, rotation, player.Player);

            // Set Tires
            for (var i = 0; i < (Tires?.Length ?? 0); i++)
            {
                if (Tires != null) vehicle.tires[i].isAlive = Tires[i];
            }
            vehicle.sendTireAliveMaskUpdate();
            
            // Spawn Trunk Items
            if (vehicle.trunkItems != null)
            {
                foreach (var item in TrunkItems)
                {
                    if (item.Position == null) continue;
                    var itemPos = item.Position.Value;

                    vehicle.trunkItems.addItem(itemPos.X, itemPos.Y, itemPos.Rot, item.ToItem());
                }
            }
            
            // Spawn Barricades
            foreach(var barricade in Barricades)
            {
                barricade.SpawnBarricade(vehicle.transform);
            }
            
            // Set Turrets
            if (Turrets != null && Turrets.Length == vehicle.turrets.Length)
            {
                byte b = 0;
                while (b < vehicle.turrets.Length)
                {
                    vehicle.turrets[b].state = Turrets[b];
                    b += 1;
                }
            }
            else
            {
                byte b2 = 0;
                while (b2 < vehicle.turrets.Length)
                {
                    var vehicleAsset = (VehicleAsset)Assets.find(EAssetType.VEHICLE, ID);
                    var itemAsset = (ItemAsset)Assets.find(EAssetType.ITEM, vehicleAsset.turrets[b2].itemID);
                    if (itemAsset != null)
                    {
                        vehicle.turrets[b2].state = itemAsset.getState();
                    }
                    else
                    {
                        vehicle.turrets[b2].state = null;
                    }
                    b2 += 1;
                }
            }
            
            return vehicle;
        }

        public string ToInfo()
        {
            var byteArray = this.Serialize();
            return byteArray.ToBase64();
        }
    }
}