using System;
using System.IO;
using PvPModifier.Network.Packets;
using PvPModifier.Variables;
using TerrariaApi.Server;

namespace PvPModifier.Network {
    /// <summary>
    /// Creates hooks for plugins to use.
    /// </summary>
    public class DataHandler {
        public static event EventHandler<PlayerHurtArgs> PlayerHurt;
        public static event EventHandler<PlayerUpdateArgs> PlayerUpdate;
        public static event EventHandler<ProjectileNewArgs> ProjectileNew;
        public static event EventHandler<ProjectileDestroyArgs> ProjectileDestroyed;
        public static event EventHandler<PlayerDeathArgs> PlayerDeath;
        public static event EventHandler<TogglePvPArgs> PvPToggled;
        public static event EventHandler<PlayerSlotArgs> SlotUpdate;

        public static void HandleData(GetDataEventArgs args, MemoryStream data, PvPPlayer player) {
            switch (args.MsgID) {
                case PacketTypes.PlayerHurtV2:
                    if (new PlayerHurtArgs().ExtractData(args, data, player, out var playerhurt))
                        PlayerHurt?.Invoke(typeof(DataHandler), playerhurt);
                    return;

                case PacketTypes.TogglePvp:
                    if (new TogglePvPArgs().ExtractData(player, out var togglepvp))
                        PvPToggled?.Invoke(typeof(DataHandler), togglepvp);
                    return;

                case PacketTypes.PlayerSlot:
                    if (new PlayerSlotArgs().ExtractData(data, player, out var playerslot))
                        SlotUpdate?.Invoke(typeof(DataHandler), playerslot);
                    return;

                case PacketTypes.PlayerDeathV2:
                    if (new PlayerDeathArgs().ExtractData(player, out var playerdeath))
                        PlayerDeath?.Invoke(typeof(DataHandler), playerdeath);
                    return;

                case PacketTypes.ProjectileNew:
                    if (new ProjectileNewArgs().ExtractData(args, data, player, out var projectilenew))
                        ProjectileNew?.Invoke(typeof(DataHandler), projectilenew);
                    return;

                case PacketTypes.ProjectileDestroy:
                    if (new ProjectileDestroyArgs().ExtractData(data, out var projectiledestroy))
                        ProjectileDestroyed?.Invoke(typeof(DataHandler), projectiledestroy);
                    return;

                case PacketTypes.PlayerUpdate:
                    if (new PlayerUpdateArgs().ExtractData(data, player, out var playerupdate))
                        PlayerUpdate?.Invoke(typeof(DataHandler), playerupdate);
                    return;
            }
        }
    }
}
