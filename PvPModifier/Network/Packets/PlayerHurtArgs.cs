using PvPModifier.Utilities.Extensions;
using System;
using System.IO;
using System.IO.Streams;
using Terraria;
using Terraria.DataStructures;
using TerrariaApi.Server;
using TShockAPI;

namespace PvPModifier.Network.Packets {
    public class PlayerHurtArgs : EventArgs {
        public GetDataEventArgs Args { get; set; }

        public TSPlayer Attacker { get; set; }
        public TSPlayer Target { get; set; }

        public Item Weapon { get; set; }
        public Projectile Projectile { get; set; }

        public PlayerDeathReason PlayerHitReason { get; set; }

        public int InflictedDamage { get; set; }
        public int DamageReceived { get; set; }
        public int HitDirection { get; set; }
        public int Flag { get; set; }

        public bool ExtractData(GetDataEventArgs args, MemoryStream data, TSPlayer attacker, out PlayerHurtArgs arg) {
            arg = null;
            int targetId = data.ReadByte();
            var playerHitReason = PlayerDeathReason.FromReader(new BinaryReader(data));
            TSPlayer target;
            
            if (targetId > -1) {
                target = TShock.Players[targetId];
                if (target == null || !target.ConnectionAlive || !target.Active) {
                    return false;
                }

                if (attacker == target) {
                    return false;
                }
            } else {
                return false;
            }
            
            var projectile = playerHitReason.SourceProjectileIndex == -1
                ? null
                : attacker.GetProjectileTracker().Projectiles[playerHitReason.SourceProjectileType];
            var weapon = projectile?.GetItemOriginated() ?? attacker.TPlayer.HeldItem;
            
            arg = new PlayerHurtArgs {
                Args = args,
                Attacker = attacker,
                Target = target,
                Projectile = projectile,
                PlayerHitReason = playerHitReason,
                Weapon = weapon,
                InflictedDamage = data.ReadInt16(),
                DamageReceived = target.GetDamageReceived(InflictedDamage),
                HitDirection = data.ReadByte() - 1,
                Flag = data.ReadByte()
            };

            return true;
        }
    }
}
