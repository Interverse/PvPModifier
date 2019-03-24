using PvPModifier.Variables;

namespace PvPModifier.DataStorage {
    public class DbItem {
        public int ID;
        public int Damage;
        public float Knockback;
        public int UseAnimation;
        public int UseTime;
        public int Shoot;
        public float ShootSpeed;
        public int AmmoIdentifier;
        public int UseAmmoIdentifier;
        public bool NotAmmo;
        public BuffInfo InflictBuff;
        public BuffInfo ReceiveBuff;

        public override string ToString() {
            return $"ID: {ID}\n" +
                   $"Damage: {Damage}\n" +
                   $"Knockback: {Knockback}\n" +
                   $"UseAnimation: {UseAnimation}\n" +
                   $"UseTime: {UseTime}\n" +
                   $"Shoot: {Shoot}\n" +
                   $"ShootSpeed: {ShootSpeed}\n" +
                   $"AmmoIdentifier: {AmmoIdentifier}\n" +
                   $"UseAmmoIdentifier: {UseAmmoIdentifier}\n" +
                   $"NotAmmo: {NotAmmo}\n" +
                   $"Inflict Buff: {Terraria.Lang.GetBuffName(InflictBuff.BuffId)} for {InflictBuff.BuffDuration / 60.0}s\n" +
                   $"Receive Buff: {Terraria.Lang.GetBuffName(ReceiveBuff.BuffId)} for {ReceiveBuff.BuffDuration / 60.0}s";
        }
    }
}
