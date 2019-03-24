using System.Text;
using System.IO;

namespace PvPModifier.CustomWeaponAPI {
    public class PacketWriter {
        private MemoryStream memoryStream;
        private BinaryWriter writer;

        public PacketWriter() {
            memoryStream = new MemoryStream();
            writer = new BinaryWriter(memoryStream);
            writer.BaseStream.Position = 3L;
        }

        public PacketWriter SetType(short type) {
            long currentPosition = writer.BaseStream.Position;
            writer.BaseStream.Position = 2L;
            writer.Write(type);
            writer.BaseStream.Position = currentPosition;
            return this;
        }

        public PacketWriter PackSByte(sbyte num) {
            writer.Write(num);
            return this;
        }

        public PacketWriter PackByte(byte num) {
            writer.Write(num);
            return this;
        }

        public PacketWriter PackInt16(short num) {
            writer.Write(num);
            return this;
        }

        public PacketWriter PackUInt16(ushort num) {
            writer.Write(num);
            return this;
        }

        public PacketWriter PackInt32(int num) {
            writer.Write(num);
            return this;
        }

        public PacketWriter PackUInt32(uint num) {
            writer.Write(num);
            return this;
        }

        public PacketWriter PackUInt64(ulong num) {
            writer.Write(num);
            return this;
        }

        public PacketWriter PackSingle(float num) {
            writer.Write(num);
            return this;
        }

        public PacketWriter PackString(string str) {
            writer.Write(str);
            return this;
        }

        private void UpdateLength() {
            long currentPosition = writer.BaseStream.Position;
            writer.BaseStream.Position = 0L;
            writer.Write((short) currentPosition);
            writer.BaseStream.Position = currentPosition;
        }

        public static string ByteArrayToString(byte[] ba) {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

        public byte[] GetByteData() {
            UpdateLength();
            return memoryStream.ToArray();
        }
    }
}
