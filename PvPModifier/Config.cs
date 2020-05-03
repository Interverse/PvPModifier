using Newtonsoft.Json;
using System.IO;
using TShockAPI;

namespace PvPModifier {
    public class Config {
        public static string ConfigPath = Path.Combine(TShock.SavePath, "pvpmodifier.json");
        public static string LogPath = Path.Combine(TShock.SavePath, "pvplog.txt");

        public bool EnablePlugin;

        public double IframeTime;
        public bool EnableKnockback;
        public bool ForceCustomKnockback;

        public bool EnableHoming;
        
        public bool EnableSpectreMask;

        public bool EnableTurtle;
        public double TurtleMultiplier;
        public bool EnableThorns;
        public double ThornMultiplier;

        public bool EnableNebula;

        public bool EnableFrost;
        public double FrostDuration;

        public bool LoseVortexOnHit;

        public bool EnableBuffs;

        public double KnockbackMultiplier;
        public double KnockupAmount;
        public double KnockbackFalloff;
        public double MaxKnockbackSpeed;
        public uint ComboTime;

        public bool FirstConfigGeneration = true;

        /// <summary>
        /// Writes the current internal server config to the external .json file
        /// </summary>
        /// <param Name="path"></param>
        public void Write(string path) {
            File.WriteAllText(path, JsonConvert.SerializeObject(this, Formatting.Indented));
        }

        /// <summary>
        /// Reads the .json file and stores its contents into the plugin
        /// </summary>
        public static Config Read(string path) {
            if (!File.Exists(path))
                return new Config();
            return JsonConvert.DeserializeObject<Config>(File.ReadAllText(path));
        }

        /// <summary>
        /// Sets all default values in the config and the sql(ite) database.
        /// </summary>
        public bool SetDefaultValues() {
            if (FirstConfigGeneration) {
                EnablePlugin = true;

                IframeTime = 0.0;
                EnableKnockback = false;
                ForceCustomKnockback = false;

                EnableHoming = true;

                EnableSpectreMask = true;

                EnableTurtle = true;
                TurtleMultiplier = 1.0;
                EnableThorns = true;
                ThornMultiplier = 1.0;

                EnableNebula = true;

                EnableFrost = true;
                FrostDuration = 3.0;

                LoseVortexOnHit = false;

                EnableBuffs = true;

                KnockbackMultiplier = 1.0;
                KnockupAmount = 1.0;
                KnockbackFalloff = 0.5;
                MaxKnockbackSpeed = 30.0;
                ComboTime = 500;

                FirstConfigGeneration = false;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Resets all values of the config to its default values.
        /// </summary>
        public void ResetConfigValues() {
            FirstConfigGeneration = true;
            SetDefaultValues();
        }

        /// <summary>
        /// Parses all item, projectile, and buff changes and puts it into a .txt file in the tshock folder.
        /// </summary>
        public void LogChange(string log) {
            StreamWriter sw = new StreamWriter(LogPath, true);
            sw.WriteLine(log);
            sw.Close();
        }
    }
}
