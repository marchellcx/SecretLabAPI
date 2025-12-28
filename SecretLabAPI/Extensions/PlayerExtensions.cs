using LabExtended.API;
using LabExtended.Extensions;

using MapGeneration;

using UnityEngine;

namespace SecretLabAPI.Extensions
{
    /// <summary>
    /// Provides extension methods for the ExPlayer type.
    /// </summary>
    public static class PlayerExtensions
    {
        /// <summary>
        /// Provides an array containing all defined facility zones except for None and Other.
        /// </summary>
        /// <remarks>This array can be used to iterate over all standard facility zones, excluding special
        /// values such as None and Other that may represent undefined or miscellaneous cases.</remarks>
        public static FacilityZone[] AllZones = EnumUtils<FacilityZone>.Values.Where(z => z != FacilityZone.None && z != FacilityZone.Other).ToArray();

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
    }
}