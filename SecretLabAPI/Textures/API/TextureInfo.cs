using System.Collections.Concurrent;

namespace SecretLabAPI.Textures.API
{
    /// <summary>
    /// Represents a loaded texture.
    /// </summary>
    public class TextureInfo
    {
        internal volatile List<TextureFrame> frames = new();
        internal volatile ConcurrentBag<TextureFrame> loadedFrames = new();

        internal volatile TextureSettings settings;
        internal volatile AnimatedTextureSettings animatedSettings;

        internal volatile string path;
        internal volatile float delay;
        internal volatile bool loop;

        /// <summary>
        /// The path to the file of the texture.
        /// </summary>
        public string Path => path;

        /// <summary>
        /// The delay between frames.
        /// </summary>
        public float Delay => delay;

        /// <summary>
        /// Whether or not animation should loop.
        /// </summary>
        public bool Loop => loop;

        /// <summary>
        /// Gets the first frame.
        /// </summary>
        public TextureFrame Texture => Frames.Count == 0
            ? null!
            : Frames[0];

        /// <summary>
        /// The settings for the texture.
        /// </summary>
        public TextureSettings Settings => settings;

        /// <summary>
        /// The settings for the animated texture.
        /// </summary>
        public AnimatedTextureSettings AnimatedSettings => animatedSettings;

        /// <summary>
        /// The loaded texture.
        /// </summary>
        public IReadOnlyList<TextureFrame> Frames => frames;
    }
}
