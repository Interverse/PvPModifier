using PvPModifier.DataStorage;
using Terraria;
using TShockAPI;

namespace PvPModifier.Utilities.Extensions {
    public static class ItemExtension {
        /// <summary>
        /// Gets damage based off server config.
        /// Returns the base damage of an item if the config damage value is -1
        /// </summary>
        public static int GetConfigDamage(this Item item) {
            var configDamage = Cache.Items[item.type].Damage;
            if (configDamage == -1) {
                return item.damage;
            }

            return configDamage;
        }

        /// <summary>
        /// Gets the knockback of an item from the player's stats.
        /// </summary>
        public static float GetKnockback(this Item item, TSPlayer owner) => owner.TPlayer.GetWeaponKnockback(item, Cache.Items[item.type].Knockback);
    }
}
