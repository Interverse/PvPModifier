using System;
using System.IO;
using System.IO.Streams;
using PvPModifier.Variables;
using Terraria.DataStructures;
using TerrariaApi.Server;

namespace PvPModifier.Network.Packets {
    public class PlayerHurtArgs : EventArgs {
        public GetDataEventArgs Args { get; set; }

        public PvPPlayer Attacker { get; set; }
        public PvPPlayer Target { get; set; }

        public PvPItem Weapon { get; set; }
        public PvPProjectile Projectile { get; set; }

        public PlayerDeathReason PlayerHitReason { get; set; }

        public int InflictedDamage { get; set; }
        public int DamageReceived { get; set; }
        public int HitDirection { get; set; }
        public int Flag { get; set; }

        public bool ExtractData(GetDataEventArgs args, MemoryStream data, PvPPlayer attacker, out PlayerHurtArgs arg) {
            arg = null;
            int targetId = data.ReadByte();
            var playerHitReason = PlayerDeathReason.FromReader(new BinaryReader(data));
            PvPPlayer target;
            
            if (targetId > -1) {
                target = PvPModifier.PvPers[targetId];
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
                : attacker.ProjTracker.Projectiles[playerHitReason.SourceProjectileType];
            var weapon = projectile?.ItemOriginated ?? attacker.HeldItem;
            target.LastHitBy = attacker;
            target.LastHitWeapon = Weapon;
            target.LastHitProjectile = Projectile;
            
            arg = new PlayerHurtArgs {
                Args = args,
                Attacker = attacker,
                Target = target,
                Projectile = projectile,
                PlayerHitReason = playerHitReason,
                Weapon = weapon,
                InflictedDamage = data.ReadInt16(),
                DamageReceived = target.DamageReceived(InflictedDamage),
                HitDirection = data.ReadByte(),
                Flag = data.ReadByte()
            };

            return true;
        }
    }
}
