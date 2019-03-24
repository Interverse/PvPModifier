using PvPModifier.Variables;

namespace PvPModifier.DataStorage {
    public class DbProjectile {
        public int ID;
        public int Shoot;
        public float VelocityMultiplier;
        public int Damage;
        public BuffInfo InflictBuff;
        public BuffInfo ReceiveBuff;

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
