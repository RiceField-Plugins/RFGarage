using System;
using System.IO;
using RFGarage.Enums;
using RFRocketLibrary.API.Interfaces;
using RFRocketLibrary.Models;
using Rocket.Core.Logging;

namespace RFGarage.DatabaseManagers
{
    public static class DatabaseManager
    {
        private static readonly string LiteDB_FileName = "garage.db";
        internal static readonly string LiteDB_FilePath = Path.Combine(Plugin.Inst.Directory, LiteDB_FileName);
        internal static readonly string LiteDB_ConnectionString = $"Filename={LiteDB_FilePath};Connection=shared;";
        
        internal static string MySql_ConnectionString;
        internal static string MySql_TableName;
        
        internal static ISerialQueue Queue;
        
        internal static void Initialize()
        {
            try
            {
                Queue = new SerialQueue();
                if (Plugin.Conf.Database == EDatabase.MYSQL)
                {
                    var index = Plugin.Conf.MySqlConnectionString.LastIndexOf("TABLENAME", StringComparison.Ordinal);
                    if (index == -1)
                    {
                        MySql_TableName = "rfgarage";
                        MySql_ConnectionString = Plugin.Conf.MySqlConnectionString;
                    }
                    else
                    {
                        var substr = Plugin.Conf.MySqlConnectionString.Substring(
                            Plugin.Conf.MySqlConnectionString.LastIndexOf('='));
                        MySql_TableName = substr.Substring(1, substr.Length - 2);
                        MySql_ConnectionString = Plugin.Conf.MySqlConnectionString.Remove(index);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.LogError("[RFGarage] [ERROR] DatabaseManager Initializing: " + e.Message);
                Logger.LogError("[RFGarage] [ERROR] Details: " + e);
            }
        }
    }
}