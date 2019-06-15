using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using PvPModifier.DataStorage;
using PvPModifier.Utilities;
using Terraria;
using Terraria.DataStructures;
using Terraria.Localization;
using TShockAPI;
using PvPModifier.Utilities.PvPConstants;
using System.Threading.Tasks;
using CustomWeaponAPI;

namespace PvPModifier.Variables {
    public static class TSPlayerExtension {
        private static void SetLastHit(this TSPlayer player, DateTime time) {
            player.SetData("_lastHit", time);
        }

        private static DateTime GetLastHit(this TSPlayer player) {
            return player.GetData<DateTime>("_lastHit");
        }

        private static void SetLastInventoryModified(this TSPlayer player, DateTime time) {
            player.SetData("_lastInventoryModified", time);
        }

        private static DateTime GetLastInventoryModified(this TSPlayer player) {
            return player.GetData<DateTime>("_lastInventoryModified");
        }

        private static void SetMedusaHitCount(this TSPlayer player, int count) {
            player.SetData("_medusaHitCount", count);
        }

        private static int GetMedusaHitCount(this TSPlayer player) {
            return player.GetData<int>("_medusaHitCount");
        }

        public static ProjectileTracker GetProjectileTracker(this TSPlayer player) {
            return player.GetData<ProjectileTracker>("ProjectileTracker");
        }

        public static InventoryTracker GetInvTracker(this TSPlayer player) {
            return player.GetData<InventoryTracker>("InventoryTracker");
        }

        public static void Initialize(this TSPlayer player) {
            player.SetLastHit(DateTime.Now);
            player.SetLastInventoryModified(DateTime.Now);
            player.SetData<ProjectileTracker>("ProjectileTracker", new ProjectileTracker());
            player.SetData<InventoryTracker>("InventoryTracker", new InventoryTracker(player));
        }

        /// <summary>
        /// Finds the player's item from its inventory.
        /// </summary>
        public static Item FindPlayerItem(this TSPlayer player, int type) {
            var item = new Item();

            if (player.TPlayer.FindItem(type) != -1) {
                return player.TPlayer.inventory[player.TPlayer.FindItem(type)];
            }

            return item;
        }
        
        /// <summary>
        /// Gets the damage received from an attack.
        /// </summary>
        public static int GetDamageReceived(this TSPlayer player, int damage) => (int)TerrariaUtils.GetHurtDamage(player, damage);

        /// <summary>
        /// Gets the angle that a target is from the player in radians.
        /// </summary>
        public static double AngleFrom(this TSPlayer player, Vector2 target) => Math.Atan2(target.Y - player.Y, target.X - player.X);
        
        /// <summary>
        /// Checks whether a target is left from a player
        /// </summary>
        /// <returns>Returns true if the target is left of the player</returns>
        public static bool IsLeftFrom(this TSPlayer player, Vector2 target) => target.X > player.X;
        
        /// <summary>
        /// Damages players. Criticals and custom knockback will apply if enabled.
        /// </summary>
        public static void DamagePlayer(this TSPlayer player, string deathmessage, Item weapon, int damage, int hitDirection, bool isCrit) {
            NetMessage.SendPlayerHurt(player.Index, PlayerDeathReason.ByCustomReason(deathmessage),
                damage, hitDirection, false, true, 5);
        }

        /// <summary>
        /// Sets a velocity to a player, emulating directional knockback.
        /// 
        /// This method requires SSC to be enabled. To allow knockback to work
        /// on non-SSC servers, the method will temporarily enable SSC to set player
        /// velocity.
        /// </summary>
        public static void KnockBack(this TSPlayer player, double knockback, double angle, double hitDirection = 1) {
            new SSCAction(player, () => {
                if (player.TPlayer.velocity.Length() <= Math.Abs(knockback)) {
                    if (player.TPlayer.velocity.Length() <= Math.Abs(knockback)) {
                        if (Math.Abs(player.TPlayer.velocity.Length() + knockback) < knockback) {
                            player.TPlayer.velocity.X += (float)(knockback * Math.Cos(angle) * hitDirection);
                            player.TPlayer.velocity.Y += (float)(knockback * Math.Sin(angle));
                        } else {
                            player.TPlayer.velocity.X = (float)(knockback * Math.Cos(angle) * hitDirection);
                            player.TPlayer.velocity.Y = (float)(knockback * Math.Sin(angle));
                        }
                    }
                    
                    NetMessage.SendData(13, -1, -1, null, player.Index, 0, 4);
                }
            });
        }

        /// <summary>
        /// Applies effects that normally won't work in vanilla pvp.
        /// Effects include nebula/frost armor, yoyo-bag projectiles, and thorns/turtle damage.
        /// </summary>
        public static void ApplyPvPEffects(this TSPlayer player, TSPlayer attacker, Item weapon, Projectile projectile, int damage) {
            player.ApplyReflectDamage(attacker, damage, weapon);
            player.ApplyArmorEffects(attacker, weapon, projectile);
            TerrariaUtils.ActivateYoyoBag(player, attacker, damage, weapon.knockBack);
        }

        /// <summary>
        /// Applies turtle and thorns damage to the attacker.
        /// </summary>
        public static void ApplyReflectDamage(this TSPlayer player, TSPlayer attacker, int damage, Item weapon) {
            Item reflectTag = new Item();
            reflectTag.SetDefaults(1150);
            string deathmessage = PresetData.ReflectedDeathMessages.SelectRandom();

            if (PvPModifier.Config.EnableTurtle && attacker.TPlayer.setBonus == Language.GetTextValue("ArmorSetBonus.Turtle") && weapon.melee) {
                deathmessage = player.Name + deathmessage + attacker.Name + "'s Turtle Armor.";
                int turtleDamage = (int)(damage * PvPModifier.Config.TurtleMultiplier);

                NetMessage.SendPlayerHurt(player.Index, PlayerDeathReason.ByCustomReason(PvPUtils.GetPvPDeathMessage(deathmessage, reflectTag, type: 2)),
                    turtleDamage, 0, false, true, 5);
            }

            if (PvPModifier.Config.EnableThorns && attacker.TPlayer.FindBuffIndex(14) != -1) {
                int thornDamage = (int)(damage * PvPModifier.Config.ThornMultiplier);
                deathmessage = player.Name + deathmessage + attacker.Name + "'s Thorns.";

                NetMessage.SendPlayerHurt(player.Index, PlayerDeathReason.ByCustomReason(PvPUtils.GetPvPDeathMessage(deathmessage, reflectTag, type: 2)),
                    thornDamage, 0, false, true, 5);
            } 
        }

        /// <summary>
        /// Applies nebula, spectre, and frost armor effects.
        /// </summary>
        public static void ApplyArmorEffects(this TSPlayer player, TSPlayer target, Item weapon, Projectile projectile) {
            if (player.TPlayer.setNebula && player.TPlayer.nebulaCD == 0 && Main.rand.Next(3) == 0 && PvPModifier.Config.EnableNebula && weapon.magic) {
                player.TPlayer.nebulaCD = 30;
                int type = new int[] { 3453, 3454, 3455 }.SelectRandom();

                int index = Item.NewItem((int)player.TPlayer.position.X, (int)player.TPlayer.position.Y, player.TPlayer.width, player.TPlayer.height, type);

                float velocityY = Main.rand.Next(-20, 1) * 0.2f;
                float velocityX = Main.rand.Next(10, 31) * 0.2f * Main.rand.Next(-1, 1).Replace(0, 1);

                var itemDrop = new PacketWriter()
                    .SetType((int)PacketTypes.UpdateItemDrop)
                    .PackInt16((short)index)
                    .PackSingle(target.TPlayer.position.X) 
                    .PackSingle(target.TPlayer.position.Y) 
                    .PackSingle(velocityX)
                    .PackSingle(velocityY)
                    .PackInt16(1)
                    .PackByte(0)
                    .PackByte(0)
                    .PackInt16((short)type)
                    .GetByteData();
                var itemOwner = new PacketWriter()
                    .SetType((int)PacketTypes.ItemOwner)
                    .PackInt16((short)index)
                    .PackByte((byte)player.Index)
                    .GetByteData();

                foreach (var pvper in PvPUtils.ActivePlayers) {
                    pvper.SendRawData(itemDrop);
                    pvper.SendRawData(itemOwner);
                }
            }

            if ((weapon.ranged || weapon.melee) && player.TPlayer.frostArmor && PvPModifier.Config.EnableFrost) {
                target.SetBuff(44, (int)(PvPModifier.Config.FrostDuration * 30));
            }

            if (player.TPlayer.ghostHurt && projectile?.type != 356) {
                TerrariaUtils.ActivateSpectreBolt(player, target, weapon, weapon.GetConfigDamage());
            }
        }

        /// <summary>
        /// Applies buffs to the attacker based off own buffs, if any.
        /// </summary>
        public static void ApplyBuffDebuffs(this TSPlayer player, TSPlayer attacker, Item weapon) {
            for(int x = 0; x < Terraria.Player.maxBuffs; x++) {
                int buffType = attacker.TPlayer.buffType[x];
                if (PresetData.FlaskDebuffs.ContainsKey(buffType)) {
                    if (weapon.melee) {
                        player.SetBuff(Cache.Buffs[buffType].InflictBuff);
                    }
                    continue;
                }
                player.SetBuff(Cache.Buffs[buffType].InflictBuff);
            }
        }

        /// <summary>
        /// Applies buffs to self based off a buff, if any.
        /// </summary>
        public static void ApplyReceiveBuff(this TSPlayer player) {
            for (int x = 0; x < Terraria.Player.maxBuffs; x++) {
                int buffType = player.TPlayer.buffType[x];
                player.SetBuff(Cache.Buffs[buffType].ReceiveBuff);
            }
        }

        /// <summary>
        /// Determines whether a person can be hit based off the config iframes.
        /// </summary>
        public static bool CanBeHit(this TSPlayer player) {
            if ((DateTime.Now - player.GetLastHit()).TotalMilliseconds >= PvPModifier.Config.IframeTime) {
                player.SetLastHit(DateTime.Now);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Sets a cooldown for when a player can have their inventory modded.
        /// </summary>
        public static bool CanModInventory(this TSPlayer player) {
            if ((DateTime.Now - player.GetLastInventoryModified()).TotalMilliseconds >= Constants.SpawnItemDelay) {
                player.SetLastInventoryModified(DateTime.Now);
                return true;
            }

            return false;
        }

        public static async Task WaitUntilNoUnmoddedItems(this TSPlayer player) {
            await Task.Run(() => {
                while (player.ConnectionAlive && PvPUtils.ContainsModifiedItem(player)) {
                    Task.Delay((int)Constants.SecondPerFrame);
                }
            });
        }

        public static async Task WaitUntilItemChanged(this TSPlayer player, int slotId, int itemID) {
            await Task.Run(() => {
                while (player.ConnectionAlive && player.TPlayer.inventory[slotId].netID != itemID) {
                    Task.Delay((int)Constants.SecondPerFrame);
                }
            });
        }

        public static async Task WaitUntilReleaseItem(this TSPlayer player) {
            await Task.Run(() => {
                while (player.ConnectionAlive && !player.TPlayer.releaseUseItem) {
                    Task.Delay((int)Constants.SecondPerFrame);
                }
            });
        }

        /// <summary>
        /// Sets a buff to the player based off <see cref="BuffInfo"/>
        /// </summary>
        public static void SetBuff(this TSPlayer player, BuffInfo buffInfo) => player.SetBuff(buffInfo.BuffId, buffInfo.BuffDuration);
        
        /// <summary>
        /// Determines whether a person can be hit with Medusa Head.
        /// A normal Medusa attack hits six times at once, so this method
        /// limits it down to one hit per attack.
        /// </summary>
        /// <returns></returns>
        public static bool CheckMedusa(this TSPlayer player) {
            player.SetMedusaHitCount(player.GetMedusaHitCount() + 1);
            if (player.GetMedusaHitCount() != 1) {
                if (player.GetMedusaHitCount() == 6) player.SetMedusaHitCount(0);
                return false;
            }

            return true;
        }
    }

    /// <summary>
    /// Stores the weapon used to the projectile that was shot.
    /// </summary>
    public class ProjectileTracker {
        public Projectile[] Projectiles = new Projectile[Main.maxProjectileTypes];

        public ProjectileTracker() {
            for (int x = 0; x < Projectiles.Length; x++) {
                Projectiles[x] = new Projectile();
                Projectiles[x].SetDefaults(0);
            }
        }

        public void InsertProjectile(int index, int projectileType, int ownerIndex, int itemID) {
            var projectile = Main.projectile[index];
            projectile.owner = ownerIndex;
            projectile.SetItemOriginated(itemID);
            Projectiles[projectileType] = projectile;
        }
    }

    /// <summary>
    /// Helper class that handles inventory modifications.
    /// </summary>
    public class InventoryTracker {
        private readonly TSPlayer _player;
        private readonly List<CustomWeapon> _inv = new List<CustomWeapon>();

        public bool LockModifications;
        public bool OnPvPInventoryChecked;
        public bool StartForcePvPInventoryCheck;
        public bool StartPvPInventoryCheck;

        public InventoryTracker(TSPlayer player) {
            _player = player;
        }

        public void AddItem(CustomWeapon wep) {
            LockModifications = true;
            StartPvPInventoryCheck = true;
            _inv.Add(wep);
        }

        public void DropModifiedItems() {
            foreach (var wep in _inv)
                CustomWeaponDropper.DropItem(_player, wep);
        }

        public bool CheckItem(short id) {
            _inv.Remove(new CustomWeapon {ItemNetId = id});

            if (_inv.Count == 0 && LockModifications) {
                LockModifications = false;
            }

            return !LockModifications;
        }

        public bool CheckFinishedModifications(short id) {
            if (CheckItem(id) || _player == null) {
                SSCUtils.FillInventory(_player, Constants.JunkItem, Constants.EmptyItem);
                LockModifications = false;
                OnPvPInventoryChecked = true;
                Clear();
                return true;
            }

            return false;
        }

        public void Clear() {
            _inv.Clear();
        }
    }
}
