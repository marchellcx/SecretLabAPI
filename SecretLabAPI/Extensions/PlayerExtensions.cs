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