using LabExtended.API;

using PlayerStatsSystem;

using UnityEngine;

using Utils;

namespace SecretLabAPI.Utilities
{
    /// <summary>
    /// Provides static methods and utilities for creating and managing explosion-related visual or audio effects.
    /// </summary>
    public static class ExplosionEffects
    {
        /// <summary>
        /// Triggers an explosion effect for the specified player, with options for spawning grenades,
        /// applying visual effects, and causing player death.
        /// </summary>
        /// <param name="player">The player entity to target with the explosion effect.</param>
        /// <param name="amount">The number of explosions to trigger.</param>
        /// <param name="grenadeType">The type of grenade to simulate in the explosion.</param>
        /// <param name="deathReason">The reason assigned for the player's death, if applicable.</param>
        /// <param name="effectOnly">Specifies whether only the visual effect should be triggered
        /// without causing damage to entities.</param>
        /// <param name="killPlayer">Indicates whether the targeted player should be killed by the
        /// explosion.</param>
        /// <param name="velocityMultiplier">A multiplier for the velocity applied to the player's ragdoll
        /// upon explosion.</param>
        /// <returns>Returns true if the explosion was successfully applied, false otherwise.</returns>
        public static bool Explode(this ExPlayer player, int amount, ItemType grenadeType, string? deathReason,
            bool effectOnly = false, bool killPlayer = true, float velocityMultiplier = 1f)
        {
            if (player?.ReferenceHub == null)
                return false;

            if (amount < 1)
                return false;

            if (!player.Role.IsAlive)
                return false;

            deathReason ??= "No death reason provided.";

            if (effectOnly)
            {
                for (var x = 0; x < amount; x++)
                {
                    ExplosionUtils.ServerSpawnEffect(player.Position, grenadeType);
                }
            }
            else
            {
                var explosionType = ExplosionType.Grenade;

                switch (grenadeType)
                {
                    case ItemType.ParticleDisruptor:
                        explosionType = ExplosionType.Disruptor;
                        break;

                    case ItemType.SCP018:
                        explosionType = ExplosionType.SCP018;
                        break;

                    case ItemType.SCP207:
                        explosionType = ExplosionType.Cola;
                        break;

                    case ItemType.SCP330:
                        explosionType = ExplosionType.PinkCandy;
                        break;

                    case ItemType.Jailbird:
                        explosionType = ExplosionType.Jailbird;
                        break;
                }

                for (var x = 0; x < amount; x++)
                {
                    ExplosionUtils.ServerExplode(player.Position, player.Footprint, explosionType);
                }
            }

            if (killPlayer)
            {
                if (player.IsGodModeEnabled) player.IsGodModeEnabled = false;
                
                var velocity = player.Rotation.Rotation * (Vector3.back * velocityMultiplier);

                velocity.y = 1f;
                velocity.Normalize();

                velocity *= (5f + velocityMultiplier);
                velocity.y += 2f;

                var damageHandler = new CustomReasonDamageHandler(deathReason, -1f);

                damageHandler.ApplyDamage(player.ReferenceHub);
                damageHandler.StartVelocity = velocity;

                player.ReferenceHub.playerStats.KillPlayer(damageHandler);
            }

            return true;
        }
    }
}