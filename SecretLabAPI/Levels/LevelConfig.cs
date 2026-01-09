using SecretLabAPI.Utilities.Configs;
using System.ComponentModel;

namespace SecretLabAPI.Levels
{
    /// <summary>
    /// Represents the config file model for levels.
    /// </summary>
    public class LevelConfig
    {
        /// <summary>
        /// Gets or sets a value indicating whether the server uses shared storage.
        /// </summary>
        [Description("Whether or not to use a shared storage for this server.")]
        public bool SharedStorage { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the level is displayed in custom information.
        /// </summary>
        [Description("Whether or not to show level in custom info.")]
        public bool ShowInCustomInfo { get; set; } = true;

        /// <summary>
        /// Gets or sets the experience increase per-level.
        /// </summary>
        [Description("Sets the experience increase per-level.")]
        public int Step { get; set; } = 100;

        /// <summary>
        /// Gets or sets the maximum achievable level.
        /// </summary>
        [Description("Sets the maximum achievable level.")]
        public byte Cap { get; set; } = 100;

        /// <summary>
        /// Gets or sets the step offsets for different level ranges.
        /// </summary>
        [Description("Sets the level step offsets for different level ranges.")]
        public Dictionary<byte, int> Offsets { get; set; } = new()
        {
            [21] = 1900
        };

        /// <summary>
        /// Gets or sets the overlay options for level display.
        /// </summary>
        [Description("Overlay options for level display.")]
        public OverlayOptions Overlay { get; set; } = new();
    }
}