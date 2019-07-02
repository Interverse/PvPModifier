using System;
using System.IO;
using TShockAPI;

namespace PvPModifier.Network.Packets {
    public class PlayerUpdateArgs : EventArgs {
        public TSPlayer Player;

        public int PlayerAction;
        public int Pulley;
        public int SelectedSlot;

        public bool ExtractData(MemoryStream data, TSPlayer player, out PlayerUpdateArgs arg) {
            data.ReadByte();

            arg = new PlayerUpdateArgs {
                Player = player,
                PlayerAction = data.ReadByte(),
                Pulley = data.ReadByte(),
                SelectedSlot = data.ReadByte()
            };

            return true;
        }
    }
}
