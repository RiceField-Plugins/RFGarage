using System;
using System.Collections.Generic;
using System.Linq;
using I18N.West;
using MySql.Data.MySqlClient;
using RFGarage.Models;
using Rocket.Core.Logging;

namespace RFGarage.DatabaseManagers
{
    public class MySqlDb
    {
        internal const string CreateTableQuery = 
            "`EntryID` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT, " + 
            "`SteamID` VARCHAR(32) NOT NULL DEFAULT '0', " + 
            "`GarageName` VARCHAR(32) NOT NULL DEFAULT 'Default', " + 
            "`VehicleName` VARCHAR(32) NOT NULL DEFAULT 'NoName', " + 
            "`Info` TEXT NOT NULL, " + 
            "`LastUpdated` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP," + 
            "PRIMARY KEY (EntryID)";

        public string Address;
        public string Name;
        public string Password;
        public uint Port;
        public string TableName;
        public string Username;
        
        // CONSTRUCTOR   
        public MySqlDb(string address, uint port, string username, string password, string name, string tableName, string createTableQuery)
        {
            Address = address;
            Port = port;
            Username = username;
            Password = password;
            Name = name;
            TableName = tableName;
            
            var cp1250 = new CP1250();
            CreateTableSchema(createTableQuery);
        }

        // METHODS
        private MySqlConnection CreateConnection()
        {
            MySqlConnection connection = null;
            try
            {
                if (Port == 0)
                    Port = 3306;
                connection = new MySqlConnection(
                    $"SERVER={Address};DATABASE={Name};UID={Username};PASSWORD={Password};PORT={Port};");
            }
            catch (Exception ex)
            {
                Logger.LogError("[RFGarage] DbError: " + ex);
            }

            return connection;
        }
        private void CreateTableSchema(string createTableQuery)
        {
            ExecuteQuery(EQueryType.NonQuery,
                $"CREATE TABLE IF NOT EXISTS `{TableName}` ({createTableQuery});");
        }
        public object ExecuteQuery(EQueryType queryType, string query, params MySqlParameter[] parameters)
        {
            object result = null;
            MySqlDataReader reader = null;

            using (var connection = CreateConnection())
            {
                try
                {
                    var command = connection.CreateCommand();
                    command.CommandText = query;

                    foreach (var parameter in parameters)
                        command.Parameters.Add(parameter);

                    connection.Open();
                    switch (queryType)
                    {
                        case EQueryType.Reader:
                            var readerResult = new List<Row>();

                            reader = command.ExecuteReader();
                            while (reader.Read())
                                try
                                {
                                    var values = new Dictionary<string, object>();

                                    for (var i = 0; i < reader.FieldCount; i++)
                                    {
                                        var columnName = reader.GetName(i);
                                        values.Add(columnName, reader[columnName]);
                                    }

                                    readerResult.Add(new Row { Values = values });
                                }
                                catch (Exception ex)
                                {
                                    Logger.LogError(
                                        $"The following query threw an error during reader execution:\nQuery: \"{query}\"\nError: {ex.Message}");
                                }

                            result = readerResult;
                            break;
                        case EQueryType.Scalar:
                            result = command.ExecuteScalar();
                            break;
                        case EQueryType.NonQuery:
                            result = command.ExecuteNonQuery();
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(queryType), queryType, null);
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError("[RFGarage] DbError: " + ex);
                }
                finally
                {
                    reader?.Close();
                    connection.Close();
                }
            }

            return result;
        }
        public bool IsDataExist(string tableName, string data, string column)
        {
            var scalar = ExecuteQuery(EQueryType.Scalar,
                $"SELECT * FROM `{tableName}` WHERE {column} = @data;",
                new MySqlParameter("@data", data));
            return scalar != null;
        }
        public void DeleteData(string tableName, string data, string column)
        {
            ExecuteQuery(EQueryType.NonQuery,
                $"DELETE FROM `{tableName}` WHERE {column}=@data;", 
                new MySqlParameter("@data", data));
        }
        public void DeleteVgVehicle(ulong entryID)
        {
            ExecuteQuery(EQueryType.NonQuery, $"DELETE FROM `{TableName}` WHERE EntryID=@entryID;", 
                new MySqlParameter("@entryID", entryID));
        }
        public object GetData(string tableName, string data, string column, string selectedColumn)
        {
            var result = ExecuteQuery(EQueryType.Scalar,
                $"SELECT {selectedColumn} FROM `{tableName}` WHERE {column} = @data;",
                new MySqlParameter("@data", data));

            return result;
        }
        public uint GetVehicleCount(string steamID, string garageName)
        {
            var result = ExecuteQuery(EQueryType.Scalar,
                $"SELECT COUNT(*) FROM `{TableName}` WHERE SteamID = @steamID AND GarageName = @garageName;",
                new MySqlParameter("@steamID", steamID), new MySqlParameter("@garageName", garageName));
            uint.TryParse(result.ToString(), out var count);
            return count;
        }
        public bool HasVehicle(string steamID)
        {
            var readerResult = (List<Row>)ExecuteQuery(EQueryType.Reader,
                $"SELECT * FROM `{TableName}` WHERE SteamID = @steamID;",
                new MySqlParameter("@steamID", steamID));

            return readerResult.Count > 0;
        }
        public bool HasVehicle(string steamID, string vehicleName)
        {
            var readerResult = (List<Row>)ExecuteQuery(EQueryType.Reader,
                $"SELECT * FROM `{TableName}` WHERE SteamID = @steamID AND VehicleName = @vehicleName;",
                new MySqlParameter("@steamID", steamID), new MySqlParameter("@vehicleName", vehicleName));

            return readerResult.Count != 0;
        }
        public bool IsVehicleExist(string steamID, string garageName)
        {
            var scalar = ExecuteQuery(EQueryType.Scalar,
                $"SELECT * FROM `{TableName}` WHERE SteamID = @steamID AND GarageName = @garageName;",
                new MySqlParameter("@steamID", steamID), new MySqlParameter("@garageName", garageName));

            return scalar != null;
        }
        public bool IsVehicleExist(string steamID, string garageName, string vehicleName)
        {
            var scalar = ExecuteQuery(EQueryType.Scalar,
                $"SELECT * FROM `{TableName}` WHERE SteamID = @steamID AND GarageName = @garageName AND VehicleName = @vehicleName;",
                new MySqlParameter("@steamID", steamID), new MySqlParameter("@garageName", garageName), 
                new MySqlParameter("@vehicleName", vehicleName));

            return scalar != null;
        }
        public bool IsGarageFull(string steamID, GarageModel garageModel)
        {
            var readerResult = (List<Row>)ExecuteQuery(EQueryType.Reader,
                $"SELECT * FROM `{TableName}` WHERE SteamID = @steamID AND GarageName = @garageName;",
                new MySqlParameter("@steamID", steamID), new MySqlParameter("@garageName", garageModel.Name));

            return readerResult.Count < garageModel.Slot;
        }
        public void InsertVgVehicle(string steamID, string garageName, string vehicleName, string info)
        {
            ExecuteQuery(EQueryType.NonQuery,
                $"INSERT INTO `{TableName}` (SteamID,GarageName,VehicleName,Info) VALUES(@steamID,@garageName,@vehicleName,@info);", 
                new MySqlParameter("@steamID", steamID), new MySqlParameter("@garageName", garageName),
                new MySqlParameter("@vehicleName", vehicleName), new MySqlParameter("@info", info));
        }
        public object ReadData(string tableName)
        {
            var readerResult = (List<Row>)ExecuteQuery(EQueryType.Reader, $"SELECT * FROM `{tableName}`;");

            return readerResult;
        }
        public PlayerSerializableVehicleModel ReadVgVehicleByVehicleName(string steamID, string garageName, string vehicleName)
        {
            var readerResult = (List<Row>)ExecuteQuery(EQueryType.Reader,
                $"SELECT * FROM `{TableName}` WHERE SteamID = @steamID AND GarageName = @garageName AND VehicleName = @vehicleName;",
                new MySqlParameter("@steamID", steamID), new MySqlParameter("@garageName", garageName),
                new MySqlParameter("@vehicleName", vehicleName));

            return readerResult?.Select(r => new PlayerSerializableVehicleModel
            {
                EntryID = ulong.Parse(r.Values["EntryID"].ToString()),
                SteamID = ulong.Parse(r.Values["SteamID"].ToString()),
                GarageName = r.Values["GarageName"].ToString(),
                VehicleName = r.Values["VehicleName"].ToString(),
                Info = r.Values["Info"].ToString(),
            }).FirstOrDefault();
        }
        public IEnumerable<PlayerSerializableVehicleModel> ReadVgVehicleByGarageName(string steamID, string garageName)
        {
            var readerResult = (List<Row>)ExecuteQuery(EQueryType.Reader,
                $"SELECT * FROM `{TableName}` WHERE SteamID = @steamID AND GarageName = @garageName;",
                new MySqlParameter("@steamID", steamID), new MySqlParameter("@garageName", garageName));

            return readerResult?.Select(r => new PlayerSerializableVehicleModel
            {
                EntryID = ulong.Parse(r.Values["EntryID"].ToString()),
                SteamID = ulong.Parse(r.Values["SteamID"].ToString()),
                GarageName = r.Values["GarageName"].ToString(),
                VehicleName = r.Values["VehicleName"].ToString(),
                Info = r.Values["Info"].ToString(),
            });
        }
        public IEnumerable<PlayerSerializableVehicleModel> ReadVgVehicleAllWithoutDrown(string steamID)
        {
            var readerResult = (List<Row>)ExecuteQuery(EQueryType.Reader,
                $"SELECT * FROM `{TableName}` WHERE SteamID = @steamID AND GarageName <> 'Drown';",
                new MySqlParameter("@steamID", steamID));

            return readerResult?.Select(r => new PlayerSerializableVehicleModel
            {
                EntryID = ulong.Parse(r.Values["EntryID"].ToString()),
                SteamID = ulong.Parse(r.Values["SteamID"].ToString()),
                GarageName = r.Values["GarageName"].ToString(),
                VehicleName = r.Values["VehicleName"].ToString(),
                Info = r.Values["Info"].ToString(),
            });
        }
        public void UpdateData(string tableName, string oldData, string data, string column)
        {
            ExecuteQuery(EQueryType.NonQuery,
                $"UPDATE `{tableName}` SET {oldData}=@newData WHERE {column}=@{oldData};", 
                new MySqlParameter("@newData", data));
        }
    }

    public enum EQueryType
    {
        NonQuery,
        Reader,
        Scalar,
    }

    public class Row
    {
        public Dictionary<string, object> Values;
    }
}