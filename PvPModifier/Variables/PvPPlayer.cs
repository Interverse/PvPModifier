using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using PvPModifier.CustomWeaponAPI;
using PvPModifier.DataStorage;
using PvPModifier.Utilities;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using TShockAPI;
using System.Timers;
using PvPModifier.Utilities.PvPConstants;

namespace PvPModifier.Variables {
    public class PvPPlayer : TSPlayer {

        DateTime _lastHit;
        DateTime _lastInventoryModified;
        public ProjectileTracker ProjTracker = new ProjectileTracker();
        public InventoryTracker InvTracker;
        public PvPPlayer LastHitBy = null;
        public PvPItem LastHitWeapon = null;
        public PvPProjectile LastHitProjectile = null;

        private int _medusaHitCount;

        public PvPPlayer(int index) : base(index) {
            _lastHit = DateTime.Now;
            _lastInventoryModified = DateTime.Now;
            User = TShock.Players[index].User;
            InvTracker = new InventoryTracker(this);
        }

        /// <summary>
        /// Gets the item the player is currently holding.
        /// </summary>
        public PvPItem HeldItem => PvPUtils.ConvertToPvPItem(SelectedItem);

        /// <summary>
        /// Finds the player's item from its inventory.
        /// </summary>
        public PvPItem FindPlayerItem(int type) => TPlayer.FindItem(type) != -1
            ? PvPUtils.ConvertToPvPItem(TPlayer.inventory[TPlayer.FindItem(type)])
            : new PvPItem(type) { owner = Index };

        /// <summary>
        /// Gets the damage received from an attack.
        /// </summary>
        public int DamageReceived(int damage) => (int)TerrariaUtils.GetHurtDamage(this, damage);

        /// <summary>
        /// Gets the angle that a target is from the player in radians.
        /// </summary>
        public double AngleFrom(Vector2 target) => Math.Atan2(target.Y - this.Y, target.X - this.X);
        
        /// <summary>
        /// Checks whether a target is left from a player
        /// </summary>
        /// <returns>Returns true if the target is left of the player</returns>
        public bool IsLeftFrom(Vector2 target) => target.X > this.X;
        
        /// <summary>
        /// Damages players. Criticals and custom knockback will apply if enabled.
        /// </summary>
        public void DamagePlayer(string deathmessage, PvPItem weapon, int damage, int hitDirection, bool isCrit) {
            NetMessage.SendPlayerHurt(this.Index, PlayerDeathReason.ByCustomReason(deathmessage),
                damage, hitDirection, false, true, 5);
        }

        /// <summary>
        /// Sets a velocity to a player, emulating directional knockback.
        /// 
        /// This method requires SSC to be enabled. To allow knockback to work
        /// on non-SSC servers, the method will temporarily enable SSC to set player
        /// velocity.
        /// </summary>
        public void KnockBack(double knockback, double angle, double hitDirection = 1) {
            if (TPlayer.noKnockback) return;

            new SSCAction(this, () => {
                if (TPlayer.velocity.Length() <= Math.Abs(knockback)) {
                    if (TPlayer.velocity.Length() <= Math.Abs(knockback)) {
                        if (Math.Abs(TPlayer.velocity.Length() + knockback) < knockback) {
                            TPlayer.velocity.X += (float)(knockback * Math.Cos(angle) * hitDirection);
                            TPlayer.velocity.Y += (float)(knockback * Math.Sin(angle));
                        } else {
                            TPlayer.velocity.X = (float)(knockback * Math.Cos(angle) * hitDirection);
                            TPlayer.velocity.Y = (float)(knockback * Math.Sin(angle));
                        }
                    }
                    
                    NetMessage.SendData(13, -1, -1, null, Index, 0, 4);
                }
            });
        }

        /// <summary>
        /// Applies effects that normally won't work in vanilla pvp.
        /// Effects include nebula/frost armor, yoyo-bag projectiles, and thorns/turtle damage.
        /// </summary>
        public void ApplyPvPEffects(PvPPlayer attacker, PvPItem weapon, PvPProjectile projectile, int damage) {
            this.ApplyReflectDamage(attacker, damage, weapon);
            this.ApplyArmorEffects(attacker, weapon, projectile);
            TerrariaUtils.ActivateYoyoBag(this, attacker, damage, weapon.knockBack);
        }

        /// <summary>
        /// Applies turtle and thorns damage to the attacker.
        /// </summary>
        public void ApplyReflectDamage(PvPPlayer attacker, int damage, PvPItem weapon) {
            PvPItem reflectTag = new PvPItem(1150);
            Random random = new Random();
            string deathmessage = PresetData.ReflectedDeathMessages[random.Next(PresetData.ReflectedDeathMessages.Count)];

            if (PvPModifier.Config.EnableTurtle && attacker.TPlayer.setBonus == Language.GetTextValue("ArmorSetBonus.Turtle") && weapon.melee) {
                deathmessage = Name + deathmessage + attacker.Name + "'s Turtle Armor.";
                int turtleDamage = (int)(damage * PvPModifier.Config.TurtleMultiplier);

                NetMessage.SendPlayerHurt(this.Index, PlayerDeathReason.ByCustomReason(PvPUtils.GetPvPDeathMessage(deathmessage, reflectTag, type: 2)),
                    turtleDamage, 0, false, true, 5);
            }

            if (PvPModifier.Config.EnableThorns && attacker.TPlayer.FindBuffIndex(14) != -1) {
                int thornDamage = (int)(damage * PvPModifier.Config.ThornMultiplier);
                deathmessage = Name + deathmessage + attacker.Name + "'s Thorns.";

                NetMessage.SendPlayerHurt(this.Index, PlayerDeathReason.ByCustomReason(PvPUtils.GetPvPDeathMessage(deathmessage, reflectTag, type: 2)),
                    thornDamage, 0, false, true, 5);
            } 
        }

        /// <summary>
        /// Applies nebula, spectre, and frost armor effects.
        /// </summary>
        public void ApplyArmorEffects(PvPPlayer target, PvPItem weapon, PvPProjectile projectile) {
            if (TPlayer.setNebula && TPlayer.nebulaCD == 0 && Main.rand.Next(3) == 0 && PvPModifier.Config.EnableNebula) {
                TPlayer.nebulaCD = 30;
                int type = Terraria.Utils.SelectRandom(Main.rand, 3453, 3454, 3455);

                int index = Item.NewItem((int)TPlayer.position.X, (int)TPlayer.position.Y, TPlayer.width, TPlayer.height, type);

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
                    .PackByte((byte)Index)
                    .GetByteData();

                foreach (var pvper in PvPModifier.PvPers.Where(c => c != null)) {
                    pvper.SendRawData(itemDrop);
                    pvper.SendRawData(itemOwner);
                }
            }

            if ((weapon.ranged || weapon.melee) && TPlayer.frostArmor && PvPModifier.Config.EnableFrost) {
                target.SetBuff(44, (int)(PvPModifier.Config.FrostDuration * 30));
            }

            if (TPlayer.ghostHurt && projectile?.type != 356) {
                TerrariaUtils.ActivateSpectreBolt(this, target, weapon, weapon.ConfigDamage);
            }
        }

        /// <summary>
        /// Applies buffs to the attacker based off own buffs, if any.
        /// </summary>
        public void ApplyBuffDebuffs(PvPPlayer attacker, PvPItem weapon) {
            for(int x = 0; x < Terraria.Player.maxBuffs; x++) {
                int buffType = attacker.TPlayer.buffType[x];
                if (PresetData.FlaskDebuffs.ContainsKey(buffType)) {
                    if (weapon.melee) {
                        SetBuff(Cache.Buffs[buffType].InflictBuff);
                    }
                    continue;
                }
                SetBuff(Cache.Buffs[buffType].InflictBuff);
            }
        }

        /// <summary>
        /// Applies buffs to self based off a buff, if any.
        /// </summary>
        public void ApplyReceiveBuff() {
            for (int x = 0; x < Terraria.Player.maxBuffs; x++) {
                int buffType = this.TPlayer.buffType[x];
                this.SetBuff(Cache.Buffs[buffType].ReceiveBuff);
            }
        }

        /// <summary>
        /// Determines whether a person can be hit based off the config iframes.
        /// </summary>
        public bool CanBeHit() {
            if ((DateTime.Now - _lastHit).TotalMilliseconds >= PvPModifier.Config.IframeTime) {
                _lastHit = DateTime.Now;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Sets a cooldown for when a player can have their inventory modded.
        /// </summary>
        public bool CanModInventory() {
            if ((DateTime.Now - _lastInventoryModified).TotalMilliseconds >= Constants.SpawnItemDelay) {
                _lastInventoryModified = DateTime.Now;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Sets a buff to the player based off <see cref="BuffInfo"/>
        /// </summary>
        public void SetBuff(BuffInfo buffInfo) => SetBuff(buffInfo.BuffId, buffInfo.BuffDuration);
        
        /// <summary>
        /// Determines whether a person can be hit with Medusa Head.
        /// A normal Medusa attack hits six times at once, so this method
        /// limits it down to one hit per attack.
        /// </summary>
        /// <returns></returns>
        public bool CheckMedusa() {
            _medusaHitCount++;
            if (_medusaHitCount != 1) {
                if (_medusaHitCount == 6) _medusaHitCount = 0;
                return false;
            }

            return true;
        }

        public static bool operator == (PvPPlayer obj1, PvPPlayer obj2) => obj1?.Index == obj2?.Index;
        public static bool operator != (PvPPlayer obj1, PvPPlayer obj2) => obj1?.Index != obj2?.Index;
    }

    /// <summary>
    /// Stores the weapon used to the projectile that was shot.
    /// </summary>
    public class ProjectileTracker {
        public PvPProjectile[] Projectiles = new PvPProjectile[Main.maxProjectileTypes];

        public ProjectileTracker() {
            for (int x = 0; x < Projectiles.Length; x++) {
                Projectiles[x] = new PvPProjectile(0);
            }
        }

        public void InsertProjectile(int index, int projectileType, int ownerIndex, PvPItem item) {
            var projectile = new PvPProjectile(projectileType, index, ownerIndex, item);
            Projectiles[projectileType] = projectile;
        }
    }

    /// <summary>
    /// Helper class that handles inventory modifications.
    /// </summary>
    public class InventoryTracker {
        private readonly PvPPlayer _player;
        private readonly List<CustomWeapon> _inv = new List<CustomWeapon>();

        private readonly Timer _timer;
        private int _counter;
        
        public bool LockModifications;
        public bool FinishedModifications;
        public bool OnPvPInventoryChecked;
        public bool StartPvPInventoryCheck;

        public InventoryTracker(PvPPlayer player) {
            _player = player;
            _timer = new Timer(Constants.RetryInventoryTime);
            _timer.Elapsed += DropModifiedItems;
        }

        public void AddItem(CustomWeapon wep) {
            LockModifications = false;
            FinishedModifications = false;
            _inv.Add(wep);
        }

        public void StartDroppingItems() {
            DropModifiedItems();
            _timer.Enabled = true;
        }

        private void DropModifiedItems(object sender = null, ElapsedEventArgs e = null) {
            foreach (var wep in _inv)
                CustomWeaponDropper.DropItem(_player, wep);

            _counter++;
            if (_counter >= 10) {
                Clear();
                CheckFinishedModifications(0);
            }
        }

        public bool ContainsItem(short id) {
            return _inv.Contains(new CustomWeapon { ItemNetId = id });
        }

        public void CheckItem(short id) {
            _inv.Remove(new CustomWeapon {ItemNetId = id});

            if (_inv.Count == 0 && !LockModifications) {
                LockModifications = true;
                FinishedModifications = true;
            }
        }

        public bool CheckFinishedModifications(short id) {
            CheckItem(id);
            if (LockModifications && FinishedModifications || _player == null) {
                SSCUtils.FillInventory(_player, Constants.JunkItem, 0);
                FinishedModifications = false;
                OnPvPInventoryChecked = true;
                Clear();
                return true;
            }

            return false;
        }

        public void Clear() {
            _inv.Clear();
            _timer.Enabled = false;
            _counter = 0;
        }
    }
}
