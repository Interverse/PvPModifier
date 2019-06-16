using PvPModifier.Network.Packets;
using PvPModifier.Utilities.Extensions;

namespace PvPModifier.Network.Events {
    public class NetworkEvents {
        /// <summary>
        /// Indicate that the player sent an ItemOwner packet.
        /// This is used in conjunction with UpdateItemOwner, as sending one makes
        /// their client send an ItemOwner packet in response.
        /// 
        /// This technique is used for ping or response testing.
        /// </summary>
        public static void ReceivePing(object sender, ItemOwnerArgs e) {
            e.Player.SetPingCheck(true);
        }
    }
}
