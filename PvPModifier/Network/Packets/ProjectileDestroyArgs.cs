using System;
using System.IO;
using System.IO.Streams;

namespace PvPModifier.Network.Packets {
    public class ProjectileDestroyArgs : EventArgs {
        public int ProjectileIndex { get; set; }
        public int Owner { get; set; }

        public bool ExtractData(MemoryStream data, out ProjectileDestroyArgs arg) {
            arg = new ProjectileDestroyArgs {
                ProjectileIndex = data.ReadInt16(),
                Owner = data.ReadByte()
            };

            return true;
        }
    }
}
