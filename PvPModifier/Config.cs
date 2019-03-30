using Newtonsoft.Json;
using System.IO;
using TShockAPI;

namespace PvPModifier {
    public class Config {
        public static string ConfigPath = Path.Combine(TShock.SavePath, "pvpmodifier.json");
        public static string LogPath = Path.Combine(TShock.SavePath, "pvplog.txt");

        public bool EnablePlugin { get; set; }

        public double IframeTime { get; set; }
        public bool EnableKnockback { get; set; }

        public bool EnableHoming { get; set; }
        
        public bool EnableSpectreMask { get; set; }

        public bool EnableTurtle { get; set; }
        public double TurtleMultiplier { get; set; }
        public bool EnableThorns { get; set; }
        public double ThornMultiplier { get; set; }

        public bool EnableNebula { get; set; }

        public bool EnableFrost { get; set; }
        public double FrostDuration { get; set; }
        
        public bool EnableBuffs { get; set; }

        public bool FirstConfigGeneration { get; set; } = true;

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

                EnableHoming = true;

                EnableSpectreMask = true;

                EnableTurtle = true;
                TurtleMultiplier = 1.0;
                EnableThorns = true;
                ThornMultiplier = 1.0;

                EnableNebula = true;

                EnableFrost = true;
                FrostDuration = 3.0;

                EnableBuffs = true;

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
