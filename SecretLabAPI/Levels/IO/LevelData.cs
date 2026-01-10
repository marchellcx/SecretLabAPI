using LabExtended.Core.Storage;

using Mirror;

namespace SecretLabAPI.Levels.IO
{
    /// <summary>
    /// Represents the player's level, points, and experience data for storage.
    /// </summary>
    public class LevelData : StorageValue
    {
        private byte level = 1;

        private int points = 0;
        private int experience = 0;

        /// <summary>
        /// Gets or sets the current level of the player.
        /// </summary>
        public byte Level
        {
            get => level;
            set => SetField(ref level, value);
        }

        /// <summary>
        /// Gets or sets the amount of points the player has.
        /// </summary>
        public int Points
        {
            get => points;
            set => SetField(ref points, value);
        }

        /// <summary>
        /// Gets or sets the amount of experience the player has.
        /// </summary>
        public int Experience
        {
            get => experience;
            set => SetField(ref experience, value);
        }

        /// <summary>
        /// Gets or sets the minimum amount of experience required to reach the next level.
        /// </summary>
        public int RequiredExperience { get; set; }

        /// <inheritdoc/>
        public override void ApplyDefault()
        {
            base.ApplyDefault();

            level = 1;
            points = 0;
            experience = 0;
        }

        /// <inheritdoc/>
        public override void WriteValue(NetworkWriter writer)
        {
            base.WriteValue(writer);

            writer.WriteByte(level);
            writer.WriteInt(points);
            writer.WriteInt(experience);
        }

        /// <inheritdoc/>
        public override void ReadValue(NetworkReader reader)
        {
            base.ReadValue(reader);

            level = reader.ReadByte();
            points = reader.ReadInt();
            experience = reader.ReadInt();
        }
    }
}