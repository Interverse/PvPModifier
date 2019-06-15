using System;
using System.Timers;
using PvPModifier.CustomWeaponAPI;
using PvPModifier.DataStorage;
using PvPModifier.Network.Events;
using PvPModifier.Network.Packets;
using PvPModifier.Utilities;
using PvPModifier.Utilities.PvPConstants;
using PvPModifier.Variables;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace PvPModifier.Network {
    public class PvPEvents {
        private TerrariaPlugin _plugin;

        public PvPEvents(TerrariaPlugin plugin) {
            DataHandler.PlayerHurt += PlayerEvents.OnPlayerHurt;
            DataHandler.ProjectileNew += ProjectileEvents.OnNewProjectile;
            DataHandler.PvPToggled += PlayerEvents.OnPvPToggledAsync;
            DataHandler.PlayerUpdate += PlayerEvents.OnPlayerUpdateAsync;
            DataHandler.SlotUpdate += ItemEvents.CheckIncomingItems;
            DataHandler.SlotUpdate += ItemEvents.CheckDrops;

            _plugin = plugin;

            ServerApi.Hooks.ProjectileAIUpdate.Register(_plugin, ProjectileEvents.UpdateProjectileHoming);
            ServerApi.Hooks.ProjectileAIUpdate.Register(_plugin, ProjectileEvents.UpdateActiveProjectileAI);
            ServerApi.Hooks.GameUpdate.Register(_plugin, ProjectileEvents.CleanupInactiveProjectiles);
        }

        public void Unsubscribe() {
            DataHandler.PlayerHurt -= PlayerEvents.OnPlayerHurt;
            DataHandler.ProjectileNew -= ProjectileEvents.OnNewProjectile;
            DataHandler.PvPToggled -= PlayerEvents.OnPvPToggledAsync;
            DataHandler.PlayerUpdate -= PlayerEvents.OnPlayerUpdateAsync;
            DataHandler.SlotUpdate -= ItemEvents.CheckIncomingItems;
            DataHandler.SlotUpdate -= ItemEvents.CheckDrops;

            ServerApi.Hooks.ProjectileAIUpdate.Deregister(_plugin, ProjectileEvents.UpdateProjectileHoming);
            ServerApi.Hooks.ProjectileAIUpdate.Deregister(_plugin, ProjectileEvents.UpdateActiveProjectileAI);
            ServerApi.Hooks.GameUpdate.Deregister(_plugin, ProjectileEvents.CleanupInactiveProjectiles);
        }
    }
}
