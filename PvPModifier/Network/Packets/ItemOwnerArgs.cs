using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;

namespace PvPModifier.Network.Packets {
    public class ItemOwnerArgs : EventArgs {
        public TSPlayer Player;

        public bool ExtractData(TSPlayer player, out ItemOwnerArgs arg) {
            arg = new ItemOwnerArgs {
                Player = player
            };

            return true;
        }
    }
}
