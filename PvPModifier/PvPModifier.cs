using System;
using System.IO;
using System.Reflection;
using PvPModifier.DataStorage;
using PvPModifier.Network;
using PvPModifier.Utilities.Extensions;
using PvPModifier.Utilities.PvPConstants;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace PvPModifier {
    [ApiVersion(2, 1)]
    public class PvPModifier : TerrariaPlugin {

        public static Config Config;
        private PvPEvents _pvpevents;

        public override string Name => "PvP Modifier";
        public override string Author => "Johuan";
        public override string Description => "Adds customizability to pvp";
        public override Version Version => Assembly.GetExecutingAssembly().GetName().Version;

        // Reads command line arguments to write custom database table names
        public PvPModifier(Main game) : base(game) {
            var commandLineArgs = Environment.GetCommandLineArgs();
            for (int args = 1; args < commandLineArgs.Length; args++) {
                string arg = commandLineArgs[args];
                switch (arg) {
                    case "-pvpitemtable":
                        DbTables.ItemTable = commandLineArgs[++args];
                        break;

                    case "-pvpprojtable":
                        DbTables.ProjectileTable = commandLineArgs[++args];
                        break;

                    case "-pvpbufftable":
                        DbTables.BuffTable = commandLineArgs[++args];
                        break;
                }
            }
        }

        public override void Initialize() {
            // Initializes the config, making one if it doesn't exist
            Config = Config.Read(Config.ConfigPath);
            if (!File.Exists(Config.ConfigPath)) {
                Config.Write(Config.ConfigPath);
            }

            // Connects the SQL database to the plugin
            Database.ConnectDB();

            // Registers hooks to initialize plugin features
            ServerApi.Hooks.GamePostInitialize.Register(this, OnGamePostInitialize);
            ServerApi.Hooks.NetGetData.Register(this, GetData);
            ServerApi.Hooks.ServerJoin.Register(this, OnJoin);

            // Registers an method to be called when /reload is used
            GeneralHooks.ReloadEvent += OnReload;

            // Registers all the hooks used to handle pvp
            _pvpevents = new PvPEvents(this);

            // Adds commands to be used in this plugin
            PluginCommands.RegisterCommands();
        }

        /// <summary>
        /// Deregisters all the hooks the plugin uses.
        /// </summary>
        protected override void Dispose(bool disposing) {
            if (disposing) {
                ServerApi.Hooks.GamePostInitialize.Deregister(this, OnGamePostInitialize);
                ServerApi.Hooks.NetGetData.Deregister(this, GetData);
                ServerApi.Hooks.ServerJoin.Deregister(this, OnJoin);

                GeneralHooks.ReloadEvent -= OnReload;

                _pvpevents.Unsubscribe();

                Config.Write(Config.ConfigPath);
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// On /reload, config is read from the .json file and <see cref="Cache"/> object is overwritten with
        /// whatever is currently in the external SQL database.
        /// </summary>
        /// <param name="e"></param>
        private void OnReload(ReloadEventArgs e) {
            Config = Config.Read(Config.ConfigPath);
            e.Player.SendSuccessMessage("PvPModifier reloaded.");
        }

        /// <summary>
        /// Initializes extra variables for each <see cref="TSPlayer"/> who enters the server.
        /// </summary>
        private void OnJoin(JoinEventArgs args) {
            TShock.Players[args.Who].Initialize();
        }

        /// <summary>
        /// Sets default config values if a config doesn't exist after the server has loaded the game.
        /// Also loads the database and writes it in <see cref="Cache"/>.
        /// </summary>
        private void OnGamePostInitialize(EventArgs args) {
            if (Config.SetDefaultValues()) {
                Database.InitDefaultTables();
            }
            Config.Write(Config.ConfigPath);
        }

        /// <summary>
        /// Creates objects to be processes in <see cref="Network.PvPEvents"/>.
        /// </summary>
        /// <param name="args">The data to be processed.</param> 
        private void GetData(GetDataEventArgs args) {
            MemoryStream data = new MemoryStream(args.Msg.readBuffer, args.Index, args.Length);
            TSPlayer attacker = TShock.Players[args.Msg.whoAmI];

            if (attacker == null || !attacker.TPlayer.active || !attacker.ConnectionAlive) return;

            DataHandler.HandleData(args, data, attacker);
        }
    }
}
