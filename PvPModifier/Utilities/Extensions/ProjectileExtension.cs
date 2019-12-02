using PvPModifier.DataStorage;
using PvPModifier.Utilities;
using PvPModifier.Utilities.PvPConstants;
using System.Collections.Generic;
using Terraria;
using TShockAPI;

namespace PvPModifier.Utilities.Extensions {
    /// <summary>
    /// The class used to store projectile data. Includes additional methods and variables to
    /// perform pvp based calculations and actions.
    /// </summary>
    public static class ProjectileExtension {
        public static int MaxAI = 10;

        public enum AI {
            ItemOriginated = 2,
            Cooldown = 3
        }

        public static void InitializeExtraAISlots(this Projectile proj) {
            Queue<float> aiCollection = new Queue<float>();

            foreach (float ai in proj.ai) {
                aiCollection.Enqueue(ai);
            }

            proj.ai = new float[MaxAI];

            int aiIndex = 0;
            while (aiCollection.Count > 0) {
                proj.ai[aiIndex++] = aiCollection.Dequeue();
            }
        }

        public static bool CooldownFinished(this Projectile proj) {
            return proj.ai[(int)AI.Cooldown] <= 0;
        }

        public static bool HasInitializedExtraAISlots(this Projectile proj) {
            return proj.ai.Length == MaxAI;
        }

        public static void SetItemOriginated(this Projectile proj, int itemType) {
            if (!HasInitializedExtraAISlots(proj)) {
                proj.InitializeExtraAISlots();
            }
            proj.ai[(int)AI.ItemOriginated] = itemType;
        }

        public static Projectile SetCooldown(this Projectile proj, int cooldown) {
            if (!HasInitializedExtraAISlots(proj)) {
                proj.InitializeExtraAISlots();
            }
            proj.ai[(int)AI.Cooldown] = cooldown;
            return proj;
        }

        public static void DecreaseCooldown(this Projectile proj) {
            if (!HasInitializedExtraAISlots(proj)) {
                proj.InitializeExtraAISlots();
            }
            if (proj.ai[(int)AI.Cooldown] > 0) {
                proj.ai[(int)AI.Cooldown]--;
            }
        }

        public static Projectile SetIdentity(this Projectile proj, int identity) {
            proj.identity = identity;
            return proj;
        }

        public static Projectile SetType(this Projectile proj, int type) {
            proj.SetDefaults(type);
            return proj;
        }

        public static TSPlayer GetOwner(this Projectile proj) {
            if (proj.owner >= TShock.Players.Length) return null;
            return TShock.Players[proj.owner];
        }

        public static Item GetItemOriginated(this Projectile proj) {
            return proj.GetOwner()?.FindPlayerItem((int)proj.ai[(int)AI.ItemOriginated]);
        }

        public static Projectile RotateVelocity(this Projectile proj, float degrees) {
            proj.velocity = proj.velocity.Rotate(degrees);
            return proj;
        }

        /// <summary>
        /// Performs additional actions for projectiles.
        /// </summary>
        public static void PerformProjectileAction(this Projectile proj) {
            var owner = proj.GetOwner();
            var ItemOriginated = proj.GetItemOriginated();

            if (!owner.TPlayer.hostile) return;
            switch (proj.type) {
                //Medusa Ray projectile
                case 536:
                    var target = PvPUtils.FindClosestPlayer(owner.TPlayer.position, owner.Index, Constants.MedusaHeadRange, owner.TPlayer.team);

                    if (target != null) {
                        if (Collision.CanHit(owner.TPlayer.position, owner.TPlayer.width, owner.TPlayer.height,
                            target.TPlayer.position, target.TPlayer.width, target.TPlayer.height)) {
                            if (target.CheckMedusa()) {
                                string deathmessage = target.Name + " was petrified by " + target.Name + "'s Medusa Head.";
                                target.DamagePlayer(owner, PvPUtils.GetPvPDeathMessage(deathmessage, ItemOriginated),
                                    ItemOriginated, ItemOriginated.GetConfigDamage(), 0, false);
                                target.SetBuff(Cache.GetProjectile(535).InflictBuff);
                            }
                        }
                    }
                    break;
            }
        }
    }

    public enum ActiveAIType {
        None = 0,
        Minion = 1,
        RandomScatter = 2,
        V_Split = 3
    }
}
