using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PvPModifier.Utilities;

namespace PvPModifier.DataStorage {
    public abstract class DbObject {
        public abstract string Section { get; }
        public int ID;

        public bool TrySetValue(string param, string value) {
            if (MiscUtils.SetValueWithString(this, param, value)) {
                Database.Update(Section, ID, param, value);
                return true;
            }
            
            return false;
        }
    }
}
