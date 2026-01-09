using LabExtended.API;

using LabExtended.Events;

using SecretLabAPI.Levels.Interfaces;

namespace SecretLabAPI.Levels.Events
{
    /// <summary>
    /// Provides data for the event that occurs when points are about to be added to a player.
    /// </summary>
    public class AddingPointsEventArgs : BooleanEventArgs
    {
        /// <summary>
        /// Gets the player receiving the points.
        /// </summary>
        public ExPlayer Player { get; }

        /// <summary>
        /// Gets the reward being given.
        /// </summary>
        public ILevelReward Reward { get; }

        /// <summary>
        /// Gets or sets the amount of points to be given.
        /// </summary>
        public int Amount { get; set; }

        /// <summary>
        /// Initializes a new instance of the AddingPointsEventArgs class with the specified player, reward, and
        /// points amount.
        /// </summary>
        /// <param name="player">The player who is receiving the points.</param>
        /// <param name="reward">The level reward associated with the points being added.</param>
        /// <param name="amount">The amount of points to be added.</param>
        public AddingPointsEventArgs(ExPlayer player, ILevelReward reward, int amount)
        {
            Player = player;
            Reward = reward;
            Amount = amount;
        }
    }
}