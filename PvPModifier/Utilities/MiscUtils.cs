using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using PvPModifier.Utilities.PvPConstants;
using Terraria;
using Utils = TShockAPI.Utils;

namespace PvPModifier.Utilities {
    public static class MiscUtils {
        /// <summary>
        /// Attempts to sanitize any ' characters in a string to '' for sql queries.
        /// </summary>
        public static string SanitizeString(this string s) {
            if (!s.Contains("'")) return s;

            string[] temp = s.Split('\'');
            s = temp[0];

            for (int x = 1; x < temp.Length; x++) {
                s += "''" + temp[x];
            }
            return s;
        }

        /// <summary>
        /// Converts a string to be friendly with sql inputs.
        /// </summary>
        public static string SqlString(this string s) => "'" + SanitizeString(s) + "'";

        /// <summary>
        /// Determines whether a bit is a 1 or a 0 in an integer in a specified index, 
        /// where the index starts at 0 from the right.
        /// </summary>
        /// <param name="bitIndex">Index of the bit, starting from 0 on the left</param>
        /// <returns>True if the bit in the specified index is 1</returns>
        public static bool GetBit(this int x, int bitIndex) => (x & (1 << bitIndex)) != 0;

        /// <summary>
        /// Restricts a number between a minimum and maximum value.
        /// </summary>
        public static T Clamp<T>(this T num, T min, T max) where T : IComparable =>
            num.CompareTo(min) < 0 ? min : num.CompareTo(max) > 0 ? max : num;

        /// <summary>
        /// Replaces a value with another.
        /// </summary>
        public static T Replace<T>(this T num, T value, T replace) where T : IComparable =>
            num.CompareTo(value) == 0 ? replace : num;

        /// <summary>
        /// Generates a string with a specified amount of line breaks.
        /// </summary>
        /// <param name="amount">The amount of line breaks.</param>
        public static string LineBreaks(int amount) {
            StringBuilder sb = new StringBuilder();
            for (int x = 0; x < amount; x++) {
                sb.Append("\r\n");
            }
            return sb.ToString();
        }

        /// <summary>
        /// Separates a string into lines after a specified amount of characters.
        /// </summary>
        public static string SeparateToLines(this string s, int charPerLine = 45, string breakSpecifier = "") {
            StringBuilder sb = new StringBuilder();
            int count = 0;

            foreach (char ch in s) {
                if (count != 0 && count >= charPerLine) {
                    if (breakSpecifier != "" && ch.ToString() == breakSpecifier) {
                        sb.Append("\r\n");
                        count = 0;
                    }
                }
                sb.Append(ch);
                count++;
            }

            return sb.ToString();
        }

        /// <summary>
        /// Converts a string into a given Type.
        /// </summary>
        /// <returns>Returns false if the string is incompatible with the given Type</returns>
        public static bool TryConvertStringToType(Type referenceType, string input, out object obj) {
            try {
                TypeConverter typeConverter = TypeDescriptor.GetConverter(referenceType);
                obj = typeConverter.ConvertFromString(input.SanitizeString());
                return true;
            } catch {
                obj = default(object);
                return false;
            }
        }

        /// <summary>
        /// Gets a list of projectiles based off the given Name query.
        /// </summary>
        public static List<int> GetProjectileByName(this Utils util, string name) {
            string nameLower = name.ToLower();
            var found = new List<int>();
            for (int i = 1; i < Main.maxProjectileTypes; i++) {
                string projectileName = Lang.GetProjectileName(i).ToString();
                if (!String.IsNullOrWhiteSpace(projectileName) && projectileName.ToLower() == nameLower)
                    return new List<int> { i };
                if (!String.IsNullOrWhiteSpace(projectileName) && projectileName.ToLower().StartsWith(nameLower))
                    found.Add(i);
            }
            return found;
        }

        /// <summary>
        /// Gets the id of an item, projectile, or buff.
        /// </summary>
        public static List<int> GetIdFromInput(this Utils util, string input, string name) {
            if (input == DbTables.ItemTable) {
                var itemsFound = util.GetItemByName(name);
                return itemsFound.Select(c => c.netID).ToList();
            }

            if (input == DbTables.ProjectileTable) {
                return util.GetProjectileByName(name);
            }

            if (input == DbTables.BuffTable) {
                return util.GetBuffByName(name);
            }

            return default(List<int>);
        }

        /// <summary>
        /// Gets the name from the ID from a given table
        /// </summary>
        /// <param name="input">Input: ItemTable, ProjectileTable, BuffTable</param>
        /// <param name="id">ID of the input</param>
        /// <returns></returns>
        public static string GetNameFromInput(string input, int id) {
            if (input == DbTables.ItemTable) {
                return Lang.GetItemName(id).ToString();
            } else if (input == DbTables.ProjectileTable) {
                return Lang.GetProjectileName(id).ToString();
            } else if (input == DbTables.BuffTable) {
                return Lang.GetBuffName(id);
            }

            return default(string);
        }

        /// <summary>
        /// Converts a string to the reference value type,
        /// and sets the string to the given reference value.
        /// </summary>
        public static bool SetValueWithString<T>(ref T value, string val) {
            try {
                value = (T)Convert.ChangeType(val, value.GetType());
                return true;
            } catch {
                return false;
            }
        }

        /// <summary>
        /// Takes in a name of a variable in a class, finds the variable in an object, and
        /// tries to set a string value to the variable for the object.
        /// </summary>
        public static bool SetValueWithString(object obj, string propertyName, string val) {
            try {
                var property = obj.GetType().GetProperty(propertyName);
                if (property == null) return false;
                property.SetValue(obj, Convert.ChangeType(val, property.GetValue(obj).GetType()));
                return true;
            } catch {
                return false;
            }
        }

        public static List<string> GetVariables(Type type) {
            List<string> attributes = new List<string>();
            var values = type.GetProperties();
            foreach (var value in values) {
                //({value.PropertyType.ToString().Split('.').Last()}) to get the variable type
                attributes.Add($"{value.Name}");
            }

            return attributes;
        }

        public static List<string> GetConstants(Type type) {
            List<string> constants = new List<string>();

            var values = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(fi => fi.IsLiteral && !fi.IsInitOnly).ToList();

            foreach (var value in values) {
                //({value.FieldType.ToString().Split('.').Last()}) to get the variable type
                constants.Add($"{value.Name}");
            }

            return constants;
        }

        public static string[][] SplitIntoPairs(string[] input) {
            string[][] split = new string[input.Length / 2][];

            for (int x = 0; x < input.Length / 2; x++) {
                split[x] = new [] { input[x * 2] , input[x * 2 + 1] };
            }

            return split;
        }
    }
}
