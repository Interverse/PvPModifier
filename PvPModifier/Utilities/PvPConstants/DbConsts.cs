﻿namespace PvPModifier.Utilities.PvPConstants {
    /// <summary>
    /// Contains variable names of database objects into strings
    /// </summary>
    public static class DbConsts {
        public const string ID = "ID";
        public const string Damage = "Damage";
        public const string Knockback = "Knockback";
        public const string UseAnimation = "UseAnimation";
        public const string UseTime = "UseTime";
        public const string Shoot = "Shoot";
        public const string ShootSpeed = "ShootSpeed";
        public const string VelocityMultiplier = "VelocityMultiplier";
        public const string HomingRadius = "HomingRadius";
        public const string AngularVelocity = "AngularVelocity";
        public const string AmmoIdentifier = "AmmoIdentifier";
        public const string UseAmmoIdentifier = "UseAmmoIdentifier";
        public const string NotAmmo = "NotAmmo";

        public const string InflictBuffID = "InflictBuffID";
        public const string InflictBuffDuration = "InflictBuffDuration";
        public const string ReceiveBuffID = "ReceiveBuffID";
        public const string ReceiveBuffDuration = "ReceiveBuffDuration";

        public const string Mirror = "Mirror";
        public const string Spread = "Spread";
        public const string RandomSpread = "RandomSpread";
        public const string NumShots = "NumShots";
        public const string ProjectilePool = "ProjectilePool";

        public const string ActiveProjectileAI = "ActiveProjectileAI";
        public const string ActiveProjectilePool = "ActiveProjectilePool";
        public const string ActiveRange = "ActiveRange";
        public const string ActiveFireRate = "ActiveFireRate";
        public const string ActiveSpread = "ActiveSpread";
        public const string ActiveShootSpeed = "ActiveShootSpeed";
    }

    /// <summary>
    /// Contains variables that have the SQL table names.
    /// </summary>
    public static class DbTables {
        public const string ItemTable = "Items";
        public const string ProjectileTable = "Projectiles";
        public const string BuffTable = "Buffs";
    }
}
