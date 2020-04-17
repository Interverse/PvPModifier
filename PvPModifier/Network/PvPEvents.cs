using System;
using PvPModifier.CustomWeaponAPI;
using PvPModifier.DataStorage;
using PvPModifier.Network.Packets;
using PvPModifier.Utilities;
using PvPModifier.Utilities.PvPConstants;
using PvPModifier.Variables;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace PvPModifier.Network {
    public class PvPEvents {
        public PvPEvents() {
            DataHandler.PlayerHurt += OnPlayerHurt;
            DataHandler.ProjectileNew += OnNewProjectile;
            DataHandler.PvPToggled += OnPvPToggled;
            DataHandler.PlayerUpdate += OnPlayerUpdate;
            DataHandler.SlotUpdate += CheckIncomingItems;
            DataHandler.SlotUpdate += CheckDrops;
        }

        public void Unsubscribe() {
            DataHandler.PlayerHurt -= OnPlayerHurt;
            DataHandler.ProjectileNew -= OnNewProjectile;
            DataHandler.PvPToggled -= OnPvPToggled;
            DataHandler.PlayerUpdate -= OnPlayerUpdate;
            DataHandler.SlotUpdate -= CheckIncomingItems;
            DataHandler.SlotUpdate -= CheckDrops;
        }

        /// <summary>
        /// Runs every 1/60th second to reset any inactive projectiles.
        /// </summary>
        public static void CleanupInactiveProjectiles(EventArgs args) {
            for (int x = 0; x < Main.maxProjectiles; x++) {
                if (!Main.projectile[x].active)
                    Main.projectile[x] = new Projectile();
            }
        }

        /// <summary>
        /// Handles homing projectiles.
        /// </summary>
        public static void UpdateProjectileHoming(ProjectileAiUpdateEventArgs args) {
            if (!PvPModifier.Config.EnableHoming) return;
            
            var projectile = args.Projectile;

            float homingRadius = Cache.Projectiles[projectile.type].HomingRadius;
            if (homingRadius < 0) return;

            float angularVelocity = Cache.Projectiles[projectile.type].AngularVelocity;
            
            PvPPlayer target = PvPUtils.FindClosestPlayer(projectile.position, projectile.owner, homingRadius * Constants.PixelToWorld);
            
            if (target != null) {
                projectile.velocity = MiscUtils.TurnTowards(projectile.velocity, projectile.position, target.TPlayer.Center, angularVelocity);
                foreach (var pvper in PvPModifier.ActivePlayers) {
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
        /// Mods the player's inventory when their pvp is turned on.
        /// Resets their inventory to Terraria's defaults when their pvp is turned off.
        /// </summary>
        private void OnPvPToggled(object sender, TogglePvPArgs e) {
            if (!PvPModifier.Config.EnablePlugin) return;

            if (e.Hostile) {
                e.Player.InvTracker.StartForcePvPInventoryCheck = true;
                PvPUtils.SendCustomItems(e.Player);
            }

            if (!e.Hostile) {
                PvPUtils.RefreshInventory(e.Player);
                e.Player.InvTracker.Clear();
                e.Player.InvTracker.StartForcePvPInventoryCheck = false;
            }
        }

        /// <summary>
        /// Replaces items placed into the inventory into the pvp versions.
        /// </summary>
        private void CheckIncomingItems(object sender, PlayerSlotArgs e) {
            if (!PvPModifier.Config.EnablePlugin) return;

            if (!e.Player.TPlayer.hostile) return;
            if (e.SlotId >= 58) return;

            //Only runs after initial pvp check
            if (e.Player.InvTracker.OnPvPInventoryChecked) {
                if (e.Player.InvTracker.LockModifications) return;

                //If the item is being consumed, don't modify the item
                if (Math.Abs(e.Player.TPlayer.inventory[e.SlotId].stack - e.Stack) <= 1
                    && e.Player.TPlayer.inventory[e.SlotId].netID == e.NetID) return;
                
                //If the item is modified, fill empty spaces and add it to queue
                if (PvPUtils.IsModifiedItem(e.NetID)) {
                    SSCUtils.FillInventoryToIndex(e.Player, Constants.EmptyItem, Constants.JunkItem, e.SlotId);
                    SSCUtils.SetItem(e.Player, e.SlotId, Constants.EmptyItem);
                    e.Player.InvTracker.AddItem(PvPUtils.GetCustomWeapon(e.Player, e.NetID, e.Prefix, e.Stack));
                    e.Player.InvTracker.StartDroppingItems();
                }
            }
        }

        /// <summary>
        /// Checks if the player has received their modified items.
        /// </summary>
        private void CheckDrops(object sender, PlayerSlotArgs e) {
            if (!PvPModifier.Config.EnablePlugin) return;

            if (e.Player.TPlayer.dead) return;
            if (!e.Player.TPlayer.hostile) return;

            //This method runs right after the replacement method, and since the PlayerSlotArgs will be the same,
            //we add this check so the item doesn't instantly get checked off as being modified
            if (e.Player.InvTracker.StartPvPInventoryCheck) {
                e.Player.InvTracker.StartPvPInventoryCheck = false;
                return;
            }

            e.Player.InvTracker.CheckFinishedModifications(e.NetID);
        }

        /// <summary>
        /// Handles player updates.
        /// </summary>
        private void OnPlayerUpdate(object sender, PlayerUpdateArgs e) {
            if (!PvPModifier.Config.EnablePlugin) return;
            if (e.Player.TPlayer.dead) return;

            //If the player has their pvp turned on without sending a TogglePvP packet (ex. through a /pvp command),
            //The plugin will detect it here and send the modified items.
            if (e.Player.TPlayer.hostile && !e.Player.InvTracker.StartForcePvPInventoryCheck) {
                e.Player.InvTracker.StartForcePvPInventoryCheck = true;
                PvPUtils.SendCustomItems(e.Player);
            }

            if (!e.Player.TPlayer.hostile && e.Player.InvTracker.StartForcePvPInventoryCheck) {
                PvPUtils.RefreshInventory(e.Player);
                e.Player.InvTracker.Clear();
                e.Player.InvTracker.StartForcePvPInventoryCheck = false;
            }
            
            //If the player tries to use a modified item in their hand, it will be dumped back into their inventory
            if ((e.PlayerAction & 32) == 32) {
                if (e.Player.TPlayer.hostile) {
                    Item item = e.Player.TPlayer.inventory[58];

                    if (item.netID != 0 && PvPUtils.IsModifiedItem(item.netID) && e.Player.CanModInventory()) {
                        SSCUtils.SetItem(e.Player, 58, 0);
                        CustomWeaponDropper.DropItem(e.Player, new CustomWeapon {
                            ItemNetId = (short)item.netID, Prefix = item.prefix, Stack = (short)item.stack,
                            DropAreaWidth = short.MaxValue, DropAreaHeight = short.MaxValue
                        });
                        e.Player.SendErrorMessage("You cannot use this weapon in your hand!");
                    }
                }
            }
        }

        /// <summary>
        /// Handles projectile creation.
        /// </summary>
        private void OnNewProjectile(object sender, ProjectileNewArgs e) {
            if (!PvPModifier.Config.EnablePlugin) return;
            var projectile = Main.projectile[e.Identity];
            if (projectile.active && projectile.type == e.Type) return;
            
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
            
            e.Attacker.ProjTracker.InsertProjectile(e.Identity, e.Type, e.Owner, e.Weapon);
            e.Attacker.ProjTracker.Projectiles[e.Type].PerformProjectileAction();
        }

        /// <summary>
        /// Handles pvp attacks.
        /// </summary>
        private void OnPlayerHurt(object sender, PlayerHurtArgs e) {
            if (!PvPModifier.Config.EnablePlugin) return;          
            e.Args.Handled = true;
           
            if (e.Attacker.TPlayer.immune || !e.Target.CanBeHit()) return;

            if (PvPModifier.Config.EnableKnockback) {
                int direction = e.HitDirection;
                double angle;
                if (e.PlayerHitReason.SourceProjectileIndex != -1) {
                    angle = Math.Atan2(e.Projectile.velocity.Y, e.Projectile.velocity.X);
                    direction = e.Target.IsLeftFrom(e.Projectile.position) ? -direction : direction;
                }
                else { 
                    angle = e.Attacker.AngleFrom(e.Target.TPlayer.position);
                    direction = e.Target.IsLeftFrom(e.Attacker.TPlayer.position) ? -direction : direction;
                }
                e.Target.KnockBack(e.Weapon.GetKnockback(e.Attacker),
                   angle,
                   direction);
                e.HitDirection = 0;
            }
            
            e.Target.DamagePlayer(PvPUtils.GetPvPDeathMessage(e.PlayerHitReason.GetDeathText(e.Target.Name).ToString(), e.Weapon, e.Projectile),
                e.Weapon, e.InflictedDamage, e.HitDirection, (e.Flag & 1) == 1);

            e.Attacker.ApplyPvPEffects(e.Target, e.Weapon, e.Projectile, e.InflictedDamage);

            if (PvPModifier.Config.EnableBuffs) {
                e.Target.SetBuff(Cache.Projectiles[e.PlayerHitReason.SourceProjectileType].InflictBuff);
                e.Attacker.SetBuff(Cache.Projectiles[e.PlayerHitReason.SourceProjectileType].ReceiveBuff);
                e.Target.SetBuff(Cache.Items[e.Attacker.HeldItem.netID].InflictBuff);
                e.Attacker.SetBuff(Cache.Items[e.Attacker.HeldItem.netID].ReceiveBuff);
                e.Target.ApplyBuffDebuffs(e.Attacker, e.Weapon);
                e.Attacker.ApplyReceiveBuff();
            }
        }
    }
}
