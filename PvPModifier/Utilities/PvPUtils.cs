using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PvPModifier.CustomWeaponAPI;
using PvPModifier.DataStorage;
using PvPModifier.Variables;
using Terraria;
using TShockAPI;
using TShockAPI.Hooks;

namespace PvPModifier.Utilities {
    public class PvPUtils {
        /// <summary>
        /// Generates a death message for a person based off the weapon and type of death.
        /// </summary>
        /// <param name="type">1 for normal hits, 2 for reflection hits such as thorns and turtle.</param>
        public static string GetPvPDeathMessage(string deathMessage, PvPItem weapon, int type = 1) {
            string tag = "";
            if (type == 1) tag = weapon?.netID != 0 ? "[i/p{0}:{1}] ".SFormat(weapon?.prefix, weapon?.netID) : "";
            else if (type == 2) tag = "[i:1150] ";

            return tag + deathMessage;
        }

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
                SSCUtils.FillInventoryToIndex(player, 0, Constants.JunkItem, indexer.MaxIndex);
                foreach (int num in itemIndex)
                    SSCUtils.SetItem(player, (byte)num, 0);
                player.InvTracker.StartDroppingItems();
            } else {
                player.InvTracker.CheckFinishedModifications(0);
            }
        }

        public static void RefreshInventory(PvPPlayer player) {
            player.InvTracker.OnPvPInventoryChecked = false;
            for (byte index = 0; index < 58; index++) {
                SSCUtils.SetItem(player, index, player.TPlayer.inventory[index]);
            }
        }

        public static void RefreshItem(PvPPlayer player, int itemID) {
            for (byte index = 0; index <= 58; index++) {
                if (player.TPlayer.inventory[index].netID == itemID)
                    SSCUtils.SetItem(player, index, player.TPlayer.inventory[index]);
            }
        }

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
            if (wep.notAmmo != dbitem.NotAmmo) custwep.NotAmmo = dbitem.NotAmmo;

            return custwep;
        }

        public static bool IsModifiedItem(int type) {
            DbItem dbitem = Cache.Items[type];
            Item item = new Item();
            item.SetDefaults(type);

            //Ignore coins
            if (type >= 71 && type <= 74) return false;

            return dbitem.Damage != item.damage ||
                   dbitem.Knockback != item.knockBack ||
                   dbitem.UseAnimation != -1 ||
                   dbitem.UseTime != -1 ||
                   dbitem.Shoot != -1 ||
                   dbitem.ShootSpeed != -1 ||
                   dbitem.AmmoIdentifier != -1 ||
                   dbitem.UseAmmoIdentifier != -1 ||
                   dbitem.NotAmmo != item.notAmmo;
        }

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
    }
}
