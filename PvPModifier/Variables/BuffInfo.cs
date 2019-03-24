namespace PvPModifier.Variables {
    public class BuffInfo {
        public int BuffId { get; set; }
        public int BuffDuration { get; set; }

        public BuffInfo(int buffId, int buffDuration) {
            BuffId = buffId;
            BuffDuration = buffDuration;
        }

        public BuffInfo() {
            BuffId = 0;
            BuffDuration = 0;
        }

        public override string ToString() {
            return $"ID: {BuffId}, Duration: {BuffDuration}";
        }
    }
}