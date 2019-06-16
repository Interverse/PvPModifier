using System;
using TShockAPI;

namespace PvPModifier.Network.Packets {
    public class PlayerDeathArgs : EventArgs {
        public TSPlayer Dead;

        public bool ExtractData(TSPlayer dead, out PlayerDeathArgs arg) {
            arg = new PlayerDeathArgs {
                Dead = dead
            };

            return true;
        }
    }
}
