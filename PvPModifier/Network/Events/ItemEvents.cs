using PvPModifier.Network.Packets;
using PvPModifier.Utilities;
using PvPModifier.Utilities.Extensions;
using PvPModifier.Utilities.PvPConstants;
using System;

namespace PvPModifier.Network.Events {
    public class ItemEvents {
        /// <summary>
        /// Replaces items placed into the inventory into the pvp versions.
        /// </summary>
        public static void CheckIncomingItems(object sender, PlayerSlotArgs e) {
            if (!PvPModifier.Config.EnablePlugin) return;

            if (!e.Player.TPlayer.hostile) return;
            if (e.SlotId >= 58) return;

            //Only runs after initial pvp check
            if (e.Player.GetInvTracker().OnPvPInventoryChecked) {
                if (e.Player.GetInvTracker().LockModifications) return;

                //If the item is being consumed, don't modify the item
                if (Math.Abs(e.Player.TPlayer.inventory[e.SlotId].stack - e.Stack) <= 1 && e.Player.TPlayer.inventory[e.SlotId].netID == e.NetID) return;

                //If the item is modified, fill empty spaces and add it to queue
                if (PvPUtils.IsModifiedItem(e.NetID)) {
                    SSCUtils.FillInventoryToIndex(e.Player, Constants.EmptyItem, Constants.JunkItem, e.SlotId);
                    SSCUtils.SetItem(e.Player, e.SlotId, Constants.EmptyItem);
                    e.Player.GetInvTracker().AddItem(PvPUtils.GetCustomWeapon(e.Player, e.NetID, e.Prefix, e.Stack));
                    e.Player.GetInvTracker().DropModifiedItems();
                }
            }
        }

        /// <summary>
        /// Checks if the player has received their modified items.
        /// </summary>
        public static void CheckDrops(object sender, PlayerSlotArgs e) {
            if (!PvPModifier.Config.EnablePlugin) return;

            if (e.Player.TPlayer.dead) return;
            if (!e.Player.TPlayer.hostile) return;

            if (e.SlotId > 58) return;

            //This method runs right after the replacement method, and since the PlayerSlotArgs will be the same,
            //we add this check so the item doesn't instantly get checked off as being modified
            if (e.Player.GetInvTracker().StartPvPInventoryCheck) {
                e.Player.GetInvTracker().StartPvPInventoryCheck = false;
                return;
            }

            // If the player's inventory being updated is currently empty, check the incoming drop in the InventoryTracker
            if (e.Player.TPlayer.inventory[e.SlotId].netID == Constants.EmptyItem)
                e.Player.GetInvTracker().CheckFinishedModifications(e.NetID);
        }
    }
}
