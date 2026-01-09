using LabExtended.API;

using SecretLabAPI.Extensions;
using SecretLabAPI.Levels.Interfaces;
using SecretLabAPI.Utilities.Configs;

using System.ComponentModel;

using YamlDotNet.Serialization;

namespace SecretLabAPI.Levels
{
    /// <summary>
    /// Base class for level rewards.
    /// </summary>
    public class LevelReward : ILevelReward
    {
        /// <inheritdoc/>
        [Description("Sets the unique identifier for the reward.")]
        public virtual string RewardId { get; set; } = string.Empty;

        /// <inheritdoc/>
        [Description("Sets the name of the reward.")]
        public virtual string RewardName { get; set; } = string.Empty;

        /// <inheritdoc/>
        [Description("The range of currency points to be awarded.")]
        public virtual Int32Range PointsRange { get; set; } = new() { MinValue = 0, MaxValue = 0 };

        /// <inheritdoc/>
        [Description("The range of experience points to be awarded.")]
        public virtual Int32Range ExperienceRange { get; set; } = new() { MinValue = 0, MaxValue = 0 };

        /// <inheritdoc/>
        [Description("Sets multipliers for different players (key can be the player's user ID or permissions group).")]
        public virtual Dictionary<string, int> ExperienceMultipliers { get; set; }

        /// <summary>
        /// Gets a randomly selected number of points within the defined range.
        /// </summary>
        [YamlIgnore]
        public int RandomPoints => PointsRange.GetRandom();

        /// <summary>
        /// Calculates the total experience points to award to the specified player, including any applicable
        /// multipliers.
        /// </summary>
        /// <param name="player">The player for whom to calculate the experience points. Cannot be null.</param>
        /// <returns>The total experience points to award to the player, including any valid multipliers. The value is always
        /// non-negative.</returns>
        public int GetExperience(ExPlayer player)
        {
            var baseExp = ExperienceRange.GetRandom();
            var addedExp = player.GetValidInt32Multipliers(ExperienceMultipliers, 0);

            if (addedExp > 0)
                baseExp += addedExp;

            return baseExp;
        }

        /// <summary>
        /// Attempts to add a random number of points to the specified player.
        /// </summary>
        /// <param name="player">The player to whom points will be added. Cannot be null and must have a valid ReferenceHub.</param>
        /// <returns>true if points were successfully added to the player; otherwise, false.</returns>
        public bool AddPoints(ExPlayer player)
        {
            if (player?.ReferenceHub == null)
                return false;

            var points = RandomPoints;

            if (points < 1)
                return false;

            return player.AddPoints(this, points);
        }

        /// <summary>
        /// Attempts to add experience points to the specified player.
        /// </summary>
        /// <param name="player">The player to whom experience points will be added. Cannot be null and must have a valid ReferenceHub.</param>
        /// <returns>true if experience points were successfully added to the player; otherwise, false.</returns>
        public bool AddExperience(ExPlayer player)
        {
            if (player?.ReferenceHub == null)
                return false;

            var exp = GetExperience(player);

            if (exp < 1)
                return false;

            return player.AddExperience(this, exp);
        }

        /// <inheritdoc/>
        public virtual void EnableReward()
        {

        }

        /// <inheritdoc/>
        public virtual void DisableReward()
        {

        }
    }
}