using System.Collections.Generic;
using System.Xml.Serialization;
using RFGarage.Models;
using Rocket.API;

namespace RFGarage
{
    public class Configuration : IRocketPluginConfiguration
    {
        public bool Enabled;
        public string DatabaseAddress;
        public uint DatabasePort;
        public string DatabaseUsername;
        public string DatabasePassword;
        public string DatabaseName;
        public string DatabaseTableName;
        public string MessageColor;
        public string AnnouncerIconUrl;
        public bool AutoGarageDrownedVehicles;
        public float CheckDrownedIntervalSeconds;
        public uint DrownGarageSlot;
        public bool AutoClearDestroyedVehicles;
        [XmlArrayItem("Garage")]
        public List<GarageModel> VirtualGarages;
        [XmlArray, XmlArrayItem("Barricades")]
        public List<BlacklistModel> BlacklistedBarricades;
        [XmlArray, XmlArrayItem("TrunkItems")]
        public List<BlacklistModel> BlacklistedTrunkItems;
        [XmlArray, XmlArrayItem("Vehicles")]
        public List<BlacklistModel> BlacklistedVehicles;
        
        public void LoadDefaults()
        {
            Enabled = false;
            DatabaseAddress = "127.0.0.1";
            DatabasePort = 3306;
            DatabaseUsername = "root";
            DatabasePassword = "123456";
            DatabaseName = "unturned";
            DatabaseTableName = "rfgarage";
            MessageColor = "magenta";
            AnnouncerIconUrl = "https://i.imgur.com/3KlgN14.png";
            AutoGarageDrownedVehicles = true;
            CheckDrownedIntervalSeconds = 5f;
            DrownGarageSlot = 0;
            AutoClearDestroyedVehicles = true;
            VirtualGarages = new List<GarageModel>
            {
                new GarageModel
                {
                    Name = "Small",
                    Slot = 4,
                    Permission = "garage.small",
                },
                new GarageModel
                {
                    Name = "Medium",
                    Slot = 7,
                    Permission = "garage.medium",
                },
            };
            BlacklistedBarricades = new List<BlacklistModel> 
            {
                new BlacklistModel
                {
                    BypassPermission = "garagebypass.barricade.example", 
                    Assets = new List<AssetModel> {new AssetModel(1), new AssetModel(2)},
                },
                new BlacklistModel
                {
                    BypassPermission = "garagebypass.barricade.example1", 
                    Assets = new List<AssetModel> {new AssetModel(1), new AssetModel(2)},
                },
            };
            BlacklistedTrunkItems = new List<BlacklistModel> 
            {
                new BlacklistModel
                {
                    BypassPermission = "garagebypass.trunk.example", 
                    Assets = new List<AssetModel> {new AssetModel(1), new AssetModel(2)},
                },
                new BlacklistModel
                {
                    BypassPermission = "garagebypass.trunk.example1", 
                    Assets = new List<AssetModel> {new AssetModel(1), new AssetModel(2)},
                },
            };
            BlacklistedVehicles = new List<BlacklistModel> 
            {
                new BlacklistModel
                {
                    BypassPermission = "garagebypass.vehicle.example", 
                    Assets = new List<AssetModel> {new AssetModel(1), new AssetModel(2)},
                },
                new BlacklistModel
                {
                    BypassPermission = "garagebypass.vehicle.example1", 
                    Assets = new List<AssetModel> {new AssetModel(1), new AssetModel(2)},
                },
            };
        }
    }
}