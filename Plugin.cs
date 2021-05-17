using System.Collections.Generic;
using Rocket.API.Collections;
using Rocket.Core.Plugins;
using Rocket.Unturned;
using Rocket.Unturned.Chat;
using SDG.Unturned;
using Steamworks;
using UnityEngine;
using VirtualGarage.DatabaseManagers;
using VirtualGarage.EventListeners;
using VirtualGarage.Models;
using VirtualGarage.Utils;
using Logger = Rocket.Core.Logging.Logger;

namespace VirtualGarage
{
    public class Plugin : RocketPlugin<Configuration>
    {
        private Coroutine _drownCheckCor;
        internal static Dictionary<CSteamID, bool> GarageAddAllQueueDict;
        internal static Dictionary<CSteamID, IEnumerable<PlayerVgVehicle>> GarageRetrieveAllQueueDict;
        internal static Dictionary<CSteamID, Garage> SelectedGarageDict;
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
            GarageRetrieveAllQueueDict = new Dictionary<CSteamID, IEnumerable<PlayerVgVehicle>>();
            SelectedGarageDict = new Dictionary<CSteamID, Garage>();
            if (Conf.AutoGarageDrownedVehicles && Level.isLoaded)
            {
                _drownCheckCor = StartCoroutine(AutoCheck());
            }

            Level.onLevelLoaded += OnLevelLoadedEvent;
            U.Events.OnPlayerConnected += PlayerEvent.OnConnected;
            U.Events.OnPlayerDisconnected += PlayerEvent.OnDisconnected;
            VehicleManager.OnVehicleExploded += VehicleEvent.OnExploded;
            
            Logger.LogWarning("[VirtualGarage] Plugin loaded successfully!");
            Logger.LogWarning("[VirtualGarage] VirtualGarage v1.0.2");
            Logger.LogWarning("[VirtualGarage] Author: BarehSolok#2548");
            Logger.LogWarning("[VirtualGarage] Enjoy the plugin! ;)");
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
            
            Logger.LogWarning("[VirtualGarage] Plugin unloaded successfully!");
        }
        public override TranslationList DefaultTranslations =>
            new TranslationList
            {
                {"virtualgarage_command_all_no_queue", "[VirtualGarage] You don't have any related queue!"},
                {"virtualgarage_command_all_abort", "[VirtualGarage] The process has been aborted"},
                {"virtualgarage_command_all_fail_vehicle", "[VirtualGarage] FAILED: [ID] {0}, [Name] {1}"},
                {"virtualgarage_command_all_success_vehicle", "[VirtualGarage] SUCCESS: [ID] {0}, [Name] {1}"},
                {"virtualgarage_command_gadd_all_ask_confirm", "[VirtualGarage] Are you sure you want to add all of your vehicles? /gaall confirm | abort"},
                {"virtualgarage_command_gadd_all_blacklist_vehicle", "[VirtualGarage] BLACKLIST: [ID] {0}, [Name] {1}"},
                {"virtualgarage_command_gadd_all_confirm", "[VirtualGarage] Adding vehicles, please wait..."},
                {"virtualgarage_command_gadd_all_no_queue", "[VirtualGarage] You don't have any related queue!"},
                {"virtualgarage_command_gadd_all_success", "[VirtualGarage] Successfully added {0} vehicle(s) to your Garage(s)! Failed to add {1} vehicle(s)"},
                {"virtualgarage_command_gadd_blacklist_barricade", "[VirtualGarage] You are not allowed to save barricade: {0} [{1}]!"},
                {"virtualgarage_command_gadd_blacklist_trunk_item", "[VirtualGarage] You are not allowed to save trunk item: {0} [{1}]!"},
                {"virtualgarage_command_gadd_blacklist_vehicle", "[VirtualGarage] You are not allowed to save vehicle: {0} [{1}]!"},
                {"virtualgarage_command_gadd_garage_full", "[VirtualGarage] {0} Garage is full!"},
                {"virtualgarage_command_gadd_garage_not_selected", "[VirtualGarage] Please /gset <garageName> first! or use /gadd <garageName> <vehicleName>"},
                {"virtualgarage_command_gadd_success", "[VirtualGarage] Successfully added {0} [{1}] to {2} Garage!"},
                {"virtualgarage_command_gadd_vehicle_not_owner", "[VirtualGarage] You don't own this vehicle!"},
                {"virtualgarage_command_garage_no_permission", "[VirtualGarage] You are not allowed to use {0} Garage ({1})!"},
                {"virtualgarage_command_garage_no_vehicle", "[VirtualGarage] You don't have any vehicle in {0} Garage! Check /glist"},
                {"virtualgarage_command_garage_not_found", "[VirtualGarage] Garage not found! Check /glist"},
                {"virtualgarage_command_glist_garages_success", "[VirtualGarage] Available Garages: {0}"},
                {"virtualgarage_command_glist_garage_success", "[VirtualGarage] {0} Garage: {1}"},
                {"virtualgarage_command_gr_all_ask_confirm", "[VirtualGarage] Are you sure you want to retrieve all of your vehicles? /grall confirm | abort"},
                {"virtualgarage_command_gr_all_confirm", "[VirtualGarage] Retrieving vehicles, please wait..."},
                {"virtualgarage_command_gr_all_garage_ask_confirm", "[VirtualGarage] Are you sure you want to retrieve all of your vehicles inside {0} Garage? /grall confirm | abort"},
                {"virtualgarage_command_gr_all_queue_exists", "[VirtualGarage] Queue exists! Please do /grall abort if there is no running process"},
                {"virtualgarage_command_gr_all_success", "[VirtualGarage] Successfully retrieved {0} vehicle(s) from your Garage(s)! Failed to retrieve {1} vehicle(s)"},
                {"virtualgarage_command_gr_all_success_garage", "[VirtualGarage] Successfully retrieved {0} vehicle(s) from {1} Garage! Failed to retrieve {2} vehicle(s)"},
                {"virtualgarage_command_gr_garage_not_selected", "[VirtualGarage] Please /gset <garageName> first! or use /gr <garageName> <vehicleName>"},
                {"virtualgarage_command_gr_success", "[VirtualGarage] Successfully retrieved {0} [{1}] from {2} Garage!"},
                {"virtualgarage_command_gset_success", "[VirtualGarage] Successfully set {0} as your default Garage!"},
                {"virtualgarage_command_invalid_id", "[VirtualGarage] Please enter steamID!"},
                {"virtualgarage_command_invalid_parameter", "[VirtualGarage] Invalid parameter! Usage: {0}"},
                {"virtualgarage_command_player_not_online", "[VirtualGarage] Player must be online!"},
                {"virtualgarage_command_sgr_garage_no_vehicle", "[VirtualGarage] ID: {0} don't have any vehicle in {0} Garage! Check /sglist"},
                {"virtualgarage_command_vehicle_same_name_as_garage", "[VirtualGarage] Enter another name!"},
                {"virtualgarage_command_vehicle_not_found", "[VirtualGarage] Vehicle object not found! Try again or do command inside vehicle!"},
            };
        
        public void OnLevelLoadedEvent(int level)
        {
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
                if (VehicleUtil.VehicleHasPassenger(vehicle))
                    continue;
                var drownedVehicleRegion = BarricadeManager.getRegionFromVehicle(vehicle);
                GarageUtil.SaveVgVehicleToSql(vehicle.lockedOwner.m_SteamID, "Drown", "Drowned", vehicle,
                    drownedVehicleRegion);
            }
        }
    }
}