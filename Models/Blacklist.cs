using System.Collections.Generic;
using System.Xml.Serialization;
using RFGarageClassic.Enums;

namespace RFGarageClassic.Models
{
    public class Blacklist
    {
        [XmlAttribute]
        public EBlacklistType Type;
        [XmlAttribute]
        public string BypassPermission;
        [XmlArrayItem("Id")]
        public List<ushort> IdList;

        public Blacklist()
        {
            
        }
    }
}