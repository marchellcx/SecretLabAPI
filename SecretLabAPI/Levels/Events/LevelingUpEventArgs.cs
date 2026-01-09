using LabExtended.API;

using LabExtended.Events;

using SecretLabAPI.Levels.Interfaces;

namespace SecretLabAPI.Levels.Events
{
    /// <summary>
    /// Provides data for the event that occurs when a player is about to level up.
    /// </summary>
    public class LevelingUpEventArgs : BooleanEventArgs
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
        /// Gets the current level.
        /// </summary>
        public int CurrentLevel { get; }

        /// <summary>
        /// Gets or sets the next level.
        /// </summary>
        public int NextLevel { get; set; }

        /// <summary>
        /// Initializes a new instance of the LevelingUpEventArgs class with the specified player, reward, and
        /// current and next levels.
        /// </summary>
        /// <param name="player">The player who is leveling up.</param>
        /// <param name="reward">The level reward associated with the level up.</param>
        /// <param name="currentLevel">The current level of the player.</param>
        /// <param name="nextLevel">The next level of the player.</param>
        public LevelingUpEventArgs(ExPlayer player, ILevelReward reward, int currentLevel, int nextLevel)
        {
            Player = player;
            Reward = reward;
            CurrentLevel = currentLevel;
            NextLevel = nextLevel;
        }
    }
}