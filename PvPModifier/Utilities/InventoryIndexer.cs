namespace PvPModifier.Utilities {
    /// <summary>
    /// Helper class that cycles through the indexes of a <see cref="Terraria.Player"/>'s inventory.
    /// Terraria has a special pickup order, which goes from 0 - 9, 54 - 57, 49 - 10.
    /// This class goes through the same order as a Terrarian's pickup order.
    /// </summary>
    public class InventoryIndexer {
        public readonly int MaxInventoryCycle = 54;

        public int Cycles;

        private int _maxIndexPos;
        private int _maxIndex;
        private int _index;
        bool _isAscending = true;

        public InventoryIndexer() {
            _index = -1;
        }

        /// <summary>
        /// Returns the next index of the indexer.
        /// </summary>
        public int NextIndex() {
            if (_index < 10 || _index >= 53) _index++;
            else _index--;

            if (_index == 10 && _isAscending) {
                _index = 54;
                _isAscending = false;
            } else if (_index == 9 && !_isAscending) {
                Cycles++;
                _index = 0;
                _isAscending = true;
            }
            if (_index == 58) _index = 49;

            return _index;
        }

        /// <summary>
        /// Stores the max index of an inventory based off the pickup order.
        /// </summary>
        public void StoreMaxIndex(int index) {
            int indexPos = GetPosOfIndex(index);

            if (indexPos > _maxIndexPos) {
                _maxIndex = index;
                _maxIndexPos = indexPos;
            }
        }

        /// <summary>
        /// Gets the real position of an index based off the pickup order.
        /// </summary>
        /// <param name="index">The index of a player's inventory</param>
        public int GetPosOfIndex(int index) {
            int indexPos = index;
            if (indexPos >= 54) indexPos = indexPos - 44;
            else if (indexPos >= 10 && indexPos <= 49) indexPos = 63 - indexPos;

            return indexPos;
        }

        public int MaxIndexPos => GetPosOfIndex(_maxIndex);
        public int MaxIndex => _maxIndex;
    }
}
