using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using Mono.Data.Sqlite;
using MySql.Data.MySqlClient;
using PvPModifier.Utilities;
using PvPModifier.Utilities.PvPConstants;
using PvPModifier.Variables;
using Terraria;
using TShockAPI;
using TShockAPI.DB;

namespace PvPModifier.DataStorage {
    public static class Database {
        public static bool IsMySql => db.GetSqlType() == SqlType.Mysql;

        public static IDbConnection db;

        /// <summary>
        /// Connects the mysql/sqlite database for the plugin, creating one if the database doesn't already exist.
        /// </summary>
        public static void ConnectDB() {
            if (TShock.Config.StorageType.ToLower() == "sqlite")
                db = new SqliteConnection(string.Format("uri=file://{0},Version=3",
                    Path.Combine(TShock.SavePath, "PvPModifier.sqlite")));
            else if (TShock.Config.StorageType.ToLower() == "mysql") {
                try {
                    var host = TShock.Config.MySqlHost.Split(':');
                    db = new MySqlConnection {
                        ConnectionString = string.Format("Server={0}; Port={1}; Database={2}; Uid={3}; Pwd={4}",
                            host[0],
                            host.Length == 1 ? "3306" : host[1],
                            TShock.Config.MySqlDbName,
                            TShock.Config.MySqlUsername,
                            TShock.Config.MySqlPassword)
                    };
                } catch (MySqlException x) {
                    TShock.Log.Error(x.ToString());
                    throw new Exception("MySQL not setup correctly.");
                }
            } else
                throw new Exception("Invalid storage type.");

            var sqlCreator = new SqlTableCreator(db,
                IsMySql
                    ? (IQueryBuilder)new MysqlQueryCreator()
                    : new SqliteQueryCreator());

            sqlCreator.EnsureTableStructure(new SqlTable(DbTables.ItemTable,
                new SqlColumn(DbConsts.ID, MySqlDbType.Int32) {Primary = true},
                new SqlColumn(DbConsts.Damage, MySqlDbType.Int32),
                new SqlColumn(DbConsts.Knockback, MySqlDbType.Float),
                new SqlColumn(DbConsts.UseAnimation, MySqlDbType.Int32),
                new SqlColumn(DbConsts.UseTime, MySqlDbType.Int32),
                new SqlColumn(DbConsts.Shoot, MySqlDbType.Int32),
                new SqlColumn(DbConsts.ShootSpeed, MySqlDbType.Float),
                new SqlColumn(DbConsts.AmmoIdentifier, MySqlDbType.Int32),
                new SqlColumn(DbConsts.UseAmmoIdentifier, MySqlDbType.Int32),
                new SqlColumn(DbConsts.NotAmmo, MySqlDbType.Int32),
                new SqlColumn(DbConsts.InflictBuffID, MySqlDbType.Int32),
                new SqlColumn(DbConsts.InflictBuffDuration, MySqlDbType.Int32),
                new SqlColumn(DbConsts.ReceiveBuffID, MySqlDbType.Int32),
                new SqlColumn(DbConsts.ReceiveBuffDuration, MySqlDbType.Int32),
                new SqlColumn(DbConsts.HomingRadius, MySqlDbType.Float),
                new SqlColumn(DbConsts.AngularVelocity, MySqlDbType.Float),
                new SqlColumn(DbConsts.Mirror, MySqlDbType.Int32),
                new SqlColumn(DbConsts.Spread, MySqlDbType.Float),
                new SqlColumn(DbConsts.RandomSpread, MySqlDbType.Int32),
                new SqlColumn(DbConsts.NumShots, MySqlDbType.Int32)));

            sqlCreator.EnsureTableStructure(new SqlTable(DbTables.ProjectileTable,
                new SqlColumn(DbConsts.ID, MySqlDbType.Int32) {Primary = true},
                new SqlColumn(DbConsts.Shoot, MySqlDbType.Int32),
                new SqlColumn(DbConsts.VelocityMultiplier, MySqlDbType.Float),
                new SqlColumn(DbConsts.Damage, MySqlDbType.Int32),
                new SqlColumn(DbConsts.InflictBuffID, MySqlDbType.Int32),
                new SqlColumn(DbConsts.InflictBuffDuration, MySqlDbType.Int32),
                new SqlColumn(DbConsts.ReceiveBuffID, MySqlDbType.Int32),
                new SqlColumn(DbConsts.ReceiveBuffDuration, MySqlDbType.Int32)));

           sqlCreator.EnsureTableStructure(new SqlTable(DbTables.BuffTable,
                new SqlColumn(DbConsts.ID, MySqlDbType.Int32) { Primary = true },
                new SqlColumn(DbConsts.InflictBuffID, MySqlDbType.Int32),
                new SqlColumn(DbConsts.InflictBuffDuration, MySqlDbType.Int32),
                new SqlColumn(DbConsts.ReceiveBuffID, MySqlDbType.Int32),
                new SqlColumn(DbConsts.ReceiveBuffDuration, MySqlDbType.Int32)));
        }
        
        /// <summary>
        /// Returns data from the SQL database from a given SQL query.
        /// </summary>
        public static QueryResult QueryReader(string query) {
            return db.QueryReader(query);
        }

        /// <summary>
        /// Performs an SQL query
        /// </summary>
        /// <param name="query">The SQL statement to be ran.</param>
        /// <returns>
        /// Returns true if the statement was successful.
        /// Returns false if the statement failed.
        /// </returns>
        public static bool Query(string query) {
            bool success = true;
            db.Open();
            try {
                using (var conn = db.CreateCommand()) {
                    conn.CommandText = query;
                    conn.ExecuteNonQuery();
                }
            } catch (Exception e) {
                TShock.Log.Write(e.ToString(), TraceLevel.Error);
                success = false;
            }
            
            db.Close();
            return success;
        }

        /// <summary>
        /// Deletes the contents of an entire row.
        /// </summary>
        /// <param Name="table">The table to delete from</param>
        /// <param Name="id">The ID of the data being deleted</param>
        public static void DeleteRow(string table, int id) {
            Query("DELETE FROM {0} WHERE ID = {1}".SFormat(table, id));
        }

        /// <summary>
        /// Performs a series of SQL statements in a transaction.
        /// This allows for fast mass querying which is faster than
        /// querying one statement at a time.
        /// </summary>
        /// <param name="queries"></param>
        public static void PerformTransaction(string[] queries) {
            var conn = IsMySql
                ? (DbConnection)new MySqlConnection(db.ConnectionString)
                : new SqliteConnection(db.ConnectionString);

            conn.Open();

            using (var cmd = conn.CreateCommand()) {
                using (var transaction = conn.BeginTransaction()) {
                    foreach (string query in queries) {
                        cmd.CommandText = query;
                        cmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
            }

            conn.Close();
        }

        /// <summary>
        /// Writes the changed attribute of an item to the sql database.
        /// </summary>
        public static bool Update<T>(string table, int index, string column, T value) {
            bool selectAll = index < 0;

            if (value is string) value = (T)Convert.ChangeType(value.ToString().SqlString(), typeof(T));

            string sourceId = !selectAll ? " WHERE ID = {0}".SFormat(index) : "";
            return Query(string.Format("UPDATE {0} SET {1} = {2}{3}", table, column, value, sourceId));
        }

        /// <summary>
        /// Creates all the default stats of items, projectiles, and buffs and puts it into
        /// the mysql/sqlite database.
        /// </summary>
        public static void InitDefaultTables() {
            List<string> queries = new List<string>();

            var tableList = new[] { DbTables.ItemTable, DbTables.ProjectileTable, DbTables.BuffTable };
            foreach (string table in tableList) {
                queries.Add("DELETE FROM {0}".SFormat(table));
            }

            for (int x = 0; x < Main.maxItemTypes; x++) {
                queries.Add(GetDefaultValueSqlString(DbTables.ItemTable, x));
            }

            for (int x = 0; x < Main.maxProjectileTypes; x++) {
                queries.Add(GetDefaultValueSqlString(DbTables.ProjectileTable, x));
            }

            for (int x = 0; x < Main.maxBuffTypes; x++) {
                queries.Add(GetDefaultValueSqlString(DbTables.BuffTable, x));
            }

            PerformTransaction(queries.ToArray());
        }

        /// <summary>
        /// Gets the default values of an item, projectile, or buff and
        /// puts it into an sql query form.
        /// </summary>
        /// <returns>The default values in an sql statement</returns>
        public static string GetDefaultValueSqlString(string table, int id) {
            var inflictBuff = new BuffInfo();
            var receiveBuff = new BuffInfo();

            switch (table) {
                case "Items":
                    Item item = new Item();
                    item.SetDefaults(id);
                    
                    float knockback = item.knockBack;
                    bool notAmmo = item.notAmmo;

                    return "INSERT INTO {0} ({1}) VALUES ({2})"
                        .SFormat(DbTables.ItemTable,
                            string.Join(", ", DbConsts.ID, DbConsts.Damage, DbConsts.Knockback, 
                                              DbConsts.UseAnimation, DbConsts.UseTime, DbConsts.Shoot, 
                                              DbConsts.ShootSpeed, DbConsts.AmmoIdentifier, DbConsts.UseAmmoIdentifier, 
                                              DbConsts.NotAmmo, DbConsts.InflictBuffID, DbConsts.InflictBuffDuration, 
                                              DbConsts.ReceiveBuffID, DbConsts.ReceiveBuffDuration, DbConsts.HomingRadius, 
                                              DbConsts.AngularVelocity, DbConsts.Mirror, DbConsts.Spread,
                                              DbConsts.RandomSpread, DbConsts.NumShots),
                            string.Join(", ", id, -1, knockback, 
                                              -1, -1, -1, 
                                              -1, -1, -1, 
                                              notAmmo.ToInt(), inflictBuff.BuffId, inflictBuff.BuffDuration, 
                                              receiveBuff.BuffId, receiveBuff.BuffDuration, -1, 
                                              -1, -1, -1,
                                              -1, -1));

                case "Projectiles":
                    inflictBuff = PresetData.ProjectileDebuffs.ContainsKey(id)
                        ? PresetData.ProjectileDebuffs[id]
                        : new BuffInfo();

                    return "INSERT INTO {0} ({1}) VALUES ({2})"
                        .SFormat(DbTables.ProjectileTable,
                            string.Join(", ", DbConsts.ID, DbConsts.Shoot, DbConsts.VelocityMultiplier, 
                                              DbConsts.Damage, DbConsts.InflictBuffID, DbConsts.InflictBuffDuration, 
                                              DbConsts.ReceiveBuffID, DbConsts.ReceiveBuffDuration),
                            string.Join(", ", id, id, 1, 
                                              -1, inflictBuff.BuffId, inflictBuff.BuffDuration, 
                                              receiveBuff.BuffId, receiveBuff.BuffDuration));

                case "Buffs":
                    inflictBuff = PresetData.FlaskDebuffs.ContainsKey(id)
                        ? PresetData.FlaskDebuffs[id]
                        : new BuffInfo();

                    return "INSERT INTO {0} ({1}) VALUES ({2})"
                        .SFormat(DbTables.BuffTable,
                            string.Join(", ", DbConsts.ID, DbConsts.InflictBuffID, DbConsts.InflictBuffDuration, 
                                              DbConsts.ReceiveBuffID, DbConsts.ReceiveBuffDuration),
                            string.Join(", ", id, inflictBuff.BuffId, inflictBuff.BuffDuration, 
                                              receiveBuff.BuffId, receiveBuff.BuffDuration));

                default:
                    return "";
            }
        }

        public static void LoadDatabase() {
            using (var reader = QueryReader($"SELECT * FROM {DbTables.ItemTable}")) {
                while (reader.Read()) {
                    int id = reader.Get<int>(DbConsts.ID);
                    Cache.Items[id] = new DbItem {
                        ID = id,
                        Damage = reader.Get<int>(DbConsts.Damage),
                        Knockback = reader.Get<float>(DbConsts.Knockback),
                        UseAnimation = reader.Get<int>(DbConsts.UseAnimation),
                        UseTime = reader.Get<int>(DbConsts.UseTime),
                        Shoot = reader.Get<int>(DbConsts.Shoot),
                        ShootSpeed = reader.Get<float>(DbConsts.ShootSpeed),
                        AmmoIdentifier = reader.Get<int>(DbConsts.AmmoIdentifier),
                        UseAmmoIdentifier = reader.Get<int>(DbConsts.UseAmmoIdentifier),
                        NotAmmo = reader.Get<int>(DbConsts.NotAmmo),
                        InflictBuffID = reader.Get<int>(DbConsts.InflictBuffID),
                        InflictBuffDuration = reader.Get<int>(DbConsts.InflictBuffDuration),
                        ReceiveBuffID = reader.Get<int>(DbConsts.ReceiveBuffDuration),
                        ReceiveBuffDuration = reader.Get<int>(DbConsts.ReceiveBuffDuration),
                        HomingRadius = reader.Get<float>(DbConsts.HomingRadius),
                        AngularVelocity = reader.Get<float>(DbConsts.AngularVelocity),
                        Mirror = reader.Get<int>(DbConsts.Mirror),
                        Spread = reader.Get<float>(DbConsts.Spread),
                        RandomSpread = reader.Get<int>(DbConsts.RandomSpread),
                        NumShots = reader.Get<int>(DbConsts.NumShots)
                    };
                }
            }

            using (var reader = QueryReader($"SELECT * FROM {DbTables.ProjectileTable}")) {
                while (reader.Read()) {
                    int id = reader.Get<int>(DbConsts.ID);
                    Cache.Projectiles[id] = new DbProjectile {
                        ID = id,
                        Shoot = reader.Get<int>(DbConsts.Shoot),
                        VelocityMultiplier = reader.Get<float>(DbConsts.VelocityMultiplier),
                        Damage = reader.Get<int>(DbConsts.Damage),
                        InflictBuffID = reader.Get<int>(DbConsts.InflictBuffID),
                        InflictBuffDuration = reader.Get<int>(DbConsts.InflictBuffDuration),
                        ReceiveBuffID = reader.Get<int>(DbConsts.ReceiveBuffDuration),
                        ReceiveBuffDuration = reader.Get<int>(DbConsts.ReceiveBuffDuration)
                    };
                }
            }

            using (var reader = QueryReader($"SELECT * FROM {DbTables.BuffTable}")) {
                while (reader.Read()) {
                    var id = reader.Get<int>(DbConsts.ID);
                    Cache.Buffs[id] = new DbBuff {
                        ID = id,
                        InflictBuffID = reader.Get<int>(DbConsts.InflictBuffID),
                        InflictBuffDuration = reader.Get<int>(DbConsts.InflictBuffDuration),
                        ReceiveBuffID = reader.Get<int>(DbConsts.ReceiveBuffDuration),
                        ReceiveBuffDuration = reader.Get<int>(DbConsts.ReceiveBuffDuration)
                    };
                }
            }
        }
    }
}
