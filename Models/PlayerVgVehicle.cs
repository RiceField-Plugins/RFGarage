using VirtualGarage.Utils;

namespace VirtualGarage.Models
{
    public class PlayerVgVehicle
    {
        public ulong EntryID;
        public ulong SteamID;
        public string GarageName;
        public string VehicleName;
        public string Info;
        
        public PlayerVgVehicle() { }

        public PlayerVgVehicle(ulong entryID, ulong steamID, string garageName, string vehicleName, string info)
        {
            EntryID = entryID;
            SteamID = steamID;
            GarageName = garageName;
            VehicleName = vehicleName;
            Info = info;
        }
    }
}