using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PvPModifier.Utilities {
    public class RandomPool<T> {
        public string CurrentData = "";

        private Dictionary<double, T> _itemPool = new Dictionary<double, T>();
        private double _totalPercentage;

        public RandomPool<T> AddChance(T item, double percentage) {
            _totalPercentage += percentage;
            _itemPool[_totalPercentage] = item;

            return this;
        }

        public T GetRandomItem() {
            double randNum = Terraria.Main.rand.NextDouble() * _totalPercentage;

            foreach (KeyValuePair<double, T> pair in _itemPool) {
                if (randNum <= pair.Key) return pair.Value;
            }

            return default;
        }

        public bool IsEmpty() {
            return _itemPool.Count == 0;
        }
    }
}
