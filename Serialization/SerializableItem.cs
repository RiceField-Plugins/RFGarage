using System;
using SDG.Unturned;
using RFGarage.Models;

namespace RFGarage.Serialization
{
    // Big thanks to AdamAdam
    [Serializable]
    public class SerializableItem
    {
        public ushort ID { get; set; }
        public byte Amount { get; set; }
        public byte Quality { get; set; }
        public byte[] State { get; set; }
        public SerializableItemPosition? Position { get; set; }

        public SerializableItem()
        {
            
        }
        public static SerializableItem Create(Item item)
        {
            return new SerializableItem
            {
                ID = item.id,
                Amount = item.amount,
                Quality = item.quality,
                State = item.state,
            };
        }
        public static SerializableItem Create(byte page, ItemJar item)
        {
            var result = Create(item.item);
            result.Position = new SerializableItemPosition
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