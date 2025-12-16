namespace SecretLabAPI.Textures.API
{
    /// <summary>
    /// Settings for animated textures.
    /// </summary>
    public class AnimatedTextureSettings
    {
        /// <summary>
        /// Gets or sets the delay time, in seconds between frames.
        /// </summary>
        public float DelayTime { get; set; } = 0.2f;

        /// <summary>
        /// Whether or not the animation should loop by default.
        /// </summary>
        public bool Loop { get; set; }

        /// <summary>
        /// The audio clip to play.
        /// </summary>
        public string AudioClip { get; set; } = string.Empty;

        /// <summary>
        /// Texture settings for each frame.
        /// </summary>
        public TextureSettings FrameSettings { get; set; } = new();
    }
}
