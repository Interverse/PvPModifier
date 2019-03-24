using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PvPModifier.Utilities {
    public static class DbConsts {
        public const string ItemTable = "Items";
        public const string ProjectileTable = "Projectiles";
        public const string BuffTable = "Buffs";

        public const string ID = "ID";
        public const string Damage = "Damage";
        public const string Knockback = "Knockback";
        public const string UseAnimation = "UseAnimation";
        public const string UseTime = "UseTime";
        public const string Shoot = "Shoot";
        public const string ShootSpeed = "ShootSpeed";
        public const string VelocityMultiplier = "VelocityMultiplier";
        public const string AmmoIdentifier = "AmmoIdentifier";
        public const string UseAmmoIdentifier = "UseAmmoIdentifier";
        public const string NotAmmo = "NotAmmo";
        public const string InflictBuffID = "InflictBuffID";
        public const string InflictBuffDuration = "InflictBuffDuration";
        public const string ReceiveBuffID = "ReceiveBuffID";
        public const string ReceiveBuffDuration = "ReceiveBuffDuration";
    }

    public static class StringConsts {
        public const string Config = "Config";
        public const string Database = "Database";

        /// <summary>
        /// Gets the table name from a string.
        /// </summary>
        public static bool TryGetSectionFromString(string input, out string str) {
            switch (input.ToLower()) {
                case "items":
                case "item":
                case "i":
                    str = DbConsts.ItemTable;
                    break;

                case "projectiles":
                case "projectile":
                case "proj":
                case "p":
                    str = DbConsts.ProjectileTable;
                    break;

                case "buffs":
                case "buff":
                case "b":
                    str = DbConsts.BuffTable;
                    break;

                case "config":
                case "c":
                    str = Config;
                    break;

                case "database":
                case "d":
                    str = Database;
                    break;

                default:
                    str = input;
                    return false;
            }

            return true;
        }

        public static bool TryGetAttributeFromString(string input, out string attribute) {
            switch (input.ToLower()) {
                case "damage":
                case "dmg":
                case "d":
                    attribute = DbConsts.Damage;
                    return true;

                case "knockback":
                case "kb":
                case "k":
                    attribute = DbConsts.Knockback;
                    return true;

                case "usetime":
                case "ut":
                    attribute = DbConsts.UseTime;
                    return true;

                case "useanimation":
                case "ua":
                    attribute = DbConsts.UseAnimation;
                    return true;

                case "shoot":
                case "s":
                    attribute = DbConsts.Shoot;
                    return true;

                case "shootspeed":
                case "ss":
                    attribute = DbConsts.ShootSpeed;
                    return true;

                case "velocitymultiplier":
                case "vm":
                    attribute = DbConsts.VelocityMultiplier;
                    return true;

                case "ammoidentifier":
                case "ai":
                    attribute = DbConsts.AmmoIdentifier;
                    return true;

                case "useammoidentifier":
                case "uai":
                    attribute = DbConsts.UseAmmoIdentifier;
                    return true;

                case "notammo":
                case "na":
                    attribute = DbConsts.NotAmmo;
                    return true;

                case "inflictbuffid":
                case "ibid":
                    attribute = DbConsts.InflictBuffID;
                    return true;

                case "inflictbuffduration":
                case "ibd":
                    attribute = DbConsts.InflictBuffDuration;
                    return true;

                case "receivebuffid":
                case "rbid":
                    attribute = DbConsts.ReceiveBuffID;
                    return true;

                case "receivebuffduration":
                case "rbd":
                    attribute = DbConsts.ReceiveBuffDuration;
                    return true;

                default:
                    attribute = input;
                    return false;
            }
        }
    }

    public static class Constants {
        public const int JunkItem = 3853;
        public const int SpawnItemDelay = 250;
        public const int RetryInventoryTime = 1000;
    }
}
