using PvPModifier.Utilities.PvPConstants;

namespace PvPModifier.DataStorage {
    /// <summary>
    /// Stores the current values of Items, Projectiles, and Buffs retrieved from
    /// the SQL database and stores it in this object.
    /// </summary>
    public class Cache {
        public static DbItem[] Items = new DbItem[Terraria.Main.maxItemTypes + 1];
        public static DbProjectile[] Projectiles = new DbProjectile[Terraria.Main.maxProjectileTypes + 1];
        public static DbBuff[] Buffs = new DbBuff[Terraria.Main.maxBuffTypes + 1];

        /// <summary>
        /// Gets the <see cref="DbObject"/> from the section and ID.
        /// </summary>
        /// <param name="section">Item, Projectile, or Buff</param>
        /// <param name="id">The numerical ID of the object</param>
        /// <returns></returns>
        public static DbObject GetSection(string section, int id) {
            switch (section) {
                case DbTables.ItemTable:
                    if (id >= 0 && id <= Terraria.Main.maxItemTypes)
                        return Items[id];
                    break;
                case DbTables.ProjectileTable:
                    if (id >= 0 && id <= Terraria.Main.maxProjectileTypes)
                        return Projectiles[id];
                    break;
                case DbTables.BuffTable:
                    if (id >= 0 && id <= Terraria.Main.maxBuffTypes)
                        return Buffs[id];
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
