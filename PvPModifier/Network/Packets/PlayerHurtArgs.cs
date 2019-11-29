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
        public GetDataEventArgs Args;

        public TSPlayer Attacker;
        public TSPlayer Target;

        public Item Weapon;
        public Projectile Projectile;

        public PlayerDeathReason PlayerHitReason;

        public int InflictedDamage;
        public int DamageReceived;
        public int HitDirection;
        public int Flag;

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
