using PvPModifier.Utilities.PvPConstants;
using PvPModifier.Variables;

namespace PvPModifier.DataStorage {
    public class DbProjectile : DbObject {
        public override string Section => DbTables.ProjectileTable;

        public int Shoot { get; set; }
        public float VelocityMultiplier { get; set; }
        public int Damage { get; set; }
        public float HomingRadius { get; set; }
        public float AngularVelocity { get; set; }
        public int InflictBuffID { get; set; }
        public int InflictBuffDuration { get; set; }
        public int ReceiveBuffID { get; set; }
        public int ReceiveBuffDuration { get; set; }

        public BuffInfo InflictBuff => new BuffInfo(InflictBuffID, InflictBuffDuration);
        public BuffInfo ReceiveBuff => new BuffInfo(ReceiveBuffID, ReceiveBuffDuration);

        public override string ToString() {
            return $"ID: {ID}\n" +
                   $"Shoot: {Shoot}\n" +
                   $"Damage: {Damage}\n" +
                   $"VelocityMultiplier: {VelocityMultiplier}\n" +
                   $"HomingRadius: {HomingRadius}\n" +
                   $"AngularVelocity: {AngularVelocity}\n" +
                   $"Inflict Buff: {Terraria.Lang.GetBuffName(InflictBuffID)} for {InflictBuffDuration / Constants.TicksPerSecond}s\n" +
                   $"Receive Buff: {Terraria.Lang.GetBuffName(ReceiveBuffID)} for {ReceiveBuffDuration / Constants.TicksPerSecond}s";
        }
    }
}
