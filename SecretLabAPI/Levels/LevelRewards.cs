using System.ComponentModel;

using LabExtended.Utilities;

using SecretLabAPI.Levels.Rewards;
using SecretLabAPI.Utilities.Configs;

namespace SecretLabAPI.Levels
{
    /// <summary>
    /// Configures rewards based on player actions.
    /// </summary>
    public class LevelRewards
    {
        /// <summary>
        /// Gets the active level rewards config.
        /// </summary>
        public static LevelRewards Rewards { get; private set; }

        /// <summary>
        /// Gets or sets the configured experience range gained for killing a human enemy.
        /// </summary>
        [Description("Sets the experience gained for killing a human enemy.")]
        public Int32Range KilledHumanExp { get; set; } = new() { MinValue = 0, MaxValue = 0 };

        /// <summary>
        /// Gets or sets the experience range awarded for killing an SCP.
        /// </summary>
        [Description("Sets the experience gained for killing an SCP.")]
        public Int32Range KilledScpExp { get; set; } = new() { MinValue = 0, MaxValue = 0 };

        /// <summary>
        /// Gets or sets the experience subtracted when a player dies to a human enemy.
        /// </summary>
        [Description("Sets the experience subtracted for dying to a human enemy.")]
        public Int32Range DiedToHumanXp { get; set; } = new() { MinValue = 0, MaxValue = 0 };

        /// <summary>
        /// Gets or sets the experience subtracted for dying to an SCP.
        /// </summary>
        [Description("Sets the experience subtracted for dying to an SCP.")]
        public Int32Range DiedToScpXp { get; set; } = new() { MinValue = 0, MaxValue = 0 };

        /// <summary>
        /// Gets or sets the experience subtracted for dying due to an unknown reason, such as suicide or other unintended causes.
        /// </summary>
        [Description("Sets the experience subtracted for dying for an unknown reason (not by an enemy).")]
        public Int32Range DiedBySuicideXp { get; set; } = new() { MinValue = 0, MaxValue = 0 };

        /// <summary>
        /// Gets or sets the experience rewarded for activating a generator.
        /// </summary>
        [Description("Sets the experience gained for activating a generator.")]
        public Int32Range ActivatedGeneratorXp { get; set; } = new() { MinValue = 0, MaxValue = 0 };

        /// <summary>
        /// Gets or sets the experience points subtracted from all SCP players when a generator is activated.
        /// </summary>
        [Description("Sets the amount of XP subtracted from EVERY SCP player when a generator activates.")]
        public Int32Range ScpActivatedGeneratorXp { get; set; } = new() { MinValue = 0, MaxValue = 0 };

        [Description("Sets the amount of XP subtracted from a player that deactivates a generator as a human.")]
        public Int32Range HumanDeactivatedGeneratorXp { get; set; } = new() { MinValue = 0, MaxValue = 0 };

        [Description("Sets the amount of XP gained if an SCP disables a generator with SCP-079 active.")]
        public Int32Range ScpDeactivatedGeneratorXp { get; set; } = new() { MinValue = 0, MaxValue = 0 };
        
        internal static void InitializeRewards()
        {
            if (FileUtils.TryLoadYamlFile<LevelRewards>(SecretLab.RootDirectory, "level_rewards.yml", out var rewards))
            {
                Rewards = rewards;
            }
            else
            {
                FileUtils.TrySaveYamlFile(SecretLab.RootDirectory, "level_rewards.yml", Rewards = new());
            }
            
            KillRewards.Initialize();
            MapRewards.Initialize();
        }
    }
}