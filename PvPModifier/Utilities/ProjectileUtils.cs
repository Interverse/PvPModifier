using System.Linq;
using PvPModifier.Variables;
using Terraria;

namespace PvPModifier.Utilities {
    public class ProjectileUtils {
        /// <summary>
        /// Gets the weapon of a projectile.
        /// For certain projectiles, it will pull from a list of
        /// projectile-to-weapon Dictionaries and returns
        /// the weapon based off the dictionary mapping.
        /// </summary>
        /// <param name="owner">Index of the owner of projectile.</param>
        /// <param name="type">Type of projectile.</param>
        /// <returns>Returns the item the projectile came from.</returns>
        public static PvPItem GetProjectileWeapon(PvPPlayer owner, int type) {
            PvPItem weapon;
            if (PresetData.PresetProjDamage.ContainsKey(type)) {
                weapon = new PvPItem();
            } else if (PresetData.ProjHooks.ContainsKey(type)) {
                weapon = owner.FindPlayerItem(PresetData.ProjHooks[type]);
            } else if (PresetData.FromWhatItem.ContainsKey(type)) {
                weapon = owner.FindPlayerItem(PresetData.FromWhatItem[type]);
            } else if (PresetData.MinionItem.ContainsKey(type)) {
                weapon = owner.FindPlayerItem(PresetData.MinionItem[type]);
            } else {
                weapon = owner.HeldItem;
            }
            return weapon;
        }
    }
}
