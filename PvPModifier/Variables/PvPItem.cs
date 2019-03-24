using PvPModifier.DataStorage;
using PvPModifier.Utilities;
using Terraria;

namespace PvPModifier.Variables {
    public class PvPItem : Item {
        public double Damage;
        public float Knockback;

        public PvPItem() {
            SetDefaults();
        }

        public PvPItem(Item item) {
            SetDefaults(item.type);
            prefix = item.prefix;
            Damage = item.damage;
            Knockback = item.knockBack;
        }

        public PvPItem(int type) {
            SetDefaults(type);
            Damage = damage;
            Knockback = knockBack;
        }

        /// <summary>
        /// Gets damage based off server config.
        /// </summary>
        public int ConfigDamage => Cache.Items[type].Damage;

        /// <summary>
        /// Gets the projectile shot by an item.
        /// </summary>
        public PvPProjectile Shoot => new PvPProjectile(Cache.Items[type].Shoot);

        /// <summary>
        /// Gets the knockback of an item.
        /// </summary>
        public float GetKnockback(PvPPlayer owner) => owner.TPlayer.GetWeaponKnockback(this, Cache.Items[type].Knockback);
    }
}
