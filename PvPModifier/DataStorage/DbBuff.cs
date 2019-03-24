using PvPModifier.Variables;

namespace PvPModifier.DataStorage {
    public class DbBuff {
        public int ID;
        public BuffInfo InflictBuff;
        public BuffInfo ReceiveBuff;

        public override string ToString() {
            return $"ID: {ID}\n" +
                   $"Inflict Buff: {Terraria.Lang.GetBuffName(InflictBuff.BuffId)} for {InflictBuff.BuffDuration / 60.0}s\n" +
                   $"Receive Buff: {Terraria.Lang.GetBuffName(ReceiveBuff.BuffId)} for {ReceiveBuff.BuffDuration / 60.0}s";
        }
    }
}
