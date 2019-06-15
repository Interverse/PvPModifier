using PvPModifier.Network.Packets;
using PvPModifier.Variables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PvPModifier.Network.Events {
    public class NetworkEvents {
        public static void ReceivePing(object sender, ItemOwnerArgs e) {
            e.Player.SetPingCheck(true);
        }
    }
}
