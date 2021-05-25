using System;

namespace RFGarage.Serialization
{
    // Big thanks to AdamAdam
    [Serializable]
    public struct SerializableItemPosition
    {
        public byte Page { get; set; }
        public byte X { get; set; }
        public byte Y { get; set; }
        public byte Rot { get; set; }
    }
}