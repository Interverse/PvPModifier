using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
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
        /// Replaces a value with another.
        /// </summary>
        public static T Replace<T>(this T num, T value, T replace) where T : IComparable =>
            num.CompareTo(value) == 0 ? replace : num;

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
        public static List<int> GetIdFromInput(this Utils util, string section, string name) {
            if (section == DbTables.ItemTable) {
                var itemsFound = util.GetItemByName(name);
                return itemsFound.Select(c => c.netID).ToList();
            }

            if (section == DbTables.ProjectileTable) {
                return util.GetProjectileByName(name);
            }

            if (section == DbTables.BuffTable) {
                return util.GetBuffByName(name);
            }

            return default(List<int>);
        }

        /// <summary>
        /// Gets the name from the ID from a given section
        /// </summary>
        /// <param name="input">Input: ItemTable, ProjectileTable, BuffTable</param>
        /// <param name="id">ID of the section</param>
        public static string GetNameFromInput(string input, int id) {
            if (input == DbTables.ItemTable) {
                return Lang.GetItemName(id).ToString();
            }

            if (input == DbTables.ProjectileTable) {
                return Lang.GetProjectileName(id).ToString();
            }

            if (input == DbTables.BuffTable) {
                return Lang.GetBuffName(id);
            }

            return default(string);
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

        /// <summary>
        /// Gets all the constants of a class and stores it in a list.
        /// </summary>
        /// <param name="class">The class to pull constants from.</param>
        /// <returns>A list of strings containing all the constants in a class.</returns>
        public static List<string> GetConstants(Type @class) {
            List<string> constants = new List<string>();

            var values = @class.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(fi => fi.IsLiteral && !fi.IsInitOnly).ToList();

            foreach (var value in values) {
                //({value.FieldType.ToString().Split('.').Last()}) to get the variable class
                constants.Add($"{value.Name}");
            }

            return constants;
        }

        /// <summary>
        /// Pairs up an array of inputs into a two dimensional array.
        /// First array is the number of paired inputs.
        /// Second array contains the paired inputs, with length 2.
        /// </summary>
        /// <param name="input">An array of objects to be paired up with</param>
        public static T[][] SplitIntoPairs<T>(T[] input) {
            T[][] split = new T[input.Length / 2][];

            for (int x = 0; x < input.Length / 2; x++) {
                split[x] = new [] { input[x * 2] , input[x * 2 + 1] };
            }

            return split;
        }

        /// <summary>
        /// Returns a vector that is turned towards a target.
        /// </summary>
        public static Vector2 TurnTowards(Vector2 vel, Vector2 pos, Vector2 target, double angularVelocity) {
            float speed = vel.Length();
            Vector2 direction = target - pos;
            direction.Normalize();
            vel.Normalize();
                
            //Cross product (if both vectors are converted to Vector3 where Z = 0), or determinant of 2 vectors
            double rotateAmount = vel.X * direction.Y - vel.Y * direction.X;
            rotateAmount *= angularVelocity;
            
            return Rotate(vel * speed, (float)rotateAmount);
        }

        /// <summary>
        /// Rotates a vector.
        /// </summary>
        public static Vector2 Rotate(Vector2 v, float degrees) {
            double radians = degrees * Math.PI / 180f;
            //Formulas for sin and cos are from the Taylor Polynomial series
            double sin = radians - radians * radians * radians / 6;
            double cos = 1 - radians * radians / 2;

            float tx = v.X;
            float ty = v.Y;

            v.X = (float)(cos * tx - sin * ty);
            v.Y = (float)(sin * tx + cos * ty);
            return v;
        }
    }
}
