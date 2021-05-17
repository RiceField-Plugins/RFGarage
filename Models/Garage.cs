using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Rocket.Core;
using Rocket.Unturned.Player;

namespace VirtualGarage.Models
{
    public class Garage
    {
        [XmlAttribute]
        public string Name;
        [XmlAttribute]
        public uint Slot;
        [XmlAttribute]
        public string Permission;

        public Garage()
        {
        }
        public Garage(string name, uint slot, string permission)
        {
            Name = name;
            Slot = slot;
            Permission = permission;
        }

        public static Garage Parse(string garageName)
        {
            if (garageName == "" || garageName == string.Empty)
                return null;

            return Plugin.Conf.VirtualGarages.FirstOrDefault(virtualGarage => string.Equals(virtualGarage.Name, garageName, StringComparison.CurrentCultureIgnoreCase));
        }
        public static bool TryParse(string garageName, out Garage garage)
        {
            garage = null;

            if (garageName == "" || garageName == string.Empty)
                return false;

            foreach (var virtualGarage in Plugin.Conf.VirtualGarages.Where(virtualGarage => string.Equals(garageName, virtualGarage.Name, StringComparison.CurrentCultureIgnoreCase)))
            {
                garage = virtualGarage;
                return true;
            }

            return false;
        }
    }
}