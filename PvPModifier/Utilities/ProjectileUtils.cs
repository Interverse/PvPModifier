using Microsoft.Xna.Framework;
using PvPModifier.Variables;
using Terraria;
using TShockAPI;

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
        public static Item GetProjectileWeapon(TSPlayer owner, int type) {
            Item weapon;
            if (PresetData.PresetProjDamage.ContainsKey(type)) {
                weapon = new Item();
            } else if (PresetData.ProjHooks.ContainsKey(type)) {
                weapon = owner.FindPlayerItem(PresetData.ProjHooks[type]);
            } else if (PresetData.FromWhatItem.ContainsKey(type)) {
                weapon = owner.FindPlayerItem(PresetData.FromWhatItem[type]);
            } else if (PresetData.MinionItem.ContainsKey(type)) {
                weapon = owner.FindPlayerItem(PresetData.MinionItem[type]);
            } else {
                weapon = owner.TPlayer.HeldItem;
            }
            return weapon;
        }

        public static void SpawnProjectile(TSPlayer player, Vector2 position, Vector2 velocity, int type, int damage, float knockBack, int owner = 255, float ai0 = 0.0f, float ai1 = 0.0f, int itemType = 0) {
            SpawnProjectile(player, position.X, position.Y, velocity.X, velocity.Y, type, damage, knockBack, owner, ai0, ai0, itemType);
        }

        public static void SpawnProjectile(TSPlayer player, float x, float y, float speedX, float speedY, int type, int damage, float knockBack, int owner = 255, float ai0 = 0.0f, float ai1 = 0.0f, int itemType = 0) {
            int projIndex = Projectile.NewProjectile(x, y, speedX, speedY, type, damage, knockBack, owner, ai0, ai1);
            Main.projectile[projIndex].Initialize();
            NetMessage.SendData(27, -1, -1, null, projIndex);

            player.GetProjectileTracker().InsertProjectile(projIndex, type, player.Index, itemType);
            player.GetProjectileTracker().Projectiles[type].PerformProjectileAction();
        }
    }
}
