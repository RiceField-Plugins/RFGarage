using System.Xml.Serialization;

namespace VirtualGarage.Models
{
    public class AssetID
    {
        [XmlAttribute]
        public ushort ID;

        public AssetID()
        {
            
        }
        public AssetID(ushort id)
        {
            ID = id;
        }
    }
}