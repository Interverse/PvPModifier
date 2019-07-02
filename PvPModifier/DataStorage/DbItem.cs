using PvPModifier.Utilities;
using PvPModifier.Utilities.PvPConstants;

namespace PvPModifier.DataStorage {
    public class DbItem : DbObject {
        public override string Section => DbTables.ItemTable;

        private RandomPool<int> _projectilePool = null;
        private RandomPool<int> _activeProjectilePool = null;

        public int Damage;
        public float Knockback;
        public int UseAnimation;
        public int UseTime;
        public int Shoot;
        public float ShootSpeed;
        public float VelocityMultiplier;
        public int AmmoIdentifier;
        public int UseAmmoIdentifier;
        public int NotAmmo;
        public int InflictBuffID;
        public int InflictBuffDuration;
        public int ReceiveBuffID;
        public int ReceiveBuffDuration;
        public float HomingRadius;
        public float AngularVelocity;
        public float Mirror;
        public float Spread;
        public int RandomSpread;
        public int NumShots;
        public string ProjectilePool;
        public int ActiveProjectileAI;
        public string ActiveProjectilePool;
        public float ActiveRange;
        public int ActiveFireRate;
        public float ActiveShootSpeed;
        public float ActiveSpread;

        public BuffInfo InflictBuff => new BuffInfo(InflictBuffID, InflictBuffDuration);
        public BuffInfo ReceiveBuff => new BuffInfo(ReceiveBuffID, ReceiveBuffDuration);
        public bool IsNotAmmo => NotAmmo > 0;
        public bool IsMirror => Mirror > 0;
        public bool IsRandomSpread => RandomSpread > 0;

        public RandomPool<int> ProjectilePoolList {
            get {
                if (_projectilePool == null || _projectilePool.CurrentData != ProjectilePool) {
                    _projectilePool = new RandomPool<int>();

                    var pairedInputs = ProjectilePool.Split('|');

                    foreach (var inputs in pairedInputs) {
                        var pair = inputs.Split(',');

                        int projectileID = int.Parse(pair[0]);
                        double projectileChance = double.Parse(pair[1]);

                        if (projectileID < 0) continue;

                        _projectilePool.AddChance(projectileID, projectileChance);
                    }

                    _projectilePool.CurrentData = ProjectilePool;
                }

                return _projectilePool;
            }
        }

        public RandomPool<int> ActiveProjectilePoolList {
            get {
                if (_activeProjectilePool == null || _activeProjectilePool.CurrentData != ActiveProjectilePool) {
                    _activeProjectilePool = new RandomPool<int>();

                    var pairedInputs = ActiveProjectilePool.Split('|');

                    foreach (var inputs in pairedInputs) {
                        var pair = inputs.Split(',');

                        int projectileID = int.Parse(pair[0]);
                        double projectileChance = double.Parse(pair[1]);

                        if (projectileID < 0) continue;

                        _activeProjectilePool.AddChance(projectileID, projectileChance);
                    }

                    _activeProjectilePool.CurrentData = ActiveProjectilePool;
                }

                return _activeProjectilePool;
            }
        }

        public override string ToString() {
            return $"ID: {ID}\n" +
                   $"Damage: {Damage}\n" +
                   $"Knockback: {Knockback}\n" +
                   $"UseAnimation: {UseAnimation}\n" +
                   $"UseTime: {UseTime}\n" +
                   $"Shoot: {Shoot}\n" +
                   $"ShootSpeed: {ShootSpeed}\n" +
                   $"VelocityMultiplier: {VelocityMultiplier}\n" +
                   $"AmmoIdentifier: {AmmoIdentifier}\n" +
                   $"UseAmmoIdentifier: {UseAmmoIdentifier}\n" +
                   $"NotAmmo: {IsNotAmmo}\n" +
                   $"Inflict Buff: {Terraria.Lang.GetBuffName(InflictBuffID)} for {InflictBuffDuration / Constants.TicksPerSecond}s\n" +
                   $"Receive Buff: {Terraria.Lang.GetBuffName(ReceiveBuffID)} for {ReceiveBuffDuration / Constants.TicksPerSecond}s\n" +
                   $"HomingRadius: {HomingRadius}\n" +
                   $"AngularVelocity: {AngularVelocity}\n" +
                   $"Mirror: {Mirror}\n" +
                   $"Spread: {Spread}\n" +
                   $"RandomSpread: {IsRandomSpread}\n" +
                   $"NumShots: {NumShots}\n" +
                   $"ProjectilePool: {ProjectilePool}\n" +
                   $"ActiveProjectileAI: {ActiveProjectileAI}\n" +
                   $"ActiveProjectilePool: {ActiveProjectilePool}\n" +
                   $"ActiveRange: {ActiveRange}\n" +
                   $"ActiveFireRate: {ActiveFireRate}\n" +
                   $"ActiveSpread: {ActiveSpread}\n";
        }
    }
}
