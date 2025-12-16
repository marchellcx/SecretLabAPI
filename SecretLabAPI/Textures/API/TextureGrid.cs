namespace SecretLabAPI.Textures.API
{
    /// <summary>
    /// Defines the size of a texture grid.
    /// </summary>
    public enum TextureGrid
    {
        /// <summary>
        /// Automatic grid selection.
        /// </summary>
        Auto = -1,

        /// <summary>
        /// Single grid cell.
        /// </summary>
        Single = 1,

        /// <summary>
        /// 2x2 grid.
        /// </summary>
        Grid2x2 = 2,

        /// <summary>
        /// 4x4 grid.
        /// </summary>
        Grid4x4 = 4,

        /// <summary>
        /// Custom grid size.
        /// </summary>
        Custom = 0
    }
}