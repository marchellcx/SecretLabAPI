using SecretLabAPI.Levels.Storage;
using SecretLabAPI.Levels.Events;

namespace SecretLabAPI.Levels
{
    /// <summary>
    /// Manages level progression.
    /// </summary>
    public static class LevelProgress
    {
        /// <summary>
        /// Gets the maximum level number.
        /// </summary>
        public static byte Cap
        {
            get => SecretLab.Config.LevelCap;
            set => SecretLab.Config.LevelCap = value;
        } 
        
        /// <summary>
        /// Gets the pregenerated array of experience required for each level.
        /// </summary>
        public static int[] RequiredExperience { get; private set; }

        /// <summary>
        /// Determines the player level corresponding to a given amount of experience points.
        /// </summary>
        /// <param name="expAmount">The total experience points earned by the player.</param>
        /// <returns>The player level that matches the provided experience points, or 0 if the experience is insufficient for the first level.</returns>
        public static byte LevelAtExp(int expAmount)
        {
            for (var x = 1; x < Cap; x++)
            {
                if (RequiredExperience[x] > expAmount)
                {
                    return (byte)(x - 1);
                }
            }

            return 0;
        }

        /// <summary>
        /// Retrieves the required experience points needed to reach the specified level.
        /// </summary>
        /// <param name="level">The target level for which the experience points are being queried.</param>
        /// <returns>The experience points required to reach the specified level. Returns 0 if the level exceeds the level cap.</returns>
        public static int ExperienceForLevel(int level)
        {
            if (level < 1)
                throw new ArgumentOutOfRangeException(nameof(level));
            
            if (level > Cap)
                return -1;

            return RequiredExperience[level];
        }

        internal static void CheckProgress(string userId, string reason, SavedLevel level)
        {
            if ((level.Level + 1) > Cap || level.Experience < level.RequiredExperience)
                return;

            var newLevel = LevelAtExp(level.Experience);
            var changingLevelArgs = new ChangingLevelEventArgs(level, userId, reason, level.Level, newLevel);

            if (!LevelEvents.OnChangingLevel(changingLevelArgs))
                return;

            level.Level = newLevel;
            level.RequiredExperience = ExperienceForLevel(newLevel + 1);

            LevelEvents.OnChangedLevel(new(level, userId, reason, changingLevelArgs.CurrentLevel, newLevel), changingLevelArgs.target);
        }

        internal static void Initialize()
        {
            if (Cap < 1)
                Cap = 1;

            RequiredExperience = new int[Cap + 1];
            RequiredExperience[0] = 0;

            var exp = 0;
            var step = SecretLab.Config.LevelStep;

            for (var x = 1; x < Cap; x++)
            {
                if (SecretLab.Config.LevelStepOffsets.TryGetValue((byte)x, out var offset))
                    step += offset;

                exp += step;

                RequiredExperience[x] = exp;
            }
        }
    }
}