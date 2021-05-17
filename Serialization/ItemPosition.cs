using System;

namespace VirtualGarage.Serialization
{
    // Big thanks to AdamAdam
    [Serializable]
    public struct ItemPosition
    {
        public byte Page { get; set; }
        public byte X { get; set; }
        public byte Y { get; set; }
        public byte Rot { get; set; }
    }
}