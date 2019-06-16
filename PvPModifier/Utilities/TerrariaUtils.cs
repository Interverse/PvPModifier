using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using TShockAPI;

namespace PvPModifier.Utilities {
    /// <summary>
    /// Methods ripped off Terraria's source code to be emulated in the plugin.
    /// </summary>
    class TerrariaUtils {

        /// <summary>
        /// Calculates the amount of damage dealt to a player after factoring in their defense stats.
        /// </summary>
        public static double GetHurtDamage(TSPlayer damagedPlayer, int damage) {
            damagedPlayer.TPlayer.stealth = 1f;
            int damage1 = damage;
            double dmg = Main.CalculatePlayerDamage(damage1, damagedPlayer.TPlayer.statDefense);
            if (dmg >= 1.0) {
                dmg = (int)((1.0 - damagedPlayer.TPlayer.endurance) * dmg);
                if (dmg < 1.0)
                    dmg = 1.0;
                if (damagedPlayer.TPlayer.ConsumeSolarFlare()) {
                    dmg = (int)(0.7 * dmg);
                    if (dmg < 1.0)
                        dmg = 1.0;
                }
                if (damagedPlayer.TPlayer.beetleDefense && damagedPlayer.TPlayer.beetleOrbs > 0) {
                    dmg = (int)((1.0 - 0.15f * damagedPlayer.TPlayer.beetleOrbs) * dmg);
                    damagedPlayer.TPlayer.beetleOrbs = damagedPlayer.TPlayer.beetleOrbs - 1;
                    if (dmg < 1.0)
                        dmg = 1.0;
                }
                if (damagedPlayer.TPlayer.defendedByPaladin) {
                    if (damagedPlayer.TPlayer.whoAmI != Main.myPlayer) {
                        if (Main.player[Main.myPlayer].hasPaladinShield) {
                            Player player = Main.player[Main.myPlayer];
                            if (player.team == damagedPlayer.TPlayer.team && damagedPlayer.TPlayer.team != 0) {
                                float num1 = player.Distance(damagedPlayer.TPlayer.Center);
                                bool flag3 = num1 < 800.0;
                                if (flag3) {
                                    for (int index = 0; index < (int)byte.MaxValue; ++index) {
                                        if (index != Main.myPlayer && Main.player[index].active && (!Main.player[index].dead && !Main.player[index].immune) && (Main.player[index].hasPaladinShield && Main.player[index].team == damagedPlayer.TPlayer.team && Main.player[index].statLife > Main.player[index].statLifeMax2 * 0.25)) {
                                            float num2 = Main.player[index].Distance(damagedPlayer.TPlayer.Center);
                                            if ((double)num1 > num2 || num1 == num2 && index < Main.myPlayer) {
                                                flag3 = false;
                                                break;
                                            }
                                        }
                                    }
                                }
                                if (flag3) {
                                    int damage2 = (int)(dmg * 0.25);
                                    dmg = (int)(dmg * 0.75);
                                    player.Hurt(PlayerDeathReason.LegacyEmpty(), damage2, 0);
                                }
                            }
                        }
                    } else {
                        bool flag3 = false;
                        for (int index = 0; index < (int)byte.MaxValue; ++index) {
                            if (index != Main.myPlayer && Main.player[index].active && (!Main.player[index].dead && !Main.player[index].immune) && Main.player[index].hasPaladinShield && Main.player[index].team == damagedPlayer.TPlayer.team && Main.player[index].statLife > Main.player[index].statLifeMax2 * 0.25) {
                                flag3 = true;
                                break;
                            }
                        }
                        if (flag3)
                            dmg = (int)(dmg * 0.75);
                    }
                }
            }
            return dmg;
        }

        /// <summary>
        /// Gets the multiplier of a weapon's stat based off its prefix.
        /// For crits, the number returned is an integer that increases total percentage.
        /// </summary>
        public static float GetPrefixMultiplier(int prefix, Stat stat) {
            float damage = 1f;
            float knockback = 1f;
            float usetime = 1f;
            float scale = 1f;
            float velocity = 1f;
            float manacost = 1f;
            int crit = 0;
            if (prefix == 1)
                scale = 1.12f;
            else if (prefix == 2)
                scale = 1.18f;
            else if (prefix == 3) {
                damage = 1.05f;
                crit = 2;
                scale = 1.05f;
            } else if (prefix == 4) {
                damage = 1.1f;
                scale = 1.1f;
                knockback = 1.1f;
            } else if (prefix == 5)
                damage = 1.15f;
            else if (prefix == 6)
                damage = 1.1f;
            else if (prefix == 81) {
                knockback = 1.15f;
                damage = 1.15f;
                crit = 5;
                usetime = 0.9f;
                scale = 1.1f;
            } else if (prefix == 7)
                scale = 0.82f;
            else if (prefix == 8) {
                knockback = 0.85f;
                damage = 0.85f;
                scale = 0.87f;
            } else if (prefix == 9)
                scale = 0.9f;
            else if (prefix == 10)
                damage = 0.85f;
            else if (prefix == 11) {
                usetime = 1.1f;
                knockback = 0.9f;
                scale = 0.9f;
            } else if (prefix == 12) {
                knockback = 1.1f;
                damage = 1.05f;
                scale = 1.1f;
                usetime = 1.15f;
            } else if (prefix == 13) {
                knockback = 0.8f;
                damage = 0.9f;
                scale = 1.1f;
            } else if (prefix == 14) {
                knockback = 1.15f;
                usetime = 1.1f;
            } else if (prefix == 15) {
                knockback = 0.9f;
                usetime = 0.85f;
            } else if (prefix == 16) {
                damage = 1.1f;
                crit = 3;
            } else if (prefix == 17) {
                usetime = 0.85f;
                velocity = 1.1f;
            } else if (prefix == 18) {
                usetime = 0.9f;
                velocity = 1.15f;
            } else if (prefix == 19) {
                knockback = 1.15f;
                velocity = 1.05f;
            } else if (prefix == 20) {
                knockback = 1.05f;
                velocity = 1.05f;
                damage = 1.1f;
                usetime = 0.95f;
                crit = 2;
            } else if (prefix == 21) {
                knockback = 1.15f;
                damage = 1.1f;
            } else if (prefix == 82) {
                knockback = 1.15f;
                damage = 1.15f;
                crit = 5;
                usetime = 0.9f;
                velocity = 1.1f;
            } else if (prefix == 22) {
                knockback = 0.9f;
                velocity = 0.9f;
                damage = 0.85f;
            } else if (prefix == 23) {
                usetime = 1.15f;
                velocity = 0.9f;
            } else if (prefix == 24) {
                usetime = 1.1f;
                knockback = 0.8f;
            } else if (prefix == 25) {
                usetime = 1.1f;
                damage = 1.15f;
                crit = 1;
            } else if (prefix == 58) {
                usetime = 0.85f;
                damage = 0.85f;
            } else if (prefix == 26) {
                manacost = 0.85f;
                damage = 1.1f;
            } else if (prefix == 27)
                manacost = 0.85f;
            else if (prefix == 28) {
                manacost = 0.85f;
                damage = 1.15f;
                knockback = 1.05f;
            } else if (prefix == 83) {
                knockback = 1.15f;
                damage = 1.15f;
                crit = 5;
                usetime = 0.9f;
                manacost = 0.9f;
            } else if (prefix == 29)
                manacost = 1.1f;
            else if (prefix == 30) {
                manacost = 1.2f;
                damage = 0.9f;
            } else if (prefix == 31) {
                knockback = 0.9f;
                damage = 0.9f;
            } else if (prefix == 32) {
                manacost = 1.15f;
                damage = 1.1f;
            } else if (prefix == 33) {
                manacost = 1.1f;
                knockback = 1.1f;
                usetime = 0.9f;
            } else if (prefix == 34) {
                manacost = 0.9f;
                knockback = 1.1f;
                usetime = 1.1f;
                damage = 1.1f;
            } else if (prefix == 35) {
                manacost = 1.2f;
                damage = 1.15f;
                knockback = 1.15f;
            } else if (prefix == 52) {
                manacost = 0.9f;
                damage = 0.9f;
                usetime = 0.9f;
            } else if (prefix == 36)
                crit = 3;
            else if (prefix == 37) {
                damage = 1.1f;
                crit = 3;
                knockback = 1.1f;
            } else if (prefix == 38)
                knockback = 1.15f;
            else if (prefix == 53)
                damage = 1.1f;
            else if (prefix == 54)
                knockback = 1.15f;
            else if (prefix == 55) {
                knockback = 1.15f;
                damage = 1.05f;
            } else if (prefix == 59) {
                knockback = 1.15f;
                damage = 1.15f;
                crit = 5;
            } else if (prefix == 60) {
                damage = 1.15f;
                crit = 5;
            } else if (prefix == 61)
                crit = 5;
            else if (prefix == 39) {
                damage = 0.7f;
                knockback = 0.8f;
            } else if (prefix == 40)
                damage = 0.85f;
            else if (prefix == 56)
                knockback = 0.8f;
            else if (prefix == 41) {
                knockback = 0.85f;
                damage = 0.9f;
            } else if (prefix == 57) {
                knockback = 0.9f;
                damage = 1.18f;
            } else if (prefix == 42)
                usetime = 0.9f;
            else if (prefix == 43) {
                damage = 1.1f;
                usetime = 0.9f;
            } else if (prefix == 44) {
                usetime = 0.9f;
                crit = 3;
            } else if (prefix == 45)
                usetime = 0.95f;
            else if (prefix == 46) {
                crit = 3;
                usetime = 0.94f;
                damage = 1.07f;
            } else if (prefix == 47)
                usetime = 1.15f;
            else if (prefix == 48)
                usetime = 1.2f;
            else if (prefix == 49)
                usetime = 1.08f;
            else if (prefix == 50) {
                damage = 0.8f;
                usetime = 1.15f;
            } else if (prefix == 51) {
                knockback = 0.9f;
                usetime = 0.9f;
                damage = 1.05f;
                crit = 2;
            }

            switch (stat) {
                case Stat.Damage:
                    return damage;
                case Stat.Critical:
                    return crit;
                case Stat.Knockback:
                    return knockback;
                case Stat.ManaCost:
                    return manacost;
                case Stat.Scale:
                    return scale;
                case Stat.Usetime:
                    return usetime;
                case Stat.Velocity:
                    return velocity;
                default:
                    return 1f;
            }
        }

        /// <summary>
        /// Spawns Counterweights/Additional yoyos when a person hits with one.
        /// This normally does not work in vanilla servers, so this must be emulated on a 
        /// server for accessories such as Yoyo Bag to work.
        /// </summary>
        public static void ActivateYoyoBag(TSPlayer attacker, TSPlayer target, int dmg, float kb) {
            if (!attacker.TPlayer.yoyoGlove && attacker.TPlayer.counterWeight <= 0)
                return;
            int index1 = -1;
            int num1 = 0;
            int num2 = 0;
            for (int index2 = 0; index2 < 1000; ++index2) {
                if (Main.projectile[index2].active && Main.projectile[index2].owner == attacker.TPlayer.whoAmI) {
                    if (Main.projectile[index2].counterweight)
                        ++num2;
                    else if (Main.projectile[index2].aiStyle == 99) {
                        ++num1;
                        index1 = index2;
                    }
                }
            }
            if (attacker.TPlayer.yoyoGlove && num1 < 2) {
                if (index1 < 0)
                    return;
                Vector2 vector21 = Vector2.Subtract(target.LastNetPosition, attacker.TPlayer.Center);
                vector21.Normalize();
                Vector2 vector22 = Vector2.Multiply(vector21, 16f);
                ProjectileUtils.SpawnProjectile(attacker, attacker.TPlayer.Center.X, attacker.TPlayer.Center.Y, vector22.X, vector22.Y, Main.projectile[index1].type, Main.projectile[index1].damage, Main.projectile[index1].knockBack, attacker.TPlayer.whoAmI, 1f, itemType: attacker.TPlayer.HeldItem.netID);
            } else {
                if (num2 >= num1)
                    return;
                Vector2 vector21 = Vector2.Subtract(target.LastNetPosition, attacker.TPlayer.Center);
                vector21.Normalize();
                Vector2 vector22 = Vector2.Multiply(vector21, 16f);
                float knockBack = (float)((kb + 6.0) / 2.0);
                ProjectileUtils.SpawnProjectile(attacker, attacker.TPlayer.Center.X, attacker.TPlayer.Center.Y,
                    vector22.X, vector22.Y, attacker.TPlayer.counterWeight, (int) (dmg * 0.8), knockBack,
                    attacker.TPlayer.whoAmI, (num2 > 0).ToInt(), itemType: attacker.TPlayer.HeldItem.netID);
            }
        }

        /// <summary>
        /// Spawns a spectre bolt where the attacker is.
        /// </summary>
        public static void ActivateSpectreBolt(TSPlayer attacker, TSPlayer target, Item weapon, int dmg) {
            if (!weapon.magic)
                return;
            int Damage = dmg / 2;
            if (dmg / 2 <= 1)
                return;
            int? attackingIndex = null;
            if (Collision.CanHit(attacker.TPlayer.position, attacker.TPlayer.width, attacker.TPlayer.height, target.TPlayer.position, target.TPlayer.width, target.TPlayer.height)) {
                attackingIndex = target.Index;
            }
            if (attackingIndex == null)
                return;
            int num3 = (int)attackingIndex;
            double num4 = 4.0;
            float num5 = (float)Main.rand.Next(-100, 101);
            float num6 = (float)Main.rand.Next(-100, 101);
            double num7 = Math.Sqrt((double)num5 * (double)num5 + (double)num6 * (double)num6);
            float num8 = (float)(num4 / num7);
            float SpeedX = num5 * num8;
            float SpeedY = num6 * num8;
            ProjectileUtils.SpawnProjectile(attacker, attacker.X, attacker.Y, SpeedX, SpeedY, 356, Damage, 0.0f, attacker.Index, (float)num3, 0.0f);
        }

        public enum Stat {
            Damage,
            Knockback,
            Usetime,
            Scale,
            Velocity,
            ManaCost,
            Critical
        }
    }
}
