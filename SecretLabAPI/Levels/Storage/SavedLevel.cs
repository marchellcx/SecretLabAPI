using LabExtended.Core.Storage;

using Mirror;

namespace SecretLabAPI.Levels.Storage
{
    /// <summary>
    /// Represents a saved level, including the level number and experience points.
    /// </summary>
    public class SavedLevel : StorageValue
    {
        private byte level = 1;
        private int experience = 0;

        /// <summary>
        /// Gets a value indicating whether the level progression is capped.
        /// </summary>
        /// <remarks>
        /// The level is considered capped if the required experience is set to -1
        /// or if the next level exceeds the maximum level cap defined by <see cref="LevelProgress.Cap"/>.
        /// </remarks>
        public bool IsCapped => RequiredExperience == -1 || Level + 1 >= LevelProgress.Cap;

        /// <summary>
        /// Gets or sets the current level value.
        /// </summary>
        public byte Level
        {
            get => level;
            set => SetField(ref level, value);
        }

        /// <summary>
        /// Gets or sets the experience points.
        /// </summary>
        public int Experience
        {
            get => experience;
            set => SetField(ref experience, value);
        }

        /// <summary>
        /// Gets or sets the experience required to reach the next level.
        /// </summary>
        public int RequiredExperience { get; set; }

        /// <inheritdoc/>
        public override void ReadValue(NetworkReader reader)
        {
            level = reader.ReadByte();
            experience = reader.ReadInt();
        }

        /// <inheritdoc/>
        public override void WriteValue(NetworkWriter writer)
        {
            writer.WriteByte(level);
            writer.WriteInt(experience);
        }

        /// <inheritdoc/>
        public override void ApplyDefault()
        {
            Level = 1;
            Experience = 0;
        }
    }
}