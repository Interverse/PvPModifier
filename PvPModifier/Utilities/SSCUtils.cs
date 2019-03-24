using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PvPModifier.CustomWeaponAPI;
using Terraria;
using TShockAPI;

namespace PvPModifier.Utilities {
    public class SSCUtils {
        public static void FillInventory(TSPlayer player, short targetItemID, short replacementItemID) {
            new SSCAction(player, () => {
                for (byte index = 0; index < 58; index++) {
                    if (player.TPlayer.inventory[index].netID == targetItemID) {
                        player.TPlayer.inventory[index].SetDefaults(replacementItemID);
                        NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, null, player.Index, index, 1, 0, replacementItemID);
                    }
                }
            });
        }

        public static void FillInventoryToIndex(TSPlayer player, short targetItemID, short replacementItemID, int maxIndex) {
            InventoryIndexer indexer = new InventoryIndexer();
            indexer.StoreMaxIndex(maxIndex);

            new SSCAction(player, () => {
                for (byte loop = 0; loop < indexer.MaxIndexPos; loop++) {
                    int index = indexer.NextIndex();
                    if (player.TPlayer.inventory[index].netID == targetItemID) {
                        player.TPlayer.inventory[index].SetDefaults(replacementItemID);
                        NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, null, player.Index, index, 1, 0, replacementItemID);
                    }
                }
            });
        }

        public static void SetItem(TSPlayer player, byte index, short replacementItemID) {
            new SSCAction(player, () => {
                player.SendRawData(new PacketWriter()
                    .SetType((short)PacketTypes.PlayerSlot)
                    .PackByte((byte)player.Index)
                    .PackByte(index)
                    .PackInt16(1)
                    .PackByte(0)
                    .PackInt16(replacementItemID)
                    .GetByteData());
            });
        }

        public static void SetItem(TSPlayer player, byte index, Item item) {
            new SSCAction(player, () => {
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
