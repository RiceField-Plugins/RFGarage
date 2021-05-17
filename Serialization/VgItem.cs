using System;
using SDG.Unturned;
using VirtualGarage.Models;

namespace VirtualGarage.Serialization
{
    // Big thanks to AdamAdam
    [Serializable]
    public class VgItem
    {
        public ushort ID { get; set; }
        public byte Amount { get; set; }
        public byte Quality { get; set; }
        public byte[] State { get; set; }
        public ItemPosition? Position { get; set; }

        public VgItem()
        {
            
        }
        public static VgItem Create(Item item)
        {
            return new VgItem
            {
                ID = item.id,
                Amount = item.amount,
                Quality = item.quality,
                State = item.state,
            };
        }
        public static VgItem Create(byte page, ItemJar item)
        {
            var result = Create(item.item);
            result.Position = new ItemPosition
            {
                Page = page,
                Rot = item.rot,
                X = item.x,
                Y = item.y
            };
            return result;
        }
        public Item ToItem() => new Item(ID, Amount, Quality, State);
    }
}