using System.Xml.Serialization;

namespace RFGarage.Models
{
    public class AssetModel
    {
        [XmlAttribute]
        public ushort ID;

        public AssetModel()
        {
            
        }
        public AssetModel(ushort id)
        {
            ID = id;
        }
    }
}