using System;
using System.Collections.Generic;
using System.Linq;
using Rocket.Unturned.Player;
using SDG.Unturned;
using UnityEngine;
using RFGarage.Utils;

namespace RFGarage.Serialization
{
    // Big thanks to AdamAdam
    [Serializable]
    public class SerializableVehicle
    {
        public ushort ID { get; set; }
        public ushort Health { get; set; }
        public ushort Fuel { get; set; }
        public ushort BatteryCharge { get; set; }
        public bool[] Tires { get; set; }
        public List<byte[]> Turrets { get; set; }
        public List<SerializableItem> TrunkItems { get; set; } = new List<SerializableItem>();
        public List<SerializableBarricade> Barricades { get; set; } = new List<SerializableBarricade>();

        public SerializableVehicle()
        {
            
        }
        public static SerializableVehicle Create(InteractableVehicle vehicle)
        {
            var vehicleTurret = new List<byte[]>();
            if (vehicle.turrets != null && vehicle.turrets?.Length != 0)
            {
                byte index = 0;
                while (index < vehicle.turrets.Length)
                {
                    vehicleTurret.Add(vehicle.turrets[index].state);
                    index++;
                }
            }
            
            var result = new SerializableVehicle
            {
                Health = vehicle.health,
                Fuel = vehicle.fuel,
                BatteryCharge = vehicle.batteryCharge,
                ID = vehicle.id,
                TrunkItems =
                    vehicle.trunkItems?.items?.Select(c => SerializableItem.Create(vehicle.trunkItems.page, c)).ToList() ??
                    new List<SerializableItem>(),
                Tires = vehicle.tires?.Select(c => c.isAlive)?.ToArray() ?? new bool[0],
                Turrets = vehicleTurret
            };
            
            if (BarricadeManager.tryGetPlant(vehicle.transform, out _, out _, out _, out var region))
            {
                foreach (var barricade in from data in region.barricades where !data.barricade.isDead let drop = region.drops.FirstOrDefault(c => c.instanceID == data.instanceID) select SerializableBarricade.Create(drop, data))
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
            if (vehicle.turrets != null && Turrets != null && Turrets.Count == vehicle.turrets.Length)
            {
                byte index = 0;
                while (index < vehicle.turrets.Length)
                {
                    vehicle.turrets[index].state = Turrets[index];
                    index += 1;
                }
            }
            else
            {
                byte index = 0;
                while (index < vehicle.turrets?.Length)
                {
                    var vehicleAsset = (VehicleAsset)Assets.find(EAssetType.VEHICLE, ID);
                    var itemAsset = (ItemAsset)Assets.find(EAssetType.ITEM, vehicleAsset.turrets[index].itemID);
                    if (itemAsset != null)
                    {
                        vehicle.turrets[index].state = itemAsset.getState();
                    }
                    else
                    {
                        vehicle.turrets[index].state = null;
                    }
                    index += 1;
                }
            }
            
            return vehicle;
        }
    }
}