using LabExtended.API;

using SecretLabAPI.Levels.Interfaces;

namespace SecretLabAPI.Levels.Events
{
    /// <summary>
    /// Provides data for the event that occurs when experience is added to a player.
    /// </summary>
    public class AddedExperienceEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the player receiving the experience.
        /// </summary>
        public ExPlayer Player { get; }

        /// <summary>
        /// Gets the reward being given.
        /// </summary>
        public ILevelReward Reward { get; }

        /// <summary>
        /// Gets or sets the amount of experience to be given.
        /// </summary>
        public int Amount { get; }

        /// <summary>
        /// Initializes a new instance of the AddedExperienceEventArgs class with the specified player, reward, and
        /// experience amount.
        /// </summary>
        /// <param name="player">The player who is receiving the experience.</param>
        /// <param name="reward">The level reward associated with the experience being added.</param>
        /// <param name="amount">The amount of experience to be added.</param>
        public AddedExperienceEventArgs(ExPlayer player, ILevelReward reward, int amount)
        {
            Player = player;
            Reward = reward;
            Amount = amount;
        }
    }
}