using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using RFGarage.Enums;
using RFGarage.Models;
using RFGarage.Utils;
using RFRocketLibrary.Models;
using RFRocketLibrary.Storages;
using RFRocketLibrary.Utils;
using Logger = Rocket.Core.Logging.Logger;

namespace RFGarage.DatabaseManagers
{
    public static class GarageManager
    {
        internal static bool Ready { get; set; }
        private static List<PlayerGarage> Json_Collection { get; set; } = new List<PlayerGarage>();
        private static List<PlayerGarage> MigrateCollection { get; set; } = new List<PlayerGarage>();

        private static readonly string LiteDB_TableName = "garage";

        private static readonly string Json_FileName = "garage.json";
        private static JsonDataStore<List<PlayerGarage>> Json_DataStore { get; set; }

        private static string MySql_TableName => $"{DatabaseManager.MySql_TableName}";

        private static readonly string MySql_CreateTableQuery =
            "`Id` INT NOT NULL AUTO_INCREMENT, " +
            "`SteamId` VARCHAR(32) NOT NULL DEFAULT '0', " +
            "`VehicleName` VARCHAR(255) NOT NULL DEFAULT 'N/A', " +
            "`GarageContent` TEXT NOT NULL, " +
            "`LastUpdated` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP," +
            "PRIMARY KEY (`Id`)";

        internal static void Initialize()
        {
            try
            {
                switch (Plugin.Conf.Database)
                {
                    case EDatabase.LITEDB:
                        LiteDB_Init();
                        break;
                    case EDatabase.JSON:
                        Json_DataStore = new JsonDataStore<List<PlayerGarage>>(Plugin.Inst.Directory, Json_FileName);
                        JSON_Reload();
                        break;
                    case EDatabase.MYSQL:
                        // new CP1250();
                        MySQL_CreateTable(MySql_TableName, MySql_CreateTableQuery);
                        break;
                }

                Ready = true;
            }
            catch (Exception e)
            {
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] GarageManager Initializing: {e.Message}");
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] Details: {e}");
            }
        }

        private static int Json_NewId()
        {
            return (Json_Collection.Max(x => x.Id as int?) ?? 0) + 1;
        }

        private static void JSON_Reload(bool migrate = false)
        {
            try
            {
                if (migrate)
                {
                    MigrateCollection = Json_DataStore.Load() ?? new List<PlayerGarage>();
                    return;
                }

                Json_Collection = Json_DataStore.Load() ?? new List<PlayerGarage>();
                Json_DataStore.Save(Json_Collection);
            }
            catch (Exception e)
            {
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] GarageManager JSON_Reload: {e.Message}");
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] Details: {e}");
            }
        }

        private static void MySQL_CreateTable(string tableName, string createTableQuery)
        {
            try
            {
                using (var connection =
                       new MySql.Data.MySqlClient.MySqlConnection(DatabaseManager.MySql_ConnectionString))
                {
                    Dapper.SqlMapper.Execute(connection,
                        $"CREATE TABLE IF NOT EXISTS `{tableName}` ({createTableQuery});");
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] GarageManager MySQL_CreateTable: {e.Message}");
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] Details: {e}");
            }
        }

        private static void LiteDB_Init()
        {
            using (var db = new LiteDB.LiteDatabase(DatabaseManager.LiteDB_ConnectionString))
            {
                var col = db.GetCollection<PlayerGarage>(LiteDB_TableName);
                if (db.UserVersion == 0)
                {
                    col.EnsureIndex(x => x.Id);
                    col.EnsureIndex(x => x.SteamId);
                    col.EnsureIndex(x => x.VehicleName);
                    col.EnsureIndex(x => x.LastUpdated);

                    db.UserVersion = 1;
                }
            }
        }

        private static async Task<List<PlayerGarage>> LiteDB_LoadAllAsync()
        {
            try
            {
                var result = new List<PlayerGarage>();
                using (var db = new LiteDB.Async.LiteDatabaseAsync(DatabaseManager.LiteDB_ConnectionString))
                {
                    var col = db.GetCollection<PlayerGarage>(LiteDB_TableName);
                    var all = await col.FindAllAsync();
                    result.AddRange(all);
                }

                return result;
            }
            catch (Exception e)
            {
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] GarageManager LiteDB_LoadAllAsync: {e.Message}");
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] Details: {e}");
                return new List<PlayerGarage>();
            }
        }

        private static async Task<List<PlayerGarage>> MySQL_LoadAllAsync()
        {
            try
            {
                var result = new List<PlayerGarage>();
                using (var connection =
                       new MySql.Data.MySqlClient.MySqlConnection(DatabaseManager.MySql_ConnectionString))
                {
                    var loadQuery = $"SELECT * FROM `{MySql_TableName}`;";
                    var databases = await Dapper.SqlMapper.QueryAsync(connection, loadQuery);
                    result.AddRange(from database in databases.Cast<IDictionary<string, object>>()
                        let byteArray = database["GarageContent"].ToString().ToByteArray()
                        let garageContent = byteArray.Deserialize<VehicleWrapper>()
                        select new PlayerGarage
                        {
                            Id = Convert.ToInt32(database["Id"]),
                            SteamId = Convert.ToUInt64(database["SteamId"]),
                            VehicleName = database["VehicleName"].ToString(),
                            GarageContent = garageContent,
                            LastUpdated = Convert.ToDateTime(database["LastUpdated"]),
                        });
                }

                return result;
            }
            catch (Exception e)
            {
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] GarageManager MySQL_LoadAllAsync: {e.Message}");
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] Details: {e}");
                return new List<PlayerGarage>();
            }
        }

        public static async Task<int> AddAsync(PlayerGarage playerGarage)
        {
            try
            {
                switch (Plugin.Conf.Database)
                {
                    case EDatabase.LITEDB:
                        using (var db = new LiteDB.Async.LiteDatabaseAsync(DatabaseManager.LiteDB_ConnectionString))
                        {
                            var col = db.GetCollection<PlayerGarage>(LiteDB_TableName);
                            return await col.InsertAsync(playerGarage);
                        }
                    case EDatabase.JSON:
                        playerGarage.Id = Json_NewId();
                        Json_Collection.Add(playerGarage);
                        await Json_DataStore.SaveAsync(Json_Collection);
                        return playerGarage.Id;
                    case EDatabase.MYSQL:
                        using (var connection =
                               new MySql.Data.MySqlClient.MySqlConnection(DatabaseManager.MySql_ConnectionString))
                        {
                            var serialized = playerGarage.GarageContent.Serialize();
                            var garageContent = serialized.ToBase64();
                            var insertQuery =
                                $"INSERT INTO `{MySql_TableName}` (`SteamId`, `VehicleName`, `GarageContent`) " +
                                "VALUES(@SteamId, @VehicleName, @GarageContent); SELECT last_insert_id();";
                            var parameter = new Dapper.DynamicParameters();
                            parameter.Add("@SteamId", playerGarage.SteamId, DbType.String, ParameterDirection.Input);
                            parameter.Add("@VehicleName", playerGarage.VehicleName, DbType.String,
                                ParameterDirection.Input);
                            parameter.Add("@GarageContent", garageContent, DbType.String, ParameterDirection.Input);
                            var lastId =
                                await Dapper.SqlMapper.ExecuteScalarAsync<int>(connection, insertQuery, parameter);
                            return lastId;
                        }
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] GarageManager AddAsync: {e.Message}");
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] Details: {e}");
            }

            return -1;
        }

        public static async Task<PlayerGarage> Get(ulong steamId, string vehicleName)
        {
            try
            {
                switch (Plugin.Conf.Database)
                {
                    case EDatabase.JSON:
                        return Json_Collection.Find(x =>
                            x.SteamId == steamId &&
                            x.VehicleName.Equals(vehicleName, StringComparison.OrdinalIgnoreCase));
                    case EDatabase.LITEDB:
                        using (var db = new LiteDB.Async.LiteDatabaseAsync(DatabaseManager.LiteDB_ConnectionString))
                        {
                            var col = db.GetCollection<PlayerGarage>(LiteDB_TableName);
                            var result = await col.Query()
                                .Where(x => x.SteamId == steamId).ToListAsync();
                            return result?.FirstOrDefault(x =>
                                x.VehicleName.ToLower().Contains(vehicleName.ToLower()));
                        }
                    case EDatabase.MYSQL:
                        using (var connection =
                               new MySql.Data.MySqlClient.MySqlConnection(DatabaseManager.MySql_ConnectionString))
                        {
                            var query =
                                $"SELECT * FROM `{MySql_TableName}` WHERE `SteamId` = @SteamId AND LOCATE('{vehicleName}', `VehicleName`) > 0;";
                            var objects = await Dapper.SqlMapper.QueryAsync(connection, query, new {SteamId = steamId});
                            var first = objects?.Cast<IDictionary<string, object>>().FirstOrDefault();
                            if (first == null)
                                return null;
                            
                            var byteArray = first["GarageContent"].ToString().ToByteArray();
                            var garageContent = byteArray.Deserialize<VehicleWrapper>();
                            return new PlayerGarage
                            {
                                Id = Convert.ToInt32(first["Id"]),
                                SteamId = Convert.ToUInt64(first["SteamId"]),
                                VehicleName = first["VehicleName"].ToString(),
                                GarageContent = garageContent,
                                LastUpdated = Convert.ToDateTime(first["LastUpdated"]),
                            };
                        }
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] GarageManager Get: {e.Message}");
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] Details: {e}");
                return null;
            }
        }

        public static async Task<List<PlayerGarage>> Get(ulong steamId)
        {
            try
            {
                switch (Plugin.Conf.Database)
                {
                    case EDatabase.JSON:
                        return Json_Collection.FindAll(x => x.SteamId == steamId);
                    case EDatabase.LITEDB:
                        using (var db = new LiteDB.Async.LiteDatabaseAsync(DatabaseManager.LiteDB_ConnectionString))
                        {
                            var col = db.GetCollection<PlayerGarage>(LiteDB_TableName);
                            return await col.Query().Where(x => x.SteamId == steamId).ToListAsync();
                        }
                    case EDatabase.MYSQL:
                        using (var connection =
                               new MySql.Data.MySqlClient.MySqlConnection(DatabaseManager.MySql_ConnectionString))
                        {
                            var query =
                                $"SELECT * FROM `{MySql_TableName}` WHERE `SteamId` = @SteamId;";
                            var objects = await Dapper.SqlMapper.QueryAsync(connection, query, new {SteamId = steamId});
                            if (objects == null)
                                return null;
                            
                            return (from database in objects.Cast<IDictionary<string, object>>()
                                let byteArray = database["GarageContent"].ToString().ToByteArray()
                                let garageContent = byteArray.Deserialize<VehicleWrapper>()
                                select new PlayerGarage
                                {
                                    Id = Convert.ToInt32(database["Id"]),
                                    SteamId = Convert.ToUInt64(database["SteamId"]),
                                    VehicleName = database["VehicleName"].ToString(),
                                    GarageContent = garageContent,
                                    LastUpdated = Convert.ToDateTime(database["LastUpdated"]),
                                }).ToList();
                        }
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] GarageManager Get: {e.Message}");
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] Details: {e}");
                return null;
            }
        }

        public static int Count(ulong steamId)
        {
            try
            {
                switch (Plugin.Conf.Database)
                {
                    case EDatabase.JSON:
                        return Json_Collection.Count(x => x.SteamId == steamId);
                    case EDatabase.LITEDB:
                        using (var db = new LiteDB.LiteDatabase(DatabaseManager.LiteDB_ConnectionString))
                        {
                            var col = db.GetCollection<PlayerGarage>(LiteDB_TableName);
                            return col.Count(x => x.SteamId == steamId);
                        }
                    case EDatabase.MYSQL:
                        using (var connection =
                               new MySql.Data.MySqlClient.MySqlConnection(DatabaseManager.MySql_ConnectionString))
                        {
                            var query =
                                $"SELECT COUNT(`Id`) FROM `{MySql_TableName}` WHERE `SteamId` = @SteamId;";
                            return Dapper.SqlMapper.ExecuteScalar<int>(connection, query, new {SteamId = steamId});
                        }
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] GarageManager Get: {e.Message}");
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] Details: {e}");
                return 0;
            }
        }

        public static async Task DeleteAsync(int id)
        {
            try
            {
                switch (Plugin.Conf.Database)
                {
                    case EDatabase.JSON:
                        Json_Collection.RemoveAt(Json_Collection.FindIndex(x => x.Id == id));
                        await Json_DataStore.SaveAsync(Json_Collection);
                        break;
                    case EDatabase.LITEDB:
                        using (var db = new LiteDB.Async.LiteDatabaseAsync(DatabaseManager.LiteDB_ConnectionString))
                        {
                            var col = db.GetCollection<PlayerGarage>(LiteDB_TableName);
                            await col.DeleteAsync(id);
                        }

                        break;
                    case EDatabase.MYSQL:
                        using (var connection =
                               new MySql.Data.MySqlClient.MySqlConnection(DatabaseManager.MySql_ConnectionString))
                        {
                            await Dapper.SqlMapper.ExecuteAsync(connection,
                                $"DELETE FROM `{MySql_TableName}` WHERE `Id` = @Id;", new {Id = id});
                        }

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] GarageManager DeleteAsync: {e.Message}");
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] Details: {e}");
            }
        }

        internal static async Task MigrateAsync(EDatabase from, EDatabase to)
        {
            try
            {
                switch (from)
                {
                    case EDatabase.LITEDB:
                        MigrateCollection = await LiteDB_LoadAllAsync();
                        switch (to)
                        {
                            case EDatabase.JSON:
                                Json_DataStore =
                                    new JsonDataStore<List<PlayerGarage>>(Plugin.Inst.Directory, Json_FileName);
                                await Json_DataStore.SaveAsync(MigrateCollection);
                                break;
                            case EDatabase.MYSQL:
                                MySQL_CreateTable(MySql_TableName, MySql_CreateTableQuery);
                                using (var connection =
                                       new MySql.Data.MySqlClient.MySqlConnection(
                                           DatabaseManager.MySql_ConnectionString))
                                {
                                    var deleteQuery = $"DELETE FROM `{MySql_TableName}`;";
                                    await Dapper.SqlMapper.ExecuteAsync(connection, deleteQuery);

                                    foreach (var playerGarage in MigrateCollection)
                                    {
                                        var serialized = playerGarage.GarageContent.Serialize();
                                        var garageContent = serialized.ToBase64();
                                        var parameter = new Dapper.DynamicParameters();
                                        parameter.Add("@Id", playerGarage.Id, DbType.Int32, ParameterDirection.Input);
                                        parameter.Add("@SteamId", playerGarage.SteamId, DbType.String,
                                            ParameterDirection.Input);
                                        parameter.Add("@VehicleName", playerGarage.VehicleName, DbType.String,
                                            ParameterDirection.Input);
                                        parameter.Add("@GarageContent", garageContent ?? string.Empty, DbType.String,
                                            ParameterDirection.Input);
                                        var insertQuery =
                                            $"INSERT INTO `{MySql_TableName}` (`Id`, `SteamId`, `VehicleName`, `GarageContent`) " +
                                            "VALUES (@Id, @SteamId, @VehicleName, @GarageContent);";
                                        await Dapper.SqlMapper.ExecuteAsync(connection, insertQuery, parameter);
                                    }
                                }

                                break;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(to), to, null);
                        }

                        break;
                    case EDatabase.JSON:
                        Json_DataStore = new JsonDataStore<List<PlayerGarage>>(Plugin.Inst.Directory, Json_FileName);
                        JSON_Reload(true);
                        switch (to)
                        {
                            case EDatabase.LITEDB:
                                using (var db =
                                       new LiteDB.Async.LiteDatabaseAsync(DatabaseManager.LiteDB_ConnectionString))
                                {
                                    var col = db.GetCollection<PlayerGarage>(LiteDB_TableName);
                                    await col.DeleteAllAsync();
                                    await col.InsertBulkAsync(MigrateCollection);
                                }

                                break;
                            case EDatabase.MYSQL:
                                MySQL_CreateTable(MySql_TableName, MySql_CreateTableQuery);
                                using (var connection =
                                       new MySql.Data.MySqlClient.MySqlConnection(
                                           DatabaseManager.MySql_ConnectionString))
                                {
                                    var deleteQuery = $"DELETE FROM `{MySql_TableName}`;";
                                    await Dapper.SqlMapper.ExecuteAsync(connection, deleteQuery);

                                    foreach (var playerGarage in MigrateCollection)
                                    {
                                        var serialized = playerGarage.GarageContent.Serialize();
                                        var garageContent = serialized.ToBase64();
                                        var parameter = new Dapper.DynamicParameters();
                                        parameter.Add("@Id", playerGarage.Id, DbType.Int32, ParameterDirection.Input);
                                        parameter.Add("@SteamId", playerGarage.SteamId, DbType.String,
                                            ParameterDirection.Input);
                                        parameter.Add("@VehicleName", playerGarage.VehicleName, DbType.String,
                                            ParameterDirection.Input);
                                        parameter.Add("@GarageContent", garageContent ?? string.Empty, DbType.String,
                                            ParameterDirection.Input);
                                        var insertQuery =
                                            $"INSERT INTO `{MySql_TableName}` (`Id`, `SteamId`, `VehicleName`, `GarageContent`) " +
                                            "VALUES (@Id, @SteamId, @VehicleName, @GarageContent);";
                                        await Dapper.SqlMapper.ExecuteAsync(connection, insertQuery, parameter);
                                    }
                                }

                                break;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(to), to, null);
                        }

                        break;
                    case EDatabase.MYSQL:
                        MigrateCollection = await MySQL_LoadAllAsync();
                        switch (to)
                        {
                            case EDatabase.LITEDB:
                                using (var db =
                                       new LiteDB.Async.LiteDatabaseAsync(DatabaseManager.LiteDB_ConnectionString))
                                {
                                    var col = db.GetCollection<PlayerGarage>(LiteDB_TableName);
                                    await col.DeleteAllAsync();
                                    await col.InsertBulkAsync(MigrateCollection);
                                }

                                break;
                            case EDatabase.JSON:
                                Json_DataStore =
                                    new JsonDataStore<List<PlayerGarage>>(Plugin.Inst.Directory, Json_FileName);
                                await Json_DataStore.SaveAsync(MigrateCollection);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(to), to, null);
                        }

                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(from), from, null);
                }

                MigrateCollection.Clear();
                MigrateCollection.TrimExcess();
            }
            catch (Exception e)
            {
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] GarageManager MigrateAsync: {e.Message}");
                Logger.LogError($"[{Plugin.Inst.Name}] [ERROR] Details: {e}");
            }
        }
    }
}