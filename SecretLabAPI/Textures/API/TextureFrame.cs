using UnityEngine;

namespace SecretLabAPI.Textures.API
{
    /// <summary>
    /// The frame of a texture.
    /// </summary>
    public class TextureFrame
    {
        /// <summary>
        /// The original texture.
        /// </summary>
        public volatile Texture2D Texture;

        /// <summary>
        /// The parsed arguments for text toys.
        /// </summary>
        public volatile string[] Args;

        /// <summary>
        /// The calculated spacing.
        /// </summary>
        public volatile float Spacing;

        /// <summary>
        /// The parsed pixels.
        /// </summary>
        public volatile Color[][] Pixels;

        /// <summary>
        /// The index of the frame.
        /// </summary>
        public volatile int Index;
    }
}