using PvPModifier.Utilities.PvPConstants;

namespace PvPModifier.DataStorage {
    public class Cache {
        public static DbItem[] Items = new DbItem[Terraria.Main.maxItemTypes + 1];
        public static DbProjectile[] Projectiles = new DbProjectile[Terraria.Main.maxProjectileTypes + 1];
        public static DbBuff[] Buffs = new DbBuff[Terraria.Main.maxBuffTypes + 1];

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
    }
}
