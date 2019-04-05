using Terraria;
using TShockAPI;

namespace PvPModifier.Utilities {
    /// <summary>
    /// Allows a piece of code to be ran while temporarily turning on Server Side Characters.
    /// </summary>
    class SSCAction {
        public delegate void Action();

        public SSCAction(TSPlayer player, Action action) {
            bool isSSC = Main.ServerSideCharacter;
            if (player == null || !player.ConnectionAlive) return;

            if (!isSSC) {
                Main.ServerSideCharacter = true;
                NetMessage.SendData(7, player.Index);
                player.IgnoreSSCPackets = true;
            }

            action();

            if (!isSSC) {
                Main.ServerSideCharacter = false;
                NetMessage.SendData(7, player.Index);
                player.IgnoreSSCPackets = false;
            }
        }
    }
}
