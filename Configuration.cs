using System.Collections.Generic;
using System.Xml.Serialization;
using Rocket.API;
using VirtualGarage.Models;

namespace VirtualGarage
{
    public class Configuration : IRocketPluginConfiguration
    {
        public string DatabaseAddress;
        public uint DatabasePort;
        public string DatabaseUsername;
        public string DatabasePassword;
        public string DatabaseName;
        public string DatabaseTableName;
        public string MessageColor;
        public bool AutoGarageDrownedVehicles;
        public float CheckDrownedIntervalSeconds;
        public bool AutoClearDestroyedVehicles;
        public List<Garage> VirtualGarages;
        [XmlArray, XmlArrayItem("Barricades")]
        public List<Blacklist> BlacklistedBarricades;
        [XmlArray, XmlArrayItem("TrunkItems")]
        public List<Blacklist> BlacklistedTrunkItems;
        [XmlArray, XmlArrayItem("Vehicles")]
        public List<Blacklist> BlacklistedVehicles;
        
        public void LoadDefaults()
        {
            DatabaseAddress = "127.0.0.1";
            DatabasePort = 3306;
            DatabaseUsername = "root";
            DatabasePassword = "123456";
            DatabaseName = "unturned";
            DatabaseTableName = "virtualgarage";
            MessageColor = "magenta";
            AutoGarageDrownedVehicles = true;
            CheckDrownedIntervalSeconds = 2f;
            AutoClearDestroyedVehicles = true;
            VirtualGarages = new List<Garage>
            {
                new Garage
                {
                    Name = "Small",
                    Slot = 4,
                    Permission = "garage.small",
                },
                new Garage
                {
                    Name = "Medium",
                    Slot = 7,
                    Permission = "garage.medium",
                },
            };
            BlacklistedBarricades = new List<Blacklist> 
            {
                new Blacklist
                {
                    BypassPermission = "garagebypass.barricade.example", 
                    Assets = new List<AssetID> {new AssetID(1), new AssetID(2)},
                },
                new Blacklist
                {
                    BypassPermission = "garagebypass.barricade.example1", 
                    Assets = new List<AssetID> {new AssetID(1), new AssetID(2)},
                },
            };
            BlacklistedTrunkItems = new List<Blacklist> 
            {
                new Blacklist
                {
                    BypassPermission = "garagebypass.trunk.example", 
                    Assets = new List<AssetID> {new AssetID(1), new AssetID(2)},
                },
                new Blacklist
                {
                    BypassPermission = "garagebypass.trunk.example1", 
                    Assets = new List<AssetID> {new AssetID(1), new AssetID(2)},
                },
            };
            BlacklistedVehicles = new List<Blacklist> 
            {
                new Blacklist
                {
                    BypassPermission = "garagebypass.vehicle.example", 
                    Assets = new List<AssetID> {new AssetID(1), new AssetID(2)},
                },
                new Blacklist
                {
                    BypassPermission = "garagebypass.vehicle.example1", 
                    Assets = new List<AssetID> {new AssetID(1), new AssetID(2)},
                },
            };
        }
    }
}