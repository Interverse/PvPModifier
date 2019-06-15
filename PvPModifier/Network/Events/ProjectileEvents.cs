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
            
            if (!e.Attacker.TPlayer.hostile) return;
            if (projectile.active && projectile.type == e.Type) return;

            List<Projectile> projectiles = new List<Projectile>();
            DbItem weapon = Cache.Items[e.Weapon.netID];
            RandomPool<int> projectilePool = weapon.ProjectilePoolList;

            if (!projectilePool.IsEmpty()) {
                e.Type = projectilePool.GetRandomItem();
            }

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

            if (weapon.Spread > 0) {
                float spreadAmount = weapon.Spread / 2f;
                for (int x = 0; x < weapon.NumShots; x++) {
                    if (x == 0) {
                        e.Args.Handled = true;
                        projectile.SetDefaults(0);
                        NetMessage.SendData(27, -1, -1, null, e.Identity);
                        projectiles.Clear();
                    }

                    Projectile newProj = new Projectile();

                    if (!projectilePool.IsEmpty()) {
                        e.Type = projectilePool.GetRandomItem();
                    }

                    newProj.SetDefaults(e.Type);
                    newProj.identity = e.Identity;
                    newProj.damage = e.Damage;
                    newProj.knockBack = e.Knockback;
                    newProj.owner = e.Owner;
                    newProj.position = e.Position + new Vector2(newProj.width, newProj.height) / 2;
                    if (weapon.IsRandomSpread) {
                        newProj.velocity = e.Velocity.RotateRandom(-spreadAmount, spreadAmount);
                    } else {
                        newProj.velocity = e.Velocity.Rotate(-spreadAmount).Rotate(x * weapon.Spread / weapon.NumShots);
                    }
                    newProj.ai = e.Ai;

                    ProjectileUtils.SpawnProjectile(e.Attacker, newProj, e.Weapon.netID);

                    projectiles.Add(newProj);
                }
            }

            if (weapon.IsMirror) {
                foreach (var proj in projectiles) {
                    ProjectileUtils.SpawnProjectile(e.Attacker, proj.position + new Vector2(proj.width, proj.height) / 2, proj.velocity * -1, proj.type, proj.damage, proj.knockBack, proj.owner, proj.ai[0], proj.ai[1], e.Weapon.netID);
                }
            }

            e.Attacker.GetProjectileTracker().InsertProjectile(projectile.identity, projectile.type, e.Owner, e.Weapon.netID);
            e.Attacker.GetProjectileTracker().Projectiles[projectile.type].PerformProjectileAction();
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
            if (projectile.GetOwner() == null) return;

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

        public static void UpdateActiveProjectileAI(ProjectileAiUpdateEventArgs args) {
            if (!PvPModifier.Config.EnableHoming) return;

            var projectile = args.Projectile;

            if (!projectile.HasInitializedExtraAISlots()) return;
            if (projectile.GetOwner() == null) return;

            DbItem dbItem = Cache.Items[projectile.GetItemOriginated().type];
            RandomPool<int> projectilePool = dbItem.ActiveProjectilePoolList;
            float spread = dbItem.ActiveSpread;
            Item item = new Item();
            item.SetDefaults(projectile.GetItemOriginated().type);

            projectile.DecreaseCooldown();

            if (projectile.CooldownFinished()) {
                switch ((ActiveAIType)dbItem.ActiveProjectileAI) {
                    case ActiveAIType.Minion:
                        spread = spread.Replace(-1, 0);
                        TSPlayer target = PvPUtils.FindClosestPlayer(projectile.position, projectile.owner, dbItem.ActiveRange * Constants.PixelToWorld);
                        if (target != null) {
                            ProjectileUtils.SpawnProjectile(projectile.GetOwner(),
                                projectile.Center,
                                (target.TPlayer.Center - projectile.Center).Normalized().RotateRandom(-spread / 2, spread / 2) * dbItem.ShootSpeed.Replace(-1, item.shootSpeed),
                                projectilePool.GetRandomItem(),
                                projectile.damage,
                                projectile.knockBack,
                                projectile.owner,
                                projectile.ai[0],
                                projectile.ai[1],
                                projectile.GetItemOriginated().netID,
                                cooldown: 9999);
                        }
                        break;

                    case ActiveAIType.RandomScatter:
                        spread = spread.Replace(-1, 180);
                        ProjectileUtils.SpawnProjectile(projectile.GetOwner(),
                            projectile.Center,
                            projectile.velocity.RotateRandom(-spread, spread).Normalized() * dbItem.ShootSpeed.Replace(-1, item.shootSpeed),
                            projectilePool.GetRandomItem(),
                            projectile.damage,
                            projectile.knockBack,
                            projectile.owner,
                            projectile.ai[0],
                            projectile.ai[1],
                            projectile.GetItemOriginated().netID,
                            cooldown: 9999);
                        break;

                    case ActiveAIType.V_Split:
                        spread = spread.Replace(-1, 30);
                        for(float degrees = -spread / 2; degrees <= spread / 2; degrees += spread) {
                            ProjectileUtils.SpawnProjectile(projectile.GetOwner(),
                                projectile.Center,
                                projectile.velocity.Rotate(degrees),
                                projectilePool.GetRandomItem(),
                                projectile.damage,
                                projectile.knockBack,
                                projectile.owner,
                                projectile.ai[0],
                                projectile.ai[1],
                                projectile.GetItemOriginated().netID,
                                cooldown: 9999);
                        }
                        break;
                }

                projectile.SetCooldown(dbItem.ActiveFireRate);
            }
        }
    }
}
