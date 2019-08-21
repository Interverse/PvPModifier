using Microsoft.Xna.Framework;
using CustomWeaponAPI;
using PvPModifier.DataStorage;
using PvPModifier.Network.Packets;
using PvPModifier.Utilities;
using PvPModifier.Utilities.PvPConstants;
using System;
using System.Collections.Generic;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using PvPModifier.Utilities.Extensions;

namespace PvPModifier.Network.Events {
    public class ProjectileEvents {
        /// <summary>
        /// Handles projectile creation and extra <see cref="DbItem"/> projectile modifiers.
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

            // Sets the projectile's stats to the ones received in ProjectileNewArgs
            projectile.SetDefaults(e.Type);
            projectile.identity = e.Identity;
            projectile.damage = e.Damage;
            projectile.knockBack = e.Knockback;
            projectile.owner = e.Owner;
            projectile.position = e.Position;
            projectile.velocity = e.Velocity;
            projectile.ai = e.Ai;
            projectile.active = true;
            projectile.SetCooldown(weapon.ActiveFireRate);

            projectiles.Add(projectile);

            // Modifies the shot projectile if it was changed in DbProjectile
            if (PvPUtils.IsModifiedProjectile(e.Type)) {
                e.Args.Handled = true;
                DbProjectile proj = Cache.Projectiles[e.Type];

                projectile.SetDefaults(proj.Shoot != -1 ? proj.Shoot : e.Type);
                projectile.damage = proj.Damage != -1 ? proj.Damage : e.Damage;

                NetMessage.SendData(27, -1, -1, null, e.Identity);
            }

            // Applies an item's velocity multiplier if it was changed
            if (weapon.VelocityMultiplier != 1) {
                e.Args.Handled = true;
                e.Velocity = e.Velocity * weapon.VelocityMultiplier;
                projectile.velocity = e.Velocity;

                NetMessage.SendData(27, -1, -1, null, e.Identity);
            }

            // If weapon spread is not negative, handle multishot projectiles
            if (weapon.Spread >= 0) {
                float spreadAmount = weapon.Spread / 2f;
                for (int x = 0; x < weapon.NumShots; x++) {
                    Projectile newProj = new Projectile();

                    if (x == 0) {
                        newProj = projectile;
                        e.Args.Handled = true;
                        projectiles.Clear();
                    }

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

                    ProjectileUtils.SpawnProjectile(e.Attacker, newProj, e.Weapon.netID, weapon.ActiveFireRate);

                    projectiles.Add(newProj);

                    if (x == 0) {
                        newProj.position = newProj.position - new Vector2(newProj.width, newProj.height) / 2;
                        NetMessage.SendData(27, -1, -1, null, e.Identity);
                    }
                }
            }

            // If the weapon mirrors projectiles, spawn every currently spawned projectile with opposite velocity
            if (weapon.IsMirror) {
                foreach (var proj in projectiles) {
                    ProjectileUtils.SpawnProjectile(e.Attacker, 
                        proj.position + new Vector2(proj.width, proj.height) / 2, 
                        proj.velocity * -1, 
                        proj.type, 
                        proj.damage, 
                        proj.knockBack, 
                        proj.owner, 
                        proj.ai[0], 
                        proj.ai[1], 
                        e.Weapon.netID, 
                        weapon.ActiveFireRate);
                }
            }

            e.Attacker.GetProjectileTracker().InsertProjectile(projectile.identity, projectile.type, e.Owner, e.Weapon.netID);
            e.Attacker.GetProjectileTracker().Projectiles[projectile.type].PerformProjectileAction();
        }

        /// <summary>
        /// Runs every game update to reset any inactive projectiles.
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

            projectile.netUpdate = true;
            float homingRadius = Cache.Items[projectile.GetItemOriginated().type].HomingRadius;
            if (homingRadius < 0) return;

            float angularVelocity = Cache.Items[projectile.GetItemOriginated().type].AngularVelocity;

            TSPlayer target = PvPUtils.FindClosestPlayer(projectile.position, projectile.owner, homingRadius * Constants.PixelToWorld, projectile.GetOwner().TPlayer.team);

            // If there is a target in site, rotate the projectile towards the target.
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

        /// <summary>
        /// Updates Active Projectile AI by changing their cooldown and performing actions when
        /// their cooldown has reached zero.
        /// </summary>
        public static void UpdateActiveProjectileAI(ProjectileAiUpdateEventArgs args) {
            var projectile = args.Projectile;

            if (!projectile.HasInitializedExtraAISlots()) return;
            if (projectile.GetOwner() == null) return;

            TSPlayer owner = projectile.GetOwner();
            DbItem dbItem = Cache.Items[projectile.GetItemOriginated().type];
            RandomPool<int> projectilePool = dbItem.ActiveProjectilePoolList;
            float spread = dbItem.ActiveSpread;
            Item item = new Item();
            item.SetDefaults(projectile.GetItemOriginated().type);

            float shootSpeed = dbItem.ActiveShootSpeed.Replace(-1, item.shootSpeed);
            Vector2 velocity;
            float rangeInBlocks = dbItem.ActiveRange * 16;

            projectile.DecreaseCooldown();

            if (projectile.CooldownFinished()) {
                switch ((ActiveAIType)dbItem.ActiveProjectileAI) {
                    case ActiveAIType.Minion:
                        spread = spread.Replace(-1, 0);
                        TSPlayer target = PvPUtils.FindClosestPlayer(projectile.position, projectile.owner, rangeInBlocks, owner.TPlayer.team);
                        if (target == null) break;

                        velocity = (target.TPlayer.Center - projectile.Center).Normalized().RotateRandom(-spread / 2, spread / 2) * shootSpeed;
                        ProjectileUtils.SpawnProjectile(projectile.GetOwner(),
                            projectile.Center,
                            velocity,
                            projectilePool.GetRandomItem(),
                            projectile.damage,
                            projectile.knockBack,
                            projectile.owner,
                            projectile.ai[0],
                            projectile.ai[1],
                            projectile.GetItemOriginated().netID,
                            cooldown: 9999);
                        break;

                    case ActiveAIType.RandomScatter:
                        if (dbItem.ActiveRange != -1 && PvPUtils.FindClosestPlayer(projectile.position, projectile.owner, rangeInBlocks, owner.TPlayer.team) == null)
                            return;

                        spread = spread.Replace(-1, 180);
                        velocity = projectile.velocity;
                        if (velocity.Length() == 0) {
                            velocity = Vector2.One;
                        }
                        velocity = velocity.RotateRandom(-spread, spread).Normalized() * shootSpeed;
                        ProjectileUtils.SpawnProjectile(projectile.GetOwner(),
                            projectile.Center,
                            velocity,
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
                        if (dbItem.ActiveRange != -1 && PvPUtils.FindClosestPlayer(projectile.position, projectile.owner, rangeInBlocks, owner.TPlayer.team) == null)
                            return;
                        spread = spread.Replace(-1, 30);
                        for (float degrees = -spread / 2; degrees <= spread / 2; degrees += spread) {
                            ProjectileUtils.SpawnProjectile(projectile.GetOwner(),
                                projectile.Center,
                                projectile.velocity.Rotate(degrees).Normalized() * shootSpeed,
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
