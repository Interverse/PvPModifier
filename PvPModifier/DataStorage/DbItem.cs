using PvPModifier.Utilities.PvPConstants;
using PvPModifier.Variables;

namespace PvPModifier.DataStorage {
    public class DbItem : DbObject {
        public override string Section => DbTables.ItemTable;

        public int Damage { get; set; }
        public float Knockback { get; set; }
        public int UseAnimation { get; set; }
        public int UseTime { get; set; }
        public int Shoot { get; set; }
        public float ShootSpeed { get; set; }
        public int AmmoIdentifier { get; set; }
        public int UseAmmoIdentifier { get; set; }
        public bool NotAmmo { get; set; }
        public BuffInfo InflictBuff { get; set; }
        public BuffInfo ReceiveBuff { get; set; }

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
