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
        /// </summary>
        public int ConfigDamage => Cache.Items[type].Damage;

        /// <summary>
        /// Gets the knockback of an item.
        /// </summary>
        public float GetKnockback(PvPPlayer owner) => owner.TPlayer.GetWeaponKnockback(this, Cache.Items[type].Knockback);
    }
}
