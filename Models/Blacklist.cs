using System.Collections.Generic;

namespace VirtualGarage.Models
{
    public class Blacklist
    {
        public List<AssetID> Assets;
        public string BypassPermission;

        public Blacklist()
        {
            
        }
        public Blacklist(List<AssetID> ids, string bypassPermission)
        {
            Assets = ids;
            BypassPermission = bypassPermission;
        }
    }
}