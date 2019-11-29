using PvPModifier.Utilities.PvPConstants;
using System.Collections.Generic;

namespace PvPModifier.DataStorage {
    /// <summary>
    /// Stores the current values of Items, Projectiles, and Buffs retrieved from
    /// the SQL database and stores it in this object.
    /// </summary>
    public class Cache {
        private static Dictionary<int, DbItem> Items = new Dictionary<int, DbItem>();
        private static Dictionary<int, DbProjectile> Projectiles = new Dictionary<int, DbProjectile>();
        private static Dictionary<int, DbBuff> Buffs = new Dictionary<int, DbBuff>();

        /// <summary>
        /// Gets the <see cref="DbObject"/> from the section and ID.
        /// </summary>
        /// <param name="section">Item, Projectile, or Buff</param>
        /// <param name="id">The numerical ID of the object</param>
        /// <returns></returns>
        public static DbObject GetDbObject(string section, int id) {
            switch (section) {
                case DbTables.ItemTable:
                    if (id >= 0 && id <= Terraria.Main.maxItemTypes) {
                        if (!Items.ContainsKey(id)) {
                            Items[id] = (DbItem)Database.GetObject(section, id);
                        }
                        return Items[id];
                    }
                    break;
                case DbTables.ProjectileTable:
                    if (id >= 0 && id <= Terraria.Main.maxProjectileTypes) {
                        if (!Projectiles.ContainsKey(id)) {
                            Projectiles[id] = (DbProjectile)Database.GetObject(section, id);
                        }
                        return Projectiles[id];
                    }
                    break;
                case DbTables.BuffTable:
                    if (id >= 0 && id <= Terraria.Main.maxBuffTypes) {
                        if (!Buffs.ContainsKey(id)) {
                            Buffs[id] = (DbBuff)Database.GetObject(section, id);
                        }
                        return Buffs[id];
                    }
                    break;
            }

            return null;
        }

        public static void Clear() {
            Items.Clear();
            Projectiles.Clear();
            Buffs.Clear();
        }

        public static DbObject Load(string section, int id) {
            switch (section) {
                case DbTables.ItemTable:
                    if (id >= 0 && id <= Terraria.Main.maxItemTypes) {
                            Items[id] = (DbItem)Database.GetObject(section, id);
                    }
                    break;
                case DbTables.ProjectileTable:
                    if (id >= 0 && id <= Terraria.Main.maxProjectileTypes) {
                        Projectiles[id] = (DbProjectile)Database.GetObject(section, id);
                    }
                    break;
                case DbTables.BuffTable:
                    if (id >= 0 && id <= Terraria.Main.maxBuffTypes) {
                        Buffs[id] = (DbBuff)Database.GetObject(section, id);
                    }
                    break;
            }

            return null;
        }

        /// <summary>
        /// Ensures that no one can instantiate the Cache object.
        /// </summary>
        private Cache() { }
    }
}
