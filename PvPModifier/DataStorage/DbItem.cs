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
        public int NotAmmo { get; set; }
        public int InflictBuffID { get; set; }
        public int InflictBuffDuration { get; set; }
        public int ReceiveBuffID { get; set; }
        public int ReceiveBuffDuration { get; set; }

        public BuffInfo InflictBuff => new BuffInfo(InflictBuffID, InflictBuffDuration);
        public BuffInfo ReceiveBuff => new BuffInfo(ReceiveBuffID, ReceiveBuffDuration);
        public bool IsNotAmmo => NotAmmo == 1;

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
                   $"NotAmmo: {IsNotAmmo}\n" +
                   $"Inflict Buff: {Terraria.Lang.GetBuffName(InflictBuffID)} for {InflictBuffDuration / Constants.TicksPerSecond}s\n" +
                   $"Receive Buff: {Terraria.Lang.GetBuffName(ReceiveBuffID)} for {ReceiveBuffDuration / Constants.TicksPerSecond}s";
        }
    }
}
