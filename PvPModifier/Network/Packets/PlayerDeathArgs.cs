using System;
using System.IO;
using Terraria.DataStructures;
using TShockAPI;

namespace PvPModifier.Network.Packets {
    public class PlayerDeathArgs : EventArgs {
        public TSPlayer Dead;
        public TSPlayer Killer;

        public bool ExtractData(MemoryStream data, TSPlayer dead, out PlayerDeathArgs arg) {
            arg = null;

            data.ReadByte();
            var playerHitReason = PlayerDeathReason.FromReader(new BinaryReader(data));
            TSPlayer killer;

            int targetID = playerHitReason.SourcePlayerIndex;

            if (targetID > -1) {
                killer = TShock.Players[targetID];
                if (killer == null || !killer.ConnectionAlive || !killer.Active) {
                    return false;
                }
            } else {
                return false;
            }

            arg = new PlayerDeathArgs {
                Dead = dead,
                Killer = killer
            };

            return true;
        }
    }
}
