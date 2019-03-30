using PvPModifier.DataStorage;
using PvPModifier.Utilities;
using PvPModifier.Utilities.PvPConstants;
using Terraria;

namespace PvPModifier.Variables {
    /// <summary>
    /// The class used to store projectile data. Includes additional methods and variables to
    /// perform pvp based calculations and actions.
    /// </summary>
    public class PvPProjectile : Projectile {

        public PvPItem ItemOriginated;
        public PvPPlayer OwnerProjectile;

        public Projectile MainProjectile;

        public PvPProjectile(int type) {
            SetDefaults(type);
            this.identity = -1;
        }

        public PvPProjectile(int type, int identity) {
            SetDefaults(type);
            this.identity = identity;
            MainProjectile = Main.projectile[identity];
        }

        public PvPProjectile(int type, int index, int ownerIndex, PvPItem item) {
            SetDefaults(type);
            identity = index;
            ItemOriginated = item;
            owner = ownerIndex;
            OwnerProjectile = PvPModifier.PvPers[ownerIndex];
        }

        /// <summary>
        /// Performs additional actions for projectiles.
        /// </summary>
        public void PerformProjectileAction() {
            if (CheckNull() || !OwnerProjectile.TPlayer.hostile) return;
            switch (type) {
                //Medusa Ray projectile
                case 536:
                    var target = PvPUtils.FindClosestPlayer(OwnerProjectile.TPlayer.position, OwnerProjectile.Index,
                        Constants.MedusaHeadRange);

                    if (target != null) {
                        if (Collision.CanHit(OwnerProjectile.TPlayer.position, OwnerProjectile.TPlayer.width, OwnerProjectile.TPlayer.height,
                            target.TPlayer.position, target.TPlayer.width, target.TPlayer.height)) {
                            if (target.CheckMedusa()) {
                                string deathmessage = target.Name + " was petrified by " + target.Name + "'s Medusa Head.";
                                target.DamagePlayer(PvPUtils.GetPvPDeathMessage(deathmessage, ItemOriginated),
                                    ItemOriginated, ItemOriginated.ConfigDamage, 0, false);
                                target.SetBuff(Cache.Projectiles[535].InflictBuff);
                            }
                        }
                    }
                    break;
            }
        }

        public bool CheckNull() {
            return OwnerProjectile == null || ItemOriginated == null;
        }
    }
}
