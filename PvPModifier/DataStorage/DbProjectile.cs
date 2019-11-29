using PvPModifier.Utilities;
using PvPModifier.Utilities.PvPConstants;

namespace PvPModifier.DataStorage {
    public class DbProjectile : DbObject {
        public override string Section => DbTables.ProjectileTable;

        public int Shoot;
        public int Damage;
        public int InflictBuffID;
        public int InflictBuffDuration;
        public int ReceiveBuffID;
        public int ReceiveBuffDuration;

        public BuffInfo InflictBuff => new BuffInfo(InflictBuffID, InflictBuffDuration);
        public BuffInfo ReceiveBuff => new BuffInfo(ReceiveBuffID, ReceiveBuffDuration);

        public override string ToString() {
            return $"ID: {ID}\n" +
                   $"Shoot: {Shoot}\n" +
                   $"Damage: {Damage}\n" +
                   $"Inflict Buff: {Terraria.Lang.GetBuffName(InflictBuffID)} for {InflictBuffDuration / Constants.TicksPerSecond}s\n" +
                   $"Receive Buff: {Terraria.Lang.GetBuffName(ReceiveBuffID)} for {ReceiveBuffDuration / Constants.TicksPerSecond}s";
        }
    }
}
