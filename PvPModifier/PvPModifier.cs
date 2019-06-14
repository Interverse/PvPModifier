using System;
using System.IO;
using System.Linq;
using System.Reflection;
using PvPModifier.DataStorage;
using PvPModifier.Network;
using PvPModifier.Variables;
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

        public PvPModifier(Main game) : base(game) { }

        public override void Initialize() {
            Config = Config.Read(Config.ConfigPath);
            if (!File.Exists(Config.ConfigPath)) {
                Config.Write(Config.ConfigPath);
            }

            Database.ConnectDB();

            ServerApi.Hooks.GamePostInitialize.Register(this, OnGamePostInitialize);
            ServerApi.Hooks.NetGetData.Register(this, GetData);
            ServerApi.Hooks.ServerJoin.Register(this, OnJoin);

            GeneralHooks.ReloadEvent += OnReload;

            PlayerHooks.PlayerPostLogin += OnPlayerPostLogin;

            _pvpevents = new PvPEvents(this);
            PluginCommands.RegisterCommands();
        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                ServerApi.Hooks.GamePostInitialize.Deregister(this, OnGamePostInitialize);
                ServerApi.Hooks.NetGetData.Deregister(this, GetData);
                ServerApi.Hooks.ServerJoin.Deregister(this, OnJoin);

                GeneralHooks.ReloadEvent -= OnReload;

                PlayerHooks.PlayerPostLogin -= OnPlayerPostLogin;

                _pvpevents.Unsubscribe();

                Config.Write(Config.ConfigPath);
            }
            base.Dispose(disposing);
        }

        private void OnReload(ReloadEventArgs e) {
            Config = Config.Read(Config.ConfigPath);
            Database.LoadDatabase();
            e.Player.SendSuccessMessage("PvPModifier reloaded.");
        }

        /// <summary>
        /// Adds a player who just logged in to the plugin-stored collection of players.
        /// </summary>
        private void OnPlayerPostLogin(PlayerPostLoginEventArgs e) {
            TShock.Players[e.Player.Index].Initialize();
        }

        /// <summary>
        /// Adds the player to the plugin-stored collection of players.
        /// </summary>
        private void OnJoin(JoinEventArgs args) {
            TShock.Players[args.Who].Initialize();
        }

        /// <summary>
        /// Sets default config values if a config doesn't exist 
        /// after the server has loaded the game.
        /// Also loads the database.
        /// </summary>
        private void OnGamePostInitialize(EventArgs args) {
            if (Config.SetDefaultValues()) {
                Database.InitDefaultTables();
            }
            Config.Write(Config.ConfigPath);
            Database.LoadDatabase();
        }

        /// <summary>
        /// Processes data so it can be used in <see cref="Network.PvPEvents"/>.
        /// </summary>
        /// <param name="args">The data needed to be processed.</param> 
        private void GetData(GetDataEventArgs args) {
            MemoryStream data = new MemoryStream(args.Msg.readBuffer, args.Index, args.Length);
            TSPlayer attacker = TShock.Players[args.Msg.whoAmI];

            if (attacker == null || !attacker.TPlayer.active || !attacker.ConnectionAlive) return;

            DataHandler.HandleData(args, data, attacker);
        }
    }
}
