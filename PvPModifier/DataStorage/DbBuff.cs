using PvPModifier.Utilities.PvPConstants;
using PvPModifier.Variables;

namespace PvPModifier.DataStorage {
    public class DbBuff : DbObject {
        public override string Section => DbTables.BuffTable;

        public int InflictBuffID { get; set; }
        public int InflictBuffDuration { get; set; }
        public int ReceiveBuffID { get; set; }
        public int ReceiveBuffDuration { get; set; }

        public BuffInfo InflictBuff => new BuffInfo(InflictBuffID, InflictBuffDuration);
        public BuffInfo ReceiveBuff => new BuffInfo(ReceiveBuffID, ReceiveBuffDuration);

        public override string ToString() {
            return $"ID: {ID}\n" +
                   $"Inflict Buff: {Terraria.Lang.GetBuffName(InflictBuffID)} for {InflictBuffDuration / Constants.TicksPerSecond}s\n" +
                   $"Receive Buff: {Terraria.Lang.GetBuffName(ReceiveBuffID)} for {ReceiveBuffDuration / Constants.TicksPerSecond}s";
        }
    }
}
