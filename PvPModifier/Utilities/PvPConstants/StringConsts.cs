namespace PvPModifier.Utilities.PvPConstants {
    public static class StringConsts {
        public const string Config = "Config";
        public const string Database = "Database";
        public const string Help = "Help";

        /// <summary>
        /// Gets the table name from a string.
        /// </summary>
        public static bool TryGetSectionFromString(string input, out string str) {
            switch (input.ToLower()) {
                case "items":
                case "item":
                case "i":
                    str = DbTables.ItemTable;
                    break;

                case "projectiles":
                case "projectile":
                case "proj":
                case "p":
                    str = DbTables.ProjectileTable;
                    break;

                case "buffs":
                case "buff":
                case "b":
                    str = DbTables.BuffTable;
                    break;

                case "config":
                case "c":
                    str = Config;
                    break;

                case "database":
                case "d":
                    str = Database;
                    break;

                case "help":
                case "h":
                    str = Help;
                    break;

                default:
                    str = input;
                    return false;
            }

            return true;
        }

        public static bool TryGetDatabaseAttributeFromString(string input, out string attribute) {
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

        public static bool TryGetConfigValueFromString(string input, out string attribute) {
            switch (input.ToLower()) {
                case "plugin":
                case "p":
                    attribute = ConfigConsts.EnablePlugin;
                    return true;

                case "knockback":
                case "kb":
                case "k":
                    attribute = ConfigConsts.EnableKnockback;
                    return true;

                case "turtle":
                case "tu":
                    attribute = ConfigConsts.EnableTurtle;
                    return true;

                case "thorns":
                    attribute = ConfigConsts.EnableThorns;
                    return true;

                case "nebula":
                case "n":
                    attribute = ConfigConsts.EnableNebula;
                    return true;

                case "buffs":
                case "b":
                    attribute = ConfigConsts.EnableBuffs;
                    return true;

                case "frost":
                case "f":
                    attribute = ConfigConsts.EnableFrost;
                    return true;

                case "nebulatier1duration":
                case "nt1d":
                case "n1":
                    attribute = ConfigConsts.NebulaTier1Duration;
                    return true;

                case "nebulatier2duration":
                case "nt2d":
                case "n2":
                    attribute = ConfigConsts.NebulaTier2Duration;
                    return true;

                case "nebulatier3duration":
                case "nt3d":
                case "n3":
                    attribute = ConfigConsts.NebulaTier3Duration;
                    return true;

                case "frostduration":
                case "fd":
                    attribute = ConfigConsts.FrostDuration;
                    return true;

                case "turtlemultiplier":
                case "tum":
                    attribute = ConfigConsts.TurtleMultiplier;
                    return true;

                case "thornsmultiplier":
                case "thm":
                    attribute = ConfigConsts.ThornMultiplier;
                    return true;
                    
                case "iframetime":
                case "iframe":
                case "ift":
                    attribute = ConfigConsts.IframeTime;
                    return true;

                default:
                    attribute = input;
                    return false;
            }
        }
    }
}
