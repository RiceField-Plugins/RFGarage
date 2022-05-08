using System;
using RFRocketLibrary.Models;

namespace RFGarage.Models
{
    [Serializable]
    public class PlayerGarage
    {
        public int Id { get; set; }
        public ulong SteamId { get; set; }
        public string VehicleName { get; set; } = string.Empty;
        public VehicleWrapper GarageContent { get; set; } = new();
        public DateTime LastUpdated { get; set; } = DateTime.Now;       

        public PlayerGarage()
        {
        }

        public PlayerGarage Copy()
        {
            return (PlayerGarage) MemberwiseClone();
        }
    }
}