using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PvPModifier.CustomWeaponAPI;
using Terraria;
using TShockAPI;

namespace PvPModifier.Utilities {
    /// <summary>
    /// Contains helper methods that help run anything that requires Server Side Characters to be enabled.
    /// </summary>
    public class SSCUtils {
        /// <summary>
        /// Replaces every instance of an item that matches the target item id with an item
        /// that matches the replacement item id.
        /// </summary>
        /// <param name="player">The player to change inventory from.</param>
        /// <param name="targetItemID">The numerical ID of the item to be replaced.</param>
        /// <param name="replacementItemID">The numerical ID of the item to replace.</param>
        public static void FillInventory(TSPlayer player, short targetItemID, short replacementItemID) {
            new SSCAction(player, () => {
                for (byte index = 0; index < 58; index++) {
                    if (!player.ConnectionAlive) return;
                    if (player.TPlayer.inventory[index].netID == targetItemID) {
                        player.TPlayer.inventory[index].SetDefaults(replacementItemID);
                        NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, null, player.Index, index, 1, 0, replacementItemID);
                    }
                }
            });
        }

        /// <summary>
        /// Replaces every instance of an item that matches the target item id with an item
        /// that matches the replacement item id up to a certain inventory index.
        /// </summary>
        /// <param name="player">The player to change inventory from.</param>
        /// <param name="targetItemID">The numerical ID of the item to be replaced.</param>
        /// <param name="replacementItemID">The numerical ID of the item to replace.</param>
        /// <param name="maxIndex">The index to fill the inventory up to.</param>
        public static void FillInventoryToIndex(TSPlayer player, short targetItemID, short replacementItemID, int maxIndex) {
            InventoryIndexer indexer = new InventoryIndexer();
            indexer.StoreMaxIndex(maxIndex);

            new SSCAction(player, () => {
                for (byte loop = 0; loop < indexer.MaxIndexPos; loop++) {
                    if (!player.ConnectionAlive) return;
                    int index = indexer.NextIndex();
                    if (player.TPlayer.inventory[index].netID == targetItemID) {
                        player.TPlayer.inventory[index].SetDefaults(replacementItemID);
                        NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, null, player.Index, index, 1, 0, replacementItemID);
                    }
                }
            });
        }

        /// <summary>
        /// Sets an item to an index of a player.
        /// </summary>
        /// <param name="player">Player to modify inventory of.</param>
        /// <param name="index">Index of the player's inventory.</param>
        /// <param name="itemID">Numerical ID of an item.</param>
        public static void SetItem(TSPlayer player, byte index, short itemID) {
            new SSCAction(player, () => {
                if (!player.ConnectionAlive) return;
                player.SendRawData(new PacketWriter()
                    .SetType((short)PacketTypes.PlayerSlot)
                    .PackByte((byte)player.Index)
                    .PackByte(index)
                    .PackInt16(1)
                    .PackByte(0)
                    .PackInt16(itemID)
                    .GetByteData());
            });
        }

        /// <summary>
        /// Sets an item to an index of a player.
        /// </summary>
        /// <param name="player">Player to modify inventory of.</param>
        /// <param name="index">Index of the player's inventory.</param>
        /// <param name="item"><see cref="Terraria.Item"/> to be set.</param>
        public static void SetItem(TSPlayer player, byte index, Item item) {
            new SSCAction(player, () => {
                if (!player.ConnectionAlive) return;
                player.SendRawData(new PacketWriter()
                    .SetType((short)PacketTypes.PlayerSlot)
                    .PackByte((byte)player.Index)
                    .PackByte(index)
                    .PackInt16((short)item.stack)
                    .PackByte(item.prefix)
                    .PackInt16((short)item.type)
                    .GetByteData());
            });
        }
    }
}
