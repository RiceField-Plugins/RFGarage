using System;
using SDG.Unturned;
using UnityEngine;

namespace VirtualGarage.Serialization
{
    // Big thanks to AdamAdam
    [Serializable]
    public class VgBarricade
    {
        public ushort ID { get; set; }
        public ushort Health { get; set; }
        public byte[] State { get; set; }
        public ulong Owner { get; set; }
        public ulong Group { get; set; }
        public Vector3Wrapper Position { get; set; }
        public Vector3Wrapper Rotation { get; set; }
        
        public VgBarricade()
        {
            
        }
        
        public static VgBarricade Create(BarricadeDrop drop, BarricadeData data)
        {
            var barricade = new VgBarricade
            {
                Position = new Vector3Wrapper(data.point),
                Rotation = new Vector3Wrapper(drop.model.transform.localEulerAngles),
                Owner = data.owner,
                Group = data.@group,
                State = data.barricade.state,
                Health = data.barricade.health,
                ID = data.barricade.id
            };

            return barricade;
        }
        public Transform SpawnBarricade(Transform hit)
        {
            var barricade = new Barricade(ID, Health, State, (ItemBarricadeAsset)Assets.find(EAssetType.ITEM, ID));
            return hit != null ? BarricadeManager.dropPlantedBarricade(hit, barricade, Position.ToVector3(), Quaternion.Euler(Rotation.x, Rotation.y, Rotation.z), Owner, Group) : BarricadeManager.dropNonPlantedBarricade(barricade, Position.ToVector3(), Quaternion.Euler(Rotation.x, Rotation.y, Rotation.z), Owner, Group);
        }
    }
}