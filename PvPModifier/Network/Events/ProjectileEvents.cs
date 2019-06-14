using Microsoft.Xna.Framework;
using PvPModifier.CustomWeaponAPI;
using PvPModifier.DataStorage;
using PvPModifier.Network.Packets;
using PvPModifier.Utilities;
using PvPModifier.Utilities.PvPConstants;
using PvPModifier.Variables;
using System;
using System.Collections.Generic;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace PvPModifier.Network.Events {
    public class ProjectileEvents {
        /// <summary>
        /// Handles projectile creation.
        /// </summary>
        public static void OnNewProjectile(object sender, ProjectileNewArgs e) {
            if (!PvPModifier.Config.EnablePlugin) return;

            var projectile = Main.projectile[e.Identity];
            projectile.InitializeExtraAISlots();

            if (projectile.active && projectile.type == e.Type) return;

            List<Projectile> projectiles = new List<Projectile>();
            DbItem weapon = Cache.Items[e.Weapon.netID];

            projectile.SetDefaults(e.Type);
            projectile.identity = e.Identity;
            projectile.damage = e.Damage;
            projectile.knockBack = e.Knockback;
            projectile.owner = e.Owner;
            projectile.position = e.Position;
            projectile.velocity = e.Velocity;
            projectile.ai = e.Ai;

            projectiles.Add(projectile);

            if ((TShock.Players[e.Owner]?.TPlayer?.hostile ?? false) && PvPUtils.IsModifiedProjectile(e.Type)) {
                e.Args.Handled = true;
                DbProjectile proj = Cache.Projectiles[e.Type];

                projectile.SetDefaults(proj.Shoot != -1 ? proj.Shoot : e.Type);
                projectile.velocity = e.Velocity * proj.VelocityMultiplier;
                projectile.damage = proj.Damage != -1 ? proj.Damage : e.Damage;
                projectile.active = true;
                projectile.identity = e.Identity;
                projectile.owner = e.Owner;
                projectile.position = e.Position;

                NetMessage.SendData(27, -1, -1, null, e.Identity);
            }

            if (weapon.IsMirror) {
                foreach (var proj in projectiles) {
                    ProjectileUtils.SpawnProjectile(e.Attacker, proj.position + new Vector2(proj.width, proj.height) / 2, proj.velocity * -1, proj.type, proj.damage, proj.knockBack, proj.owner, proj.ai[0], proj.ai[1], e.Weapon.netID);
                }
            }

            e.Attacker.GetProjectileTracker().InsertProjectile(e.Identity, e.Type, e.Owner, e.Weapon.netID);
            e.Attacker.GetProjectileTracker().Projectiles[e.Type].PerformProjectileAction();
        }

        /// <summary>
        /// Runs every 1/60th second to reset any inactive projectiles.
        /// </summary>
        public static void CleanupInactiveProjectiles(EventArgs args) {
            for (int x = 0; x < Main.maxProjectiles; x++) {
                if (!Main.projectile[x].active) {
                    Main.projectile[x] = new Projectile();
                }
            }
        }

        /// <summary>
        /// Handles homing projectiles.
        /// </summary>
        public static void UpdateProjectileHoming(ProjectileAiUpdateEventArgs args) {
            if (!PvPModifier.Config.EnableHoming) return;

            var projectile = args.Projectile;

            if (!projectile.HasInitializedExtraAISlots()) return;

            float homingRadius = Cache.Items[projectile.GetItemOriginated().type].HomingRadius;
            if (homingRadius < 0) return;

            float angularVelocity = Cache.Items[projectile.GetItemOriginated().type].AngularVelocity;

            TSPlayer target = PvPUtils.FindClosestPlayer(projectile.position, projectile.owner, homingRadius * Constants.PixelToWorld);

            if (target != null) {
                projectile.velocity = MiscUtils.TurnTowards(projectile.velocity, projectile.position, target.TPlayer.Center, angularVelocity);
                foreach (var pvper in PvPUtils.ActivePlayers) {
                    pvper.SendRawData(new PacketWriter()
                        .SetType((short)PacketTypes.ProjectileNew)
                        .PackInt16((short)projectile.identity)
                        .PackSingle(projectile.position.X)
                        .PackSingle(projectile.position.Y)
                        .PackSingle(projectile.velocity.X)
                        .PackSingle(projectile.velocity.Y)
                        .PackSingle(projectile.knockBack)
                        .PackInt16((short)projectile.damage)
                        .PackByte((byte)projectile.owner)
                        .PackInt16((short)projectile.type)
                        .PackByte(0)
                        .PackSingle(projectile.ai[0])
                        .GetByteData());
                }
            }
        }
    }
}
