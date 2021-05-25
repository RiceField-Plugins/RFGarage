using RFGarage.Utils;

namespace RFGarage.Models
{
    public class PlayerSerializableVehicleModel
    {
        public ulong EntryID;
        public ulong SteamID;
        public string GarageName;
        public string VehicleName;
        public string Info;
        
        public PlayerSerializableVehicleModel() { }

        public PlayerSerializableVehicleModel(ulong entryID, ulong steamID, string garageName, string vehicleName, string info)
        {
            EntryID = entryID;
            SteamID = steamID;
            GarageName = garageName;
            VehicleName = vehicleName;
            Info = info;
        }
    }
}