using System.Linq;
using Microsoft.Xna.Framework;
using PvPModifier.DataStorage;
using PvPModifier.Utilities;
using Terraria;

namespace PvPModifier.Variables {
    /// <summary>
    /// The class used to store projectile data. Includes additional methods and variables to
    /// perform pvp based calculations and actions.
    /// </summary>
    public class PvPProjectile : Projectile {
        
        public PvPItem ItemOriginated;
        public PvPPlayer OwnerProjectile;

        public PvPProjectile(int type) {
            SetDefaults(type);
            this.identity = -1;
        }

        public PvPProjectile(int type, int identity) {
            SetDefaults(type);
            this.identity = identity;
        }

        /// <summary>
        /// Performs additional actions for projectiles.
        /// </summary>
        public void PerformProjectileAction() {
            if (!OwnerProjectile.TPlayer.hostile) return;
            switch (type) {
                //Medusa Ray projectile
                case 536:
                    foreach (PvPPlayer pvper in PvPModifier.PvPers.Where(c => c != null && c.TPlayer.hostile && !c.TPlayer.dead)) {
                        if (OwnerProjectile == pvper) continue;
                        if (Vector2.Distance(OwnerProjectile.TPlayer.position, pvper.TPlayer.position) <= 400) {
                            if (pvper.CheckMedusa()) {
                                string deathmessage = pvper.Name + " was petrified by " + pvper.Name + "'s Medusa Head.";
                                pvper.DamagePlayer(PvPUtils.GetPvPDeathMessage(deathmessage, ItemOriginated), 
                                    ItemOriginated, ItemOriginated.ConfigDamage, 0, false);
                                pvper.SetBuff(Cache.Projectiles[535].InflictBuff);
                            }
                        }
                    }
                    break;
            }
        }
    }
}
