using SecretLabAPI.Levels.Interfaces;
using SecretLabAPI.Utilities.Configs;

using System.ComponentModel;

namespace SecretLabAPI.Levels
{
    public class LevelReward : ILevelReward
    {
        /// <inheritdoc/>
        [Description("Sets the unique identifier for the reward.")]
        public virtual string RewardId { get; set; }

        /// <inheritdoc/>
        [Description("Sets the name of the reward.")]
        public virtual string RewardName { get; set; }

        /// <inheritdoc/>
        [Description("The range of currency points to be awarded.")]
        public virtual Int32Range PointsRange { get; set; } = new() { MinValue = 0, MaxValue = 0 };

        /// <inheritdoc/>
        [Description("The range of experience points to be awarded.")]
        public virtual Int32Range ExperienceRange { get; set; } = new() { MinValue = 0, MaxValue = 0 };

        /// <inheritdoc/>
        [Description("Sets multipliers for different players (key can be the player's user ID or permissions group).")]
        public virtual Dictionary<string, int> ExperienceMultipliers { get; set; }

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