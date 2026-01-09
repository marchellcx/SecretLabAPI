using LabExtended.API;
using LabExtended.Extensions;

using MapGeneration;

using PlayerRoles;
using PlayerStatsSystem;

using UnityEngine;

using Utils;

namespace SecretLabAPI.Extensions
{
    /// <summary>
    /// Provides extension methods for the ExPlayer type.
    /// </summary>
    public static class PlayerExtensions
    {
        /// <summary>
        /// Provides an array containing all values defined in the <see cref="Team"/> enumeration.
        /// </summary>
        /// <remarks>The array is ordered according to the underlying values of the <see cref="Team"/>
        /// enum. This member is useful for iterating over all possible teams or performing bulk operations.</remarks>
        public static Team[] AllTeams = EnumUtils<Team>.Values.ToArray();

        /// <summary>
        /// Provides an array containing all defined facility zones except for None and Other.
        /// </summary>
        /// <remarks>This array can be used to iterate over all standard facility zones, excluding special
        /// values such as None and Other that may represent undefined or miscellaneous cases.</remarks>
        public static FacilityZone[] AllZones = EnumUtils<FacilityZone>.Values.Where(z => z != FacilityZone.None && z != FacilityZone.Other).ToArray();

        /// <summary>
        /// Determines whether the specified player is currently located in a room with the given name, and optionally
        /// matches the specified zone and shape.
        /// </summary>
        /// <param name="player">The player to check for room membership. Must not be null.</param>
        /// <param name="name">The name of the room to check against the player's current location.</param>
        /// <param name="zone">An optional facility zone to further restrict the room match. If specified, the player's current room must
        /// be in this zone.</param>
        /// <param name="shape">An optional room shape to further restrict the room match. If specified, the player's current room must have
        /// this shape.</param>
        /// <returns>true if the player is alive and currently in a room with the specified name, and optionally matches the
        /// specified zone and shape; otherwise, false.</returns>
        public static bool IsInRoom(this ExPlayer player, RoomName name, FacilityZone? zone = null, RoomShape? shape = null)
            => player?.ReferenceHub != null
            && player.Role.IsAlive
            && player.Position.Room != null
            && player.Position.Room.Name == name
            && (!zone.HasValue || player.Position.Room.Zone == zone.Value)
            && (!shape.HasValue || player.Position.Room.Shape == shape.Value);

        /// <summary>
        /// Determines whether the specified position is located within a room that matches the given name, and
        /// optionally, the specified zone and shape.
        /// </summary>
        /// <param name="position">The world position to evaluate for room membership.</param>
        /// <param name="name">The name of the room to check against the position.</param>
        /// <param name="zone">An optional facility zone to further restrict the room match. If specified, the room must be in this zone.</param>
        /// <param name="shape">An optional room shape to further restrict the room match. If specified, the room must have this shape.</param>
        /// <returns>true if the position is within a room that matches the specified name and, if provided, the specified zone
        /// and shape; otherwise, false.</returns>
        public static bool IsInRoom(this Vector3 position, RoomName name, FacilityZone? zone = null, RoomShape? shape = null)
        {
            if (!position.TryGetRoom(out var room))
                return false;

            if (room.Name != name)
                return false;

            if (zone.HasValue && room.Zone != zone.Value)
                return false;

            if (shape.HasValue && room.Shape != shape.Value)
                return false;

            return true;
        }

        /// <summary>
        /// Attempts to teleport the player to a random spawn position associated with a randomly selected team and
        /// role, optionally excluding specified teams and roles.
        /// </summary>
        /// <remarks>The method will not teleport the player if no valid teams or roles are available
        /// after applying exclusions, or if the player is not alive. The player's position is updated only on
        /// successful teleportation.</remarks>
        /// <param name="player">The player to teleport. Must not be null and must be alive.</param>
        /// <param name="teams">An array of teams to consider for random selection. If null, all available teams are used.</param>
        /// <param name="excludedTeams">An array of teams to exclude from random selection. If null or empty, no teams are excluded.</param>
        /// <param name="excludedRoles">An array of role types to exclude from random selection within the chosen team. If null or empty, no roles
        /// are excluded.</param>
        /// <returns>true if the player was successfully teleported to a random spawn position; otherwise, false.</returns>
        public static bool RandomSpawnPositionTeleport(this ExPlayer player, Team[]? teams = null, Team[]? excludedTeams = null, RoleTypeId[]? excludedRoles = null)
        {
            if (player?.ReferenceHub == null)
                return false;

            if (!player.IsAlive)
                return false;

            teams ??= AllTeams;

            if (excludedTeams?.Length > 0)
                teams = teams.Except(excludedTeams).ToArray();

            if (teams.Length == 0)
                return false;

            var randomRole = teams.GetRandomRole(excludedRoles ?? Array.Empty<RoleTypeId>());

            if (!randomRole.TryGetSpawnPosition(out var position, out _))
                return false;

            player.Position.Position = position;
            return true;
        }

        /// <summary>
        /// Attempts to teleport the player to a random room within the specified facility zones.
        /// </summary>
        /// <remarks>The method does not perform any action if the player is not alive or if required
        /// movement components are unavailable. If no valid destination is found within the specified zones, the
        /// teleportation does not occur.</remarks>
        /// <param name="player">The player to teleport. Must not be null and must be alive.</param>
        /// <param name="zones">An array of facility zones to select the destination room from. If null, all zones are considered.</param>
        /// <returns>true if the player was successfully teleported to a random room; otherwise, false.</returns>
        public static bool RandomRoomTeleport(this ExPlayer player, FacilityZone[]? zones)
        {
            if (player?.ReferenceHub == null)
                return false;

            if (!player.IsAlive)
                return false;

            if (player.Role.MovementModule == null || player.Role.MovementModule.CharController == null)
                return false;

            var position = ExMap.GetPocketExitPosition(player, zones ?? AllZones);

            if (position != Vector3.zero)
            {
                player.Position.Position = position;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Calculates a modified velocity for the player by applying the specified multiplier to each component of the vector.
        /// </summary>
        /// <param name="player">The player whose velocity is to be modified.</param>
        /// <param name="multiplier">The multiplier to apply to each component of the player's velocity.</param>
        /// <returns>The modified velocity as a vector after applying the multiplier.</returns>
        public static Vector3 MultipliedVelocity(this ExPlayer player, float multiplier)
        {
            var velocity = player.Velocity;

            if (velocity.x > 0f) velocity.x *= multiplier;
            if (velocity.y > 0f) velocity.y *= multiplier;
            if (velocity.z > 0f) velocity.z *= multiplier;

            if (velocity.x < 0f) velocity.x *= -multiplier;
            if (velocity.y < 0f) velocity.y *= -multiplier;
            if (velocity.z < 0f) velocity.z *= -multiplier;

            return velocity;
        }
        
        /// <summary>
        /// Adjusts the Y-coordinate of the player's current position by the specified value.
        /// </summary>
        /// <param name="player">The player whose position is to be adjusted.</param>
        /// <param name="adjustment">The value by which to adjust the Y-coordinate.</param>
        /// <returns>The updated position with the adjusted Y-coordinate.</returns>
        public static Vector3 PositionAdjustY(this ExPlayer player, float adjustment)
        {
            var position = player.Position.Position;

            if (adjustment != 0f)
                position.y += adjustment;

            return position;
        }

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