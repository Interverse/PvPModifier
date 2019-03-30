using System.Collections.Generic;
using Microsoft.Xna.Framework;
using PvPModifier.CustomWeaponAPI;
using PvPModifier.DataStorage;
using PvPModifier.Utilities.PvPConstants;
using PvPModifier.Variables;
using Terraria;
using TShockAPI;

namespace PvPModifier.Utilities {
    public class PvPUtils {
        /// <summary>
        /// Generates a death message for a person based off the weapon and type of death.
        /// </summary>
        /// <param name="type">1 for normal hits, 2 for reflection hits such as thorns and turtle.</param>
        public static string GetPvPDeathMessage(string deathMessage, PvPItem weapon, PvPProjectile proj = null, int type = 1) {
            string tag = "";
            if (type == 1) tag = weapon?.netID != 0 || proj?.ItemOriginated?.netID != 0 ? 
                "[i/p{0}:{1}] ".SFormat(proj?.ItemOriginated?.prefix ?? weapon?.prefix, 
                                        proj?.ItemOriginated?.netID ?? weapon?.netID) 
                : "";
            else if (type == 2) tag = "[i:1150] ";

            return tag + deathMessage;
        }

        /// <summary>
        /// Sends every modified weapon to the player.
        /// Steps:
        /// Change every weapon to default values.
        /// Stores the index of every modified weapon, and stores the index as the max index.
        /// Fill the player's inventory with a placeholder item up to the max index.
        /// Removes every item that was modified serverside with the list of indexes.
        /// Starts to drop the modified items.
        /// </summary>
        public static void SendCustomItems(PvPPlayer player) {
            RefreshInventory(player);
            List<int> itemIndex = new List<int>();
            InventoryIndexer indexer = new InventoryIndexer();
            
            for (byte loop = 0; loop < indexer.MaxInventoryCycle; loop++) {
                int index = indexer.NextIndex();

                Item item = player.TPlayer.inventory[index];
                
                var custwep = GetCustomWeapon(player, item.type, item.prefix, (short)item.stack);
                if (IsModifiedItem(custwep.ItemNetId)) {
                    indexer.StoreMaxIndex(index);
                    itemIndex.Add(index);
                    player.InvTracker.AddItem(custwep);
                }
            }
            
            if (itemIndex.Count != 0) {
                SSCUtils.FillInventoryToIndex(player, Constants.EmptyItem, Constants.JunkItem, indexer.MaxIndex);
                foreach (int num in itemIndex)
                    SSCUtils.SetItem(player, (byte)num, Constants.EmptyItem);
                player.InvTracker.StartDroppingItems();
            } else {
                player.InvTracker.CheckFinishedModifications(0);
            }
        }

        /// <summary>
        /// Changes every item in a player's inventory to be the default values.
        /// </summary>
        public static void RefreshInventory(PvPPlayer player) {
            player.InvTracker.OnPvPInventoryChecked = false;
            for (byte index = 0; index < 58; index++) {
                SSCUtils.SetItem(player, index, player.TPlayer.inventory[index]);
            }
        }

        /// <summary>
        /// Changes every item in a player's inventory that matches the item ID to the default values.
        /// </summary>
        /// <param name="itemID">Numerical ID of item to be reset.</param>
        public static void RefreshItem(PvPPlayer player, int itemID) {
            for (byte index = 0; index <= 58; index++) {
                if (player.TPlayer.inventory[index].netID == itemID)
                    SSCUtils.SetItem(player, index, player.TPlayer.inventory[index]);
            }
        }

        /// <summary>
        /// Gets a <see cref="CustomWeapon"/> from the <see cref="Cache"/>
        /// </summary>
        /// <returns></returns>
        public static CustomWeapon GetCustomWeapon(PvPPlayer player, int type, byte prefix = 0, short stack = 1) {
            Item wep = new Item();
            wep.SetDefaults(type);
            CustomWeapon custwep = new CustomWeapon {
                ItemNetId = (short)wep.netID,
                Prefix = prefix,
                Stack = stack,
                DropAreaWidth = short.MaxValue,
                DropAreaHeight = short.MaxValue
            };
            DbItem dbitem = Cache.Items[type];

            if (wep.damage != dbitem.Damage) custwep.Damage = (ushort)dbitem.Damage;
            if (wep.knockBack != dbitem.Knockback) custwep.Knockback = dbitem.Knockback;
            if (dbitem.UseAnimation != -1) custwep.UseAnimation = (ushort)dbitem.UseAnimation;
            if (dbitem.UseTime != -1) custwep.UseTime = (ushort)dbitem.UseTime;
            if (dbitem.Shoot != -1) custwep.ShootProjectileId = (short)dbitem.Shoot;
            if (dbitem.ShootSpeed != -1) custwep.ShootSpeed = dbitem.ShootSpeed;
            if (dbitem.AmmoIdentifier != -1) custwep.AmmoIdentifier = (short)dbitem.AmmoIdentifier;
            if (dbitem.UseAmmoIdentifier != -1) custwep.UseAmmoIdentifier = (short)dbitem.AmmoIdentifier;
            if (wep.notAmmo != dbitem.IsNotAmmo) custwep.NotAmmo = dbitem.NotAmmo == 1;

            return custwep;
        }

        /// <summary>
        /// Checks whether an item was modified in the database.
        /// </summary>
        public static bool IsModifiedItem(int type) {
            DbItem dbitem = Cache.Items[type];
            Item item = new Item();
            item.SetDefaults(type);

            return dbitem.Damage != -1 ||
                   dbitem.Knockback != item.knockBack ||
                   dbitem.UseAnimation != -1 ||
                   dbitem.UseTime != -1 ||
                   dbitem.Shoot != -1 ||
                   dbitem.ShootSpeed != -1 ||
                   dbitem.AmmoIdentifier != -1 ||
                   dbitem.UseAmmoIdentifier != -1 ||
                   dbitem.IsNotAmmo != item.notAmmo;
        }

        /// <summary>
        /// Checks whether a projectile was modified in the database.
        /// </summary>
        public static bool IsModifiedProjectile(int type) {
            DbProjectile proj = Cache.Projectiles[type];
            return proj.Shoot != type ||
                   proj.VelocityMultiplier != 1 ||
                   proj.Damage != -1;
        }

        /// <summary>
        /// Converts a regular Terraria <see cref="Item"/> into a <see cref="PvPItem"/>.
        /// </summary>
        public static PvPItem ConvertToPvPItem(Item item) => new PvPItem(item);

        /// <summary>
        /// Finds the closest pvper from a position.
        /// </summary>
        /// <param name="position">The position to find players from</param>
        /// <param name="selfIndex">The user to ignore</param>
        /// <param name="radius">The radius in which to find players (in pixels)</param>
        public static PvPPlayer FindClosestPlayer(Vector2 position, int selfIndex, float radius = -1) {
            float closestPersonDistance = -1;
            PvPPlayer target = null;
            for (int pvperIndex = 0; pvperIndex < Main.maxPlayers; pvperIndex++) {
                var pvper = PvPModifier.PvPers[pvperIndex];
                if (pvper == null || !pvper.TPlayer.hostile || pvper.TPlayer.dead) continue;

                var distance = Vector2.Distance(position, pvper.TPlayer.Center);

                if (pvper.Index != selfIndex && (distance < closestPersonDistance || closestPersonDistance == -1) 
                                             && (distance < radius || radius == -1)) {
                    closestPersonDistance = distance;
                    target = pvper;
                }
            }

            return target;
        }
    }
}
