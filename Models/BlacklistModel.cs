using System.Collections.Generic;
using System.Xml.Serialization;

namespace RFGarage.Models
{
    public class BlacklistModel
    {
        [XmlArrayItem("Asset")]
        public List<AssetModel> Assets;
        public string BypassPermission;

        public BlacklistModel()
        {
            
        }
        public BlacklistModel(List<AssetModel> ids, string bypassPermission)
        {
            Assets = ids;
            BypassPermission = bypassPermission;
        }
    }
}