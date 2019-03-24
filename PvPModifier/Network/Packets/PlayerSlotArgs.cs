using System;
using System.IO;
using System.IO.Streams;
using PvPModifier.Variables;

namespace PvPModifier.Network.Packets {
    public class PlayerSlotArgs : EventArgs {
        public PvPPlayer Player;
        public int SlotId;
        public int Stack;
        public int Prefix;
        public int NetID;

        public bool ExtractData(MemoryStream data, PvPPlayer player, out PlayerSlotArgs arg) {
            data.ReadByte(); //Passes through the PlayerID data

            arg = new PlayerSlotArgs {
                SlotId = data.ReadByte(),
                Player = player,
                Stack = data.ReadInt16(),
                Prefix = data.ReadByte(),
                NetID = data.ReadInt16()
            };

            return true;
        }
    }
}
