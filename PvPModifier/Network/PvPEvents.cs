using System;
using System.Timers;
using CustomWeaponAPI;
using PvPModifier.Network.Events;
using TerrariaApi.Server;

namespace PvPModifier.Network {
    /// <summary>
    /// Contains all the methods to register/deregister pvp events from hooks.
    /// </summary>
    public class PvPEvents {
        private TerrariaPlugin _plugin;

        public PvPEvents(TerrariaPlugin plugin) {
            DataHandler.PlayerHurt += PlayerEvents.OnPlayerHurt;
            DataHandler.ProjectileNew += ProjectileEvents.OnNewProjectile;
            DataHandler.PvPToggled += PlayerEvents.OnPvPToggled;
            DataHandler.PlayerUpdate += PlayerEvents.OnPlayerUpdateAsync;
            DataHandler.SlotUpdate += ItemEvents.CheckIncomingItems;
            DataHandler.SlotUpdate += ItemEvents.CheckDrops;
            DataHandler.ItemOwner += NetworkEvents.ReceivePing;
            DataHandler.PlayerDeath += SEconomyEvents.OnPlayerDeath;

            _plugin = plugin;

            ServerApi.Hooks.ProjectileAIUpdate.Register(_plugin, ProjectileEvents.UpdateProjectileHoming);
            ServerApi.Hooks.ProjectileAIUpdate.Register(_plugin, ProjectileEvents.UpdateActiveProjectileAI);
            ServerApi.Hooks.GameUpdate.Register(_plugin, ProjectileEvents.CleanupInactiveProjectiles);
        }

        public void Unsubscribe() {
            DataHandler.PlayerHurt -= PlayerEvents.OnPlayerHurt;
            DataHandler.ProjectileNew -= ProjectileEvents.OnNewProjectile;
            DataHandler.PvPToggled -= PlayerEvents.OnPvPToggled;
            DataHandler.PlayerUpdate -= PlayerEvents.OnPlayerUpdateAsync;
            DataHandler.SlotUpdate -= ItemEvents.CheckIncomingItems;
            DataHandler.SlotUpdate -= ItemEvents.CheckDrops;
            DataHandler.ItemOwner -= NetworkEvents.ReceivePing;
            DataHandler.PlayerDeath -= SEconomyEvents.OnPlayerDeath;

            ServerApi.Hooks.ProjectileAIUpdate.Deregister(_plugin, ProjectileEvents.UpdateProjectileHoming);
            ServerApi.Hooks.ProjectileAIUpdate.Deregister(_plugin, ProjectileEvents.UpdateActiveProjectileAI);
            ServerApi.Hooks.GameUpdate.Deregister(_plugin, ProjectileEvents.CleanupInactiveProjectiles);
        }
    }
}
