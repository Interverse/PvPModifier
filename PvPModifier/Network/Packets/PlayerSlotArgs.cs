using System;
using System.IO;
using System.IO.Streams;
using TShockAPI;

namespace PvPModifier.Network.Packets {
    public class PlayerSlotArgs : EventArgs {
        public TSPlayer Player;
        public byte SlotId;
        public short Stack;
        public byte Prefix;
        public short NetID;

        public bool ExtractData(MemoryStream data, TSPlayer player, out PlayerSlotArgs arg) {
            data.ReadByte(); //Passes through the PlayerID data

            arg = new PlayerSlotArgs {
                SlotId = (byte)data.ReadByte(),
                Player = player,
                Stack = data.ReadInt16(),
                Prefix = (byte)data.ReadByte(),
                NetID = data.ReadInt16()
            };

            return true;
        }
    }
}
