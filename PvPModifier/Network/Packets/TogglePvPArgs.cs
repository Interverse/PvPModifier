using System;
using PvPModifier.Variables;

namespace PvPModifier.Network.Packets {
    public class TogglePvPArgs : EventArgs {
        public PvPPlayer Player;
        public bool Hostile;

        public bool ExtractData(PvPPlayer player, out TogglePvPArgs arg) {
            arg = new TogglePvPArgs {
                Player = player,
                Hostile = !player.TPlayer.hostile
            };

            return true;
        }
    }
}
