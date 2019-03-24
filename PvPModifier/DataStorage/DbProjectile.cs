using PvPModifier.Utilities.PvPConstants;
using PvPModifier.Variables;

namespace PvPModifier.DataStorage {
    public class DbProjectile : DbObject {
        public override string Section => DbTables.ProjectileTable;

        public int Shoot { get; set; }
        public float VelocityMultiplier { get; set; }
        public int Damage { get; set; }
        public BuffInfo InflictBuff { get; set; }
        public BuffInfo ReceiveBuff { get; set; }

        public override string ToString() {
            return $"ID: {ID}\n" +
                   $"Shoot: {Shoot}\n" +
                   $"Damage: {Damage}\n" +
                   $"VelocityMultiplier: {VelocityMultiplier}\n" +
                   $"Inflict Buff: {Terraria.Lang.GetBuffName(InflictBuff.BuffId)} for {InflictBuff.BuffDuration / 60.0}s\n" +
                   $"Receive Buff: {Terraria.Lang.GetBuffName(ReceiveBuff.BuffId)} for {ReceiveBuff.BuffDuration / 60.0}s";
        }
    }
}
