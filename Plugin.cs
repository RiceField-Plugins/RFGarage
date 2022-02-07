using RFGarage.DatabaseManagers;
using RFGarage.Enums;
using RFGarage.EventListeners;
using RFRocketLibrary.Enum;
using RFRocketLibrary.Events;
using RFRocketLibrary.Utils;
using Rocket.API.Collections;
using Rocket.Core.Plugins;
using Rocket.Unturned.Chat;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;

namespace RFGarage
{
    public class Plugin : RocketPlugin<Configuration>
    {
        private static int Major = 1;
        private static int Minor = 0;
        private static int Patch = 0;

        public static Plugin Inst;
        public static Configuration Conf;
        internal static Color MsgColor;
        public DatabaseManager Database;

        protected override void Load()
        {
            Inst = this;
            Conf = Configuration.Instance;

            if (Conf.Enabled)
            {
                MsgColor = UnturnedChat.GetColorFromName(Conf.MessageColor, Color.green);

                if (DependencyUtil.CanBeLoaded(EDependency.Harmony))
                {
                    DependencyUtil.Load(EDependency.Harmony);
                }

                if (DependencyUtil.CanBeLoaded(EDependency.NewtonsoftJson))
                {
                    DependencyUtil.Load(EDependency.NewtonsoftJson);
                    DependencyUtil.Load(EDependency.SystemRuntimeSerialization);
                }

                if (DependencyUtil.CanBeLoaded(EDependency.LiteDB))
                {
                    DependencyUtil.Load(EDependency.LiteDB);
                    DependencyUtil.Load(EDependency.LiteDBAsync);
                }

                if (DependencyUtil.CanBeLoaded(EDependency.Dapper))
                {
                    DependencyUtil.Load(EDependency.Dapper);
                    DependencyUtil.Load(EDependency.MySqlData);
                    DependencyUtil.Load(EDependency.SystemManagement);
                    DependencyUtil.Load(EDependency.UbietyDnsCore);
                }

                Database = new DatabaseManager();
                
                EventBus.Load();

                UnturnedEvent.OnVehicleExploded += VehicleEvent.OnExploded;
                if (Conf.AutoAddOnDrown)
                    UnturnedEvent.OnPreVehicleDestroyed += VehicleEvent.OnPreVehicleDestroyed;
            }
            else
                Logger.LogError($"[{Name}] Plugin: DISABLED");

            Logger.LogWarning($"[{Name}] Plugin loaded successfully!");
            Logger.LogWarning($"[{Name}] RFGarage v{Major}.{Minor}.{Patch}");
            Logger.LogWarning($"[{Name}] Made with 'rice' by RiceField Plugins!");
        }

        protected override void Unload()
        {
            if (Conf.Enabled)
            {
                StopAllCoroutines();

                UnturnedEvent.OnVehicleExploded -= VehicleEvent.OnExploded;
                if (Conf.AutoAddOnDrown)
                    UnturnedEvent.OnPreVehicleDestroyed -= VehicleEvent.OnPreVehicleDestroyed;
            }

            Conf = null;
            Inst = null;

            Logger.LogWarning($"[{Name}] Plugin unloaded successfully!");
        }

        public override TranslationList DefaultTranslations =>
            new TranslationList
            {
                {$"{EResponse.INVALID_PARAMETER}", "Invalid parameter! Usage: {0}"},
                {$"{EResponse.SAME_DATABASE}", "You can't migrate to the same database!"},
                {$"{EResponse.DATABASE_NOT_READY}", "Database is not ready, please wait..."},
                {$"{EResponse.MIGRATION_START}", "Starting migration from {0} to {1}..."},
                {$"{EResponse.MIGRATION_FINISH}", "Migration finished 100%!"},
                {$"{EResponse.NO_VEHICLE_INPUT}", "You are not looking or seating at any vehicle!"},
                {$"{EResponse.VEHICLE_NOT_OWNED}", "You don't own this vehicle!"},
                {$"{EResponse.GARAGE_FULL}", "Your garage slot is full! Max slot: {0}"},
                {$"{EResponse.TRAIN_NOT_ALLOWED}", "Train is not allowed!"},
                {$"{EResponse.BLACKLIST_VEHICLE}", "{0} ({1}) is blacklisted!"},
                {$"{EResponse.BLACKLIST_BARRICADE}", "This vehicle has blacklisted barricade! {0} ({1})"},
                {$"{EResponse.BLACKLIST_ITEM}", "This vehicle has blacklisted item! {0} ({1})"},
                {$"{EResponse.VEHICLE_NOT_FOUND}", "You don't have {0} inside your garage!"},
                {$"{EResponse.GARAGE_RETRIEVE}", "Successfully retrieved your {0} from garage!"},
                {$"{EResponse.GARAGE_ADDED}", "Successfully added {0} to garage!"},
                {$"{EResponse.NO_VEHICLE}", "You don't have any vehicle in garage!"},
                {$"{EResponse.GARAGE_SLOT}", "Current garage slot: {0}/{1}"},
                {$"{EResponse.GARAGE_LIST}", "#{0} {1} [Vehicle ID: {2} Vehicle Name: {3}]"},
                {$"{EResponse.VEHICLE_DROWN}", "Your drowned {0} has been added to your garage automatically!"},
            };
    }
}