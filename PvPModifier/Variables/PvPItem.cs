using PvPModifier.DataStorage;
using Terraria;

namespace PvPModifier.Variables {
    public class PvPItem : Item {
        public PvPItem() {
            SetDefaults();
        }

        public PvPItem(Item item) {
            SetDefaults(item.type);
            prefix = item.prefix;
        }

        public PvPItem(int type) {
            SetDefaults(type);
        }

        /// <summary>
        /// Gets damage based off server config.
        /// Returns the base damage of an item if the config damage value is -1
        /// </summary>
        public int ConfigDamage {
            get {
                var configDamage = Cache.Items[type].Damage;
                if (configDamage == -1) {
                    return base.damage;
                }

                return configDamage;
            }
        }

        /// <summary>
        /// Gets the knockback of an item from the player's stats.
        /// </summary>
        public float GetKnockback(PvPPlayer owner) => owner.TPlayer.GetWeaponKnockback(this, Cache.Items[type].Knockback);
    }
}
