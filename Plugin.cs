using System.Collections.Generic;
using System.Globalization;
using RFGarage.DatabaseManagers;
using RFGarage.EventListeners;
using RFGarage.Models;
using RFGarage.Utils;
using Rocket.API.Collections;
using Rocket.Core.Plugins;
using Rocket.Unturned;
using Rocket.Unturned.Chat;
using SDG.Unturned;
using Steamworks;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;

namespace RFGarage
{
    public class Plugin : RocketPlugin<Configuration>
    {
        private Coroutine _drownCheckCor;
        internal static Dictionary<CSteamID, bool> GarageAddAllQueueDict;
        internal static Dictionary<CSteamID, IEnumerable<PlayerSerializableVehicleModel>> GarageRetrieveAllQueueDict;
        internal static Dictionary<CSteamID, GarageModel> SelectedGarageDict;
        public static Plugin Inst;
        public static Configuration Conf;
        public static MySqlDb DbManager;
        public static Color MsgColor;

        protected override void Load()
        {
            Inst = this;
            Conf = Configuration.Instance;
            DbManager = new MySqlDb(Conf.DatabaseAddress, Conf.DatabasePort, Conf.DatabaseUsername,
                Conf.DatabasePassword, Conf.DatabaseName, Conf.DatabaseTableName, MySqlDb.CreateTableQuery);
            MsgColor = UnturnedChat.GetColorFromName(Conf.MessageColor, Color.green);

            GarageAddAllQueueDict = new Dictionary<CSteamID, bool>();
            GarageRetrieveAllQueueDict = new Dictionary<CSteamID, IEnumerable<PlayerSerializableVehicleModel>>();
            SelectedGarageDict = new Dictionary<CSteamID, GarageModel>();
            if (Conf.AutoGarageDrownedVehicles && Level.isLoaded)
            {
                _drownCheckCor = StartCoroutine(AutoCheck());
            }

            Level.onLevelLoaded += OnLevelLoadedEvent;
            U.Events.OnPlayerConnected += PlayerEvent.OnConnected;
            U.Events.OnPlayerDisconnected += PlayerEvent.OnDisconnected;
            VehicleManager.OnVehicleExploded += VehicleEvent.OnExploded;
            
            Logger.LogWarning("[RFGarage] Plugin loaded successfully!");
            Logger.LogWarning("[RFGarage] RFGarage v1.0.3");
            Logger.LogWarning("[RFGarage] Made with 'rice' by RiceField Plugins!");
        }
        protected override void Unload()
        {
            GarageAddAllQueueDict.Clear();
            GarageRetrieveAllQueueDict.Clear();
            SelectedGarageDict.Clear();
            if (Conf.AutoGarageDrownedVehicles && Level.isLoaded)
            {
                StopCoroutine(_drownCheckCor);
            }
            
            Inst = null;
            Conf = null;
            DbManager = null;
            
            Level.onLevelLoaded -= OnLevelLoadedEvent;
            U.Events.OnPlayerConnected -= PlayerEvent.OnConnected;
            U.Events.OnPlayerDisconnected -= PlayerEvent.OnDisconnected;
            VehicleManager.OnVehicleExploded -= VehicleEvent.OnExploded;
            
            Logger.LogWarning("[RFGarage] Plugin unloaded successfully!");
        }
        public override TranslationList DefaultTranslations =>
            new TranslationList
            {
                {"rfgarage_command_all_no_queue", "[RFGarage] You don't have any related queue!"},
                {"rfgarage_command_all_abort", "[RFGarage] The process has been aborted"},
                {"rfgarage_command_all_fail_vehicle", "[RFGarage] FAILED: [ID] {0}, [Name] {1}"},
                {"rfgarage_command_all_success_vehicle", "[RFGarage] SUCCESS: [ID] {0}, [Name] {1}"},
                {"rfgarage_command_gadd_all_ask_confirm", "[RFGarage] Are you sure you want to add all of your vehicles? /gaall confirm | abort"},
                {"rfgarage_command_gadd_all_blacklist_vehicle", "[RFGarage] BLACKLIST: [ID] {0}, [Name] {1}"},
                {"rfgarage_command_gadd_all_confirm", "[RFGarage] Adding vehicles, please wait..."},
                {"rfgarage_command_gadd_all_no_queue", "[RFGarage] You don't have any related queue!"},
                {"rfgarage_command_gadd_all_success", "[RFGarage] Successfully added {0} vehicle(s) to your Garage(s)! Failed to add {1} vehicle(s)"},
                {"rfgarage_command_gadd_blacklist_barricade", "[RFGarage] You are not allowed to save barricade: {0} [{1}]!"},
                {"rfgarage_command_gadd_blacklist_trunk_item", "[RFGarage] You are not allowed to save trunk item: {0} [{1}]!"},
                {"rfgarage_command_gadd_blacklist_vehicle", "[RFGarage] You are not allowed to save vehicle: {0} [{1}]!"},
                {"rfgarage_command_gadd_garage_full", "[RFGarage] {0} Garage is full!"},
                {"rfgarage_command_gadd_garage_not_selected", "[RFGarage] Please /gset <garageName> first! or use /gadd <garageName> <vehicleName>"},
                {"rfgarage_command_gadd_success", "[RFGarage] Successfully added {0} [{1}] to {2} Garage!"},
                {"rfgarage_command_gadd_vehicle_not_owner", "[RFGarage] You don't own this vehicle!"},
                {"rfgarage_command_garage_no_permission", "[RFGarage] You are not allowed to use {0} Garage ({1})!"},
                {"rfgarage_command_garage_no_vehicle", "[RFGarage] You don't have any vehicle in {0} Garage! Check /glist"},
                {"rfgarage_command_garage_not_found", "[RFGarage] Garage not found! Check /glist"},
                {"rfgarage_command_glist_garages_success", "[RFGarage] Available Garages: {0}"},
                {"rfgarage_command_glist_garage_success", "[RFGarage] {0} Garage: {1}"},
                {"rfgarage_command_gr_all_ask_confirm", "[RFGarage] Are you sure you want to retrieve all of your vehicles? /grall confirm | abort"},
                {"rfgarage_command_gr_all_confirm", "[RFGarage] Retrieving vehicles, please wait..."},
                {"rfgarage_command_gr_all_garage_ask_confirm", "[RFGarage] Are you sure you want to retrieve all of your vehicles inside {0} Garage? /grall confirm | abort"},
                {"rfgarage_command_gr_all_queue_exists", "[RFGarage] Queue exists! Please do /grall abort if there is no running process"},
                {"rfgarage_command_gr_all_success", "[RFGarage] Successfully retrieved {0} vehicle(s) from your Garage(s)! Failed to retrieve {1} vehicle(s)"},
                {"rfgarage_command_gr_all_success_garage", "[RFGarage] Successfully retrieved {0} vehicle(s) from {1} Garage! Failed to retrieve {2} vehicle(s)"},
                {"rfgarage_command_gr_garage_not_selected", "[RFGarage] Please /gset <garageName> first! or use /gr <garageName> <vehicleName>"},
                {"rfgarage_command_gr_success", "[RFGarage] Successfully retrieved {0} [{1}] from {2} Garage!"},
                {"rfgarage_command_gset_success", "[RFGarage] Successfully set {0} as your default Garage!"},
                {"rfgarage_command_invalid_id", "[RFGarage] Please enter steamID!"},
                {"rfgarage_command_invalid_parameter", "[RFGarage] Invalid parameter! Usage: {0}"},
                {"rfgarage_command_player_not_online", "[RFGarage] Player must be online!"},
                {"rfgarage_command_sgr_garage_no_vehicle", "[RFGarage] ID: {0} don't have any vehicle in {0} Garage! Check /sglist"},
                {"rfgarage_command_vehicle_same_name_as_garage", "[RFGarage] Enter another name!"},
                {"rfgarage_command_vehicle_not_found", "[RFGarage] Vehicle object not found! Try again or do command inside vehicle!"},
            };
        
        public void OnLevelLoadedEvent(int level)
        {
            if (Conf.CheckDrownedIntervalSeconds >= Provider.modeConfigData.Vehicles.Respawn_Time)
            {
                if (Provider.modeConfigData.Vehicles.Respawn_Time - 1 <= 0)
                {
                    Conf.CheckDrownedIntervalSeconds = 0;
                    Configuration.Save();
                }
                else
                {
                    Conf.CheckDrownedIntervalSeconds -= 1;
                    Configuration.Save();
                }

                Logger.LogWarning("[RFGarage] Server Configuration Vehicle.Respawn_Time must not be less than CheckDrownedIntervalSeconds!");
                Logger.LogWarning($"[RFGarage] CheckDrownedIntervalSeconds changed to {Conf.CheckDrownedIntervalSeconds} seconds");
            }
            
            _drownCheckCor = StartCoroutine(AutoCheck());
        }
        
        private static IEnumerator<WaitForSeconds> AutoCheck()
        {
            while (Conf.AutoGarageDrownedVehicles)
            {
                CheckForDrownedVehicles();
                yield return new WaitForSeconds(Conf.CheckDrownedIntervalSeconds);
            }
        }
        private static void CheckForDrownedVehicles()
        {
            var vehicles = VehicleManager.vehicles;
            foreach (var vehicle in vehicles)
            {
                if (!VehicleUtil.VehicleHasOwner(vehicle))
                    continue;
                if (!vehicle.isDrowned)
                    continue;
                var drownedVehicleRegion = BarricadeManager.getRegionFromVehicle(vehicle);
                GarageUtil.SaveVgVehicleToSql(vehicle.lockedOwner.m_SteamID, "Drown", "Drowned", vehicle,
                    drownedVehicleRegion);
            }
        }
    }
}