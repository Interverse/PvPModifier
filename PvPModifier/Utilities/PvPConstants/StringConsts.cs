namespace PvPModifier.Utilities.PvPConstants {
    public static class StringConsts {
        public const string Config = "Config";
        public const string Database = "Database";
        public const string Help = "Help";

        /// <summary>
        /// Gets the section name from a string.
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

        /// <summary>
        /// Gets a database attribute based off the given input.
        /// </summary>
        /// <param name="input">The input string</param>
        /// <param name="attribute">Returns a <see cref="DbConsts"/> string if input matches a value, or itself if it matches nothing</param>
        /// <returns>A boolean whether the input matches a database attribute</returns>
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

                case "homingradius":
                case "hr":
                    attribute = DbConsts.HomingRadius;
                    return true;

                case "angularvelocity":
                case "av":
                    attribute = DbConsts.AngularVelocity;
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

        /// <summary>
        /// Gets a config value based off the given input.
        /// </summary>
        /// <param name="input">The input string</param>
        /// <param name="attribute">
        /// Returns a <see cref="ConfigConsts"/> string if input matches a value, or itself if it matches nothing
        /// </param>
        /// <returns>A boolean whether the input matches a config value</returns>
        public static bool TryGetConfigValueFromString(string input, out string attribute) {
            switch (input.ToLower()) {
                case "enableplugin":
                case "plugin":
                case "ep":
                case "p":
                    attribute = ConfigConsts.EnablePlugin;
                    return true;

                case "enableknockback":
                case "knockback":
                case "ekb":
                case "kb":
                case "ek":
                case "k":
                    attribute = ConfigConsts.EnableKnockback;
                    return true;

                case "enablehoming":
                case "homing":
                case "eh":
                case "h":
                    attribute = ConfigConsts.EnableHoming;
                    return true;

                case "enablespectremask":
                case "spectremask":
                case "esm":
                case "sm":
                    attribute = ConfigConsts.EnableSpectreMask;
                    return true;

                case "enableturtle":
                case "turtle":
                case "etu":
                    attribute = ConfigConsts.EnableTurtle;
                    return true;

                case "enablethorns":
                case "thorns":
                case "eth":
                    attribute = ConfigConsts.EnableThorns;
                    return true;

                case "enablenebula":
                case "nebula":
                case "en":
                case "n":
                    attribute = ConfigConsts.EnableNebula;
                    return true;

                case "enablebuffs":
                case "buffs":
                case "eb":
                case "b":
                    attribute = ConfigConsts.EnableBuffs;
                    return true;

                case "enablefrost":
                case "frost":
                case "ef":
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
