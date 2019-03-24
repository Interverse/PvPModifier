using System;
using PvPModifier.CustomWeaponAPI;
using PvPModifier.DataStorage;
using PvPModifier.Network.Packets;
using PvPModifier.Utilities;
using PvPModifier.Utilities.PvPConstants;
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

        private void OnSlotUpdate(object sender, PlayerSlotArgs e) {
            if (!PvPModifier.Config.EnablePlugin) return;

            if (!e.Player.TPlayer.hostile) return;
            if (e.SlotId >= 58) return;
            if (e.NetID == Constants.JunkItem || e.NetID == 0) return;

            if (e.Player.InvTracker.CheckFinishedModifications((short)e.NetID)) return;

            //Only runs after initial pvp check
            if (e.Player.InvTracker.OnPvPInventoryChecked) {
                //If the item is being consumed, ignore
                if (Math.Abs(e.Player.TPlayer.inventory[e.SlotId].stack - e.Stack) <= 1
                    && e.Player.TPlayer.inventory[e.SlotId].netID == e.NetID) return;

                //If we pick up an item and it hasn't already been added to the queue
                if (!e.Player.InvTracker.ContainsItem((short)e.NetID)) {
                    //If the item is modified, fill empty spaces and add it to queue
                    if (PvPUtils.IsModifiedItem(e.NetID)) {
                        SSCUtils.FillInventoryToIndex(e.Player, 0, Constants.JunkItem, e.SlotId);
                        SSCUtils.SetItem(e.Player, (byte)e.SlotId, 0);
                        e.Player.InvTracker.AddItem(PvPUtils.GetCustomWeapon(e.Player, e.NetID, (byte)e.Prefix, (short)e.Stack));
                        e.Player.InvTracker.StartDroppingItems();
                    }
                }
            }
        }

        private void OnPlayerUpdate(object sender, PlayerUpdateArgs e) {
            if (!PvPModifier.Config.EnablePlugin) return;

            if (e.Player.TPlayer.hostile && !e.Player.InvTracker.StartPvPInventoryCheck) {
                e.Player.InvTracker.StartPvPInventoryCheck = true;
                PvPUtils.SendCustomItems(e.Player);
            }

            if (!e.Player.TPlayer.hostile && e.Player.InvTracker.StartPvPInventoryCheck) {
                PvPUtils.RefreshInventory(e.Player);
                e.Player.InvTracker.Clear();
                e.Player.InvTracker.StartPvPInventoryCheck = false;
            }
            
            if ((e.PlayerAction & 32) == 32) {
                if (e.Player.TPlayer.hostile) {
                    Item item = e.Player.TPlayer.inventory[58];

                    if (item.netID != 0 && PvPUtils.IsModifiedItem(item.netID) && e.Player.CanModInventory()) {
                        SSCUtils.SetItem(e.Player, 58, 0);
                        CustomWeaponDropper.DropItem(e.Player, new CustomWeapon {
                            ItemNetId = (short)item.netID, Prefix = item.prefix, Stack = (short)item.stack
                        });
                        e.Player.SendErrorMessage("You cannot use this weapon in your hand!");
                    }
                }
            }
        }

        private void OnNewProjectile(object sender, ProjectileNewArgs e) {
            if (!PvPModifier.Config.EnablePlugin) return;
            if (Main.projectile[e.Identity].active && Main.projectile[e.Identity].type == e.Type) return;

            if (TShock.Players[e.Owner].TPlayer.hostile && PvPUtils.IsModifiedProjectile(e.Type)) {
                e.Args.Handled = true;
                DbProjectile proj = Cache.Projectiles[e.Type];

                var projectile = Main.projectile[e.Identity];
                projectile.SetDefaults(proj.Shoot != -1 ? proj.Shoot : e.Type);
                projectile.velocity = e.Velocity * proj.VelocityMultiplier;
                projectile.damage = proj.Damage != -1 ? TerrariaUtils.GetWeaponDamage(e.Attacker, e.Proj.ItemOriginated, proj.Damage) : e.Damage;
                projectile.active = true;
                projectile.identity = e.Identity;
                projectile.owner = e.Owner;
                projectile.position = e.Position;

                NetMessage.SendData(27, -1, -1, null, e.Identity);
            }

            e.Attacker.ProjTracker.InsertProjectile(e.Identity, e.Type, e.Owner, e.Weapon);
            e.Attacker.ProjTracker.Projectiles[e.Type].PerformProjectileAction();
        }

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

            e.Target.DamagePlayer(PvPUtils.GetPvPDeathMessage(e.PlayerHitReason.GetDeathText(e.Target.Name).ToString(), e.Weapon),
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
