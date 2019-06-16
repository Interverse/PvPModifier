using System;
using TShockAPI;

namespace PvPModifier.Network.Packets {
    public class TogglePvPArgs : EventArgs {
        public TSPlayer Player;
        public bool Hostile;

        public bool ExtractData(TSPlayer player, out TogglePvPArgs arg) {
            arg = new TogglePvPArgs {
                Player = player,
                Hostile = !player.TPlayer.hostile
            };

            return true;
        }
    }
}
