using System;
using System.Linq;
using Microsoft.Xna.Framework;
using PvPModifier.CustomWeaponAPI;
using PvPModifier.DataStorage;
using PvPModifier.Network.Packets;
using PvPModifier.Utilities;
using PvPModifier.Utilities.PvPConstants;
using PvPModifier.Variables;
using Terraria;
using TShockAPI;

namespace PvPModifier.Network {
    public class PvPEvents {
        public PvPEvents() {
            DataHandler.PlayerHurt += OnPlayerHurt;
            DataHandler.ProjectileNew += OnNewProjectile;
            DataHandler.PvPToggled += OnPvPToggled;
            DataHandler.PlayerUpdate += OnPlayerUpdate;
            DataHandler.SlotUpdate += OnSlotUpdate;
        }

        public void Unsubscribe() {
            DataHandler.PlayerHurt -= OnPlayerHurt;
            DataHandler.ProjectileNew -= OnNewProjectile;
            DataHandler.PvPToggled -= OnPvPToggled;
            DataHandler.PlayerUpdate -= OnPlayerUpdate;
            DataHandler.SlotUpdate -= OnSlotUpdate;
        }

        public static void OnUpdate(EventArgs args) {
            foreach (var projectile in PvPModifier.Projectiles) {
                if (projectile == null) continue;

                if (!projectile.MainProjectile.active) {
                    projectile.MainProjectile.SetDefaultsDirect(0);
                    continue;
                }

                float closestPersonDistance = -1;
                PvPPlayer target = null;
                foreach (var pvper in PvPModifier.PvPers.Where(c => c != null && c.TPlayer.hostile && !c.TPlayer.dead)) {
                    var distance = Vector2.Distance(projectile.MainProjectile.position, pvper.TPlayer.position);

                    if (pvper != projectile.OwnerProjectile && (distance < closestPersonDistance || closestPersonDistance == -1) && distance < 200) {
                        closestPersonDistance = distance;
                        target = pvper;
                    }
                }

                if (target != null) {
                    projectile.MainProjectile.velocity = 
                        MiscUtils.TurnTowards(projectile.MainProjectile.velocity, projectile.MainProjectile.position, target.TPlayer.Center, Math.PI / 2);
                    if (Main.netMode == 2) {
                        //Schedule updates to occur every server tick (once every 1/60th second)
                        if (projectile.netSpam > 0) projectile.netSpam = 0;
                        projectile.netUpdate = true;
                        projectile.netUpdate2 = true;
                        NetMessage.SendData(27, -1, -1, null, projectile.identity);
                    }
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
                e.Player.InvTracker.StartPvPInventoryCheck = true;
                PvPUtils.SendCustomItems(e.Player);
            }

            if (!e.Hostile) {
                PvPUtils.RefreshInventory(e.Player);
                e.Player.InvTracker.Clear();
                e.Player.InvTracker.StartPvPInventoryCheck = false;
            }
        }

        /// <summary>
        /// Handles inventory slot updates.
        /// </summary>
        private void OnSlotUpdate(object sender, PlayerSlotArgs e) {
            if (!PvPModifier.Config.EnablePlugin) return;

            if (e.Player.TPlayer.dead) return;
            if (!e.Player.TPlayer.hostile) return;
            if (e.SlotId >= 58) return;
            if (e.NetID == Constants.JunkItem || e.NetID == Constants.EmptyItem) return;

            //If the inventory tracker finished modding the player's inventory,
            //the code stops in order for the hook to not detect the just modified item
            //which would have cause an infinite loop.
            if (e.Player.InvTracker.CheckFinishedModifications(e.NetID)) return;

            //Only runs after initial pvp check
            if (e.Player.InvTracker.OnPvPInventoryChecked) {
                //If the item is being consumed, don't modify the item
                if (Math.Abs(e.Player.TPlayer.inventory[e.SlotId].stack - e.Stack) <= 1
                    && e.Player.TPlayer.inventory[e.SlotId].netID == e.NetID) return;

                //If we pick up an item and it hasn't already been added to the queue
                if (!e.Player.InvTracker.ContainsItem(e.NetID)) {
                    //If the item is modified, fill empty spaces and add it to queue
                    if (PvPUtils.IsModifiedItem(e.NetID)) {
                        SSCUtils.FillInventoryToIndex(e.Player, Constants.EmptyItem, Constants.JunkItem, e.SlotId);
                        SSCUtils.SetItem(e.Player, e.SlotId, Constants.EmptyItem);
                        e.Player.InvTracker.AddItem(PvPUtils.GetCustomWeapon(e.Player, e.NetID, e.Prefix, e.Stack));
                        e.Player.InvTracker.StartDroppingItems();
                    }
                }
            }
        }

        /// <summary>
        /// Handles player updates.
        /// </summary>
        private void OnPlayerUpdate(object sender, PlayerUpdateArgs e) {
            if (!PvPModifier.Config.EnablePlugin) return;
            if (e.Player.TPlayer.dead) return;

            //If the player has their pvp turned on without sending a TogglePvP packet (ex. through a /pvp command),
            //The plugin will detect it here and send the modified items.
            if (e.Player.TPlayer.hostile && !e.Player.InvTracker.StartPvPInventoryCheck) {
                e.Player.InvTracker.StartPvPInventoryCheck = true;
                PvPUtils.SendCustomItems(e.Player);
            }

            if (!e.Player.TPlayer.hostile && e.Player.InvTracker.StartPvPInventoryCheck) {
                PvPUtils.RefreshInventory(e.Player);
                e.Player.InvTracker.Clear();
                e.Player.InvTracker.StartPvPInventoryCheck = false;
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
                int direction = e.HitDirection - 1;
                e.Target.KnockBack(e.Weapon.GetKnockback(e.Attacker),
                    e.Attacker.AngleFrom(e.Target.TPlayer.position),
                    e.Target.IsLeftFrom(e.Attacker.TPlayer.position) ? -direction : direction);
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
