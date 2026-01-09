using SecretLabAPI.Utilities.Configs;

namespace SecretLabAPI.Levels.Interfaces
{
    /// <summary>
    /// Base interface for level rewards.
    /// </summary>
    public interface ILevelReward
    {
        /// <summary>
        /// Gets or sets the ID of the reward.
        /// </summary>
        string RewardId { get; set; }

        /// <summary>
        /// Gets or sets the name of the reward.
        /// </summary>
        string RewardName { get; set; }

        /// <summary>
        /// Gets or sets the amount of points added.
        /// </summary>
        Int32Range PointsRange { get; set; }

        /// <summary>
        /// Gets or sets the amount of XP added.
        /// </summary>
        Int32Range ExperienceRange { get; set; }

        /// <summary>
        /// Gets or sets the experience multipliers for different player groups.
        /// </summary>
        Dictionary<string, int> ExperienceMultipliers { get; set; }

        /// <summary>
        /// Gets called once the reward is registered in the level manager.
        /// </summary>
        void EnableReward();

        /// <summary>
        /// Gets called once the reward is unregistered from the level manager.
        /// </summary>
        void DisableReward();
    }
}