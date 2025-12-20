using LabExtended.API;

using UnityEngine;

namespace SecretLabAPI.Extensions
{
    /// <summary>
    /// Provides extension methods for the ExPlayer type.
    /// </summary>
    public static class PlayerExtensions
    {
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