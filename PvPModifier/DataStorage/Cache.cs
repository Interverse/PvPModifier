namespace PvPModifier.DataStorage {
    public class Cache {
        public static DbItem[] Items = new DbItem[Terraria.Main.maxItemTypes + 1];
        public static DbProjectile[] Projectiles = new DbProjectile[Terraria.Main.maxProjectileTypes + 1];
        public static DbBuff[] Buffs = new DbBuff[Terraria.Main.maxBuffTypes + 1];
    }
}
