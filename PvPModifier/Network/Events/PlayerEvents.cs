using CustomWeaponAPI;
using PvPModifier.DataStorage;
using PvPModifier.Network.Packets;
using PvPModifier.Utilities;
using PvPModifier.Utilities.PvPConstants;
using PvPModifier.Variables;
using Terraria;

namespace PvPModifier.Network.Events {
    public class PlayerEvents {
        /// <summary>
        /// Mods the player's inventory when their pvp is turned on.
        /// Resets their inventory to Terraria's defaults when their pvp is turned off.
        /// </summary>
        public static async void OnPvPToggledAsync(object sender, TogglePvPArgs e) {
            if (!PvPModifier.Config.EnablePlugin) return;

            if (e.Hostile) {
                e.Player.GetInvTracker().StartForcePvPInventoryCheck = true;
                await PvPUtils.SendCustomItemsAsync(e.Player);
            }

            if (!e.Hostile) {
                PvPUtils.RefreshInventory(e.Player);
                e.Player.GetInvTracker().Clear();
                e.Player.GetInvTracker().StartForcePvPInventoryCheck = false;
            }
        }

        /// <summary>
        /// Handles player updates.
        /// </summary>
        public static async void OnPlayerUpdateAsync(object sender, PlayerUpdateArgs e) {
            if (!PvPModifier.Config.EnablePlugin) return;
            if (e.Player.TPlayer.dead) return;

            //If the player has their pvp turned on without sending a TogglePvP packet (ex. through a /pvp command),
            //The plugin will detect it here and send the modified items.
            if (e.Player.TPlayer.hostile && !e.Player.GetInvTracker().StartForcePvPInventoryCheck) {
                e.Player.GetInvTracker().StartForcePvPInventoryCheck = true;
                await PvPUtils.SendCustomItemsAsync(e.Player);
            }

            if (!e.Player.TPlayer.hostile && e.Player.GetInvTracker().StartForcePvPInventoryCheck) {
                PvPUtils.RefreshInventory(e.Player);
                e.Player.GetInvTracker().Clear();
                e.Player.GetInvTracker().StartForcePvPInventoryCheck = false;
            }

            //If the player tries to use a modified item in their hand, it will be dumped back into their inventory
            if ((e.PlayerAction & 32) == 32) {
                if (e.Player.TPlayer.hostile) {
                    Item item = e.Player.TPlayer.inventory[58];

                    if (item.netID != 0 && PvPUtils.IsModifiedItem(item.netID) && e.Player.CanModInventory()) {
                        SSCUtils.SetItem(e.Player, 58, Constants.EmptyItem);
                        CustomWeaponDropper.DropItem(e.Player, new CustomWeapon {
                            ItemNetId = (short)item.netID,
                            Prefix = item.prefix,
                            Stack = (short)item.stack,
                            DropAreaWidth = short.MaxValue,
                            DropAreaHeight = short.MaxValue
                        });
                        e.Player.SendErrorMessage("You cannot use this weapon in your hand!");
                    }
                }
            }
        }



        /// <summary>
        /// Handles pvp attacks.
        /// </summary>
        public static void OnPlayerHurt(object sender, PlayerHurtArgs e) {
            if (!PvPModifier.Config.EnablePlugin) return;

            e.Args.Handled = true;

            if (e.Attacker.TPlayer.immune || !e.Target.CanBeHit()) return;

            if (PvPModifier.Config.EnableKnockback) {
                int direction = e.HitDirection;
                if (!e.Target.TPlayer.noKnockback || PvPModifier.Config.ForceCustomKnockback) {
                    e.Target.KnockBack(e.Weapon.GetKnockback(e.Attacker),
                        e.Attacker.AngleFrom(e.Target.TPlayer.position),
                        e.Target.IsLeftFrom(e.Attacker.TPlayer.position) ? -direction : direction);
                }
                e.HitDirection = 0;
            }

            e.Target.DamagePlayer(PvPUtils.GetPvPDeathMessage(e.PlayerHitReason.GetDeathText(e.Target.Name).ToString(), e.Weapon, e.Projectile),
                e.Weapon, e.InflictedDamage, e.HitDirection, (e.Flag & 1) == 1);

            e.Attacker.ApplyPvPEffects(e.Target, e.Weapon, e.Projectile, e.InflictedDamage);

            if (PvPModifier.Config.EnableBuffs) {
                e.Target.SetBuff(Cache.Projectiles[e.PlayerHitReason.SourceProjectileType].InflictBuff);
                e.Attacker.SetBuff(Cache.Projectiles[e.PlayerHitReason.SourceProjectileType].ReceiveBuff);
                e.Target.SetBuff(Cache.Items[e.Attacker.TPlayer.HeldItem.netID].InflictBuff);
                e.Attacker.SetBuff(Cache.Items[e.Attacker.TPlayer.HeldItem.netID].ReceiveBuff);
                e.Target.ApplyBuffDebuffs(e.Attacker, e.Weapon);
                e.Attacker.ApplyReceiveBuff();
            }
        }
    }
}
