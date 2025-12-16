using Newtonsoft.Json;

using UnityEngine;

namespace SecretLabAPI.Textures.API
{
    /// <summary>
    /// Configs for texture utilities.
    /// </summary>
    public class TextureSettings
    {
        /// <summary>
        /// Gets or sets the character used to fill the texture.
        /// </summary>
        [JsonProperty("fillCharacter")]
        public volatile char FillCharacter = '█';

        /// <summary>
        /// Gets or sets the font size for the texture.
        /// </summary>
        [JsonProperty("fontSize")]
        public volatile float FontSize = 1f;

        /// <summary>
        /// Gets or sets the spacing between characters in the texture.
        /// </summary>
        [JsonProperty("spacing")]
        public volatile float Spacing = 1.951f;

        /// <summary>
        /// Gets or sets the line spacing for the texture.
        /// </summary>
        [JsonProperty("lineSpacing")]
        public volatile float LineSpacing = 0.61f;

        /// <summary>
        /// Gets or sets the letter spacing for the texture.
        /// </summary>
        [JsonProperty("letterSpacing")]
        public volatile float LetterSpacing = 0.612f;

        /// <summary>
        /// Gets or sets the brightness adjustment for the texture.
        /// </summary>
        [JsonProperty("brightness")]
        public volatile float Brightness = 1.0f;

        /// <summary>
        /// Gets or sets the contrast adjustment for the texture.
        /// </summary>
        [JsonProperty("contrast")]
        public volatile float Contrast  = 1.0f;

        /// <summary>
        /// Gets or sets the gamma correction for the texture.
        /// </summary>
        [JsonProperty("gamma")]
        public volatile float Gamma = 1.0f;

        /// <summary>
        /// Gets or sets the target size for the texture.
        /// </summary>
        [JsonProperty("targetSize")]
        public volatile float TargetSize = 2f;

        /// <summary>
        /// Gets or sets a value indicating whether to disable the alpha channel in the texture.
        /// </summary>
        [JsonProperty("disableAlpha")]
        public volatile bool DisableAlpha = false;

        /// <summary>
        /// Gets or sets the multiplier for auto spacing calculation.
        /// </summary>
        [JsonProperty("autoSpacingMultiplier")]
        public volatile float AutoSpacingMultiplier = 0.03038f;

        /// <summary>
        /// Gets or sets the offset for auto spacing calculation.
        /// </summary>
        [JsonProperty("autoSpacingOffset")]
        public volatile float AutoSpacingOffset  = 0.0f;

        /// <summary>
        /// Gets or sets the scale factor for the input image.
        /// </summary>
        [JsonProperty("inputImageScale")]
        public volatile float InputImageScale = 1.0f;

        /// <summary>
        /// Gets or sets the number of custom rows for the texture grid.
        /// </summary>
        [JsonProperty("customRows")]
        public volatile int CustomRows = 2;

        /// <summary>
        /// Gets or sets the number of custom columns for the texture grid.
        /// </summary>
        [JsonProperty("customColumns")]
        public volatile int CustomColumns = 2;

        /// <summary>
        /// Gets or sets the grid mode for the texture.
        /// </summary>
        [JsonProperty("grid")]
        public volatile TextureGrid Grid = TextureGrid.Single;

        /// <summary>
        /// Gets a value indicating whether to scale the input image.
        /// </summary>
        public bool ScaleInputImage => InputImageScale != 1.0f;

        /// <summary>
        /// Calculates the number of rows and columns in the grid for the specified texture based on the current grid
        /// settings.
        /// </summary>
        /// <remarks>The grid dimensions are determined by the current value of the Grid property. If Grid
        /// is set to Auto, the dimensions are calculated based on the texture's size and the InputImageScale property.
        /// For other grid modes, the dimensions are determined by the selected grid type or custom values. The method
        /// always returns values greater than or equal to 1.</remarks>
        /// <param name="texture">The texture for which to determine grid dimensions. If null, the method returns (1, 1).</param>
        /// <returns>A tuple containing the number of rows and columns in the grid. Both values are at least 1.</returns>
        public (int rows, int cols) GetGridDimensions(Texture2D texture)
        {
            var width = texture.width;
            var height = texture.height;

            int rows = 1;
            int cols = 1;

            if (Grid == TextureGrid.Auto)
            {
                if (texture == null)
                {
                    rows = 1;
                    cols = 1;

                    return (rows, cols);
                }

                var effectiveWidth = width;
                var effectiveHeight = height;

                if (ScaleInputImage && InputImageScale != 1.0f)
                {
                    effectiveWidth = Mathf.RoundToInt(width * InputImageScale);
                    effectiveHeight = Mathf.RoundToInt(height * InputImageScale);
                }

                const int targetSectionSide = 52;

                rows = Mathf.Max(1, Mathf.CeilToInt((float)effectiveHeight / targetSectionSide));
                cols = Mathf.Max(1, Mathf.CeilToInt((float)effectiveWidth / targetSectionSide));
            }
            else
            {
                switch (Grid)
                {
                    case TextureGrid.Single:
                        rows = 1;
                        cols = 1;
                        break;

                    case TextureGrid.Grid2x2:
                        rows = 2;
                        cols = 2;
                        break;

                    case TextureGrid.Grid4x4:
                        rows = 4;
                        cols = 4;
                        break;

                    case TextureGrid.Custom:
                        rows = Mathf.Max(1, CustomRows);
                        cols = Mathf.Max(1, CustomColumns);
                        break;

                    default:
                        rows = 1;
                        cols = 1;
                        break;
                }
            }

            return (rows, cols);
        }

        /// <summary>
        /// Asynchronously calculates the number of rows and columns for dividing the specified texture into a grid,
        /// based on the current grid settings.
        /// </summary>
        /// <remarks>The grid dimensions are determined by the current value of the Grid property. If Grid
        /// is set to Auto, the method calculates the grid size based on the texture's dimensions and the
        /// InputImageScale property, aiming for sections of approximately 52 pixels per side. For other grid modes, the
        /// dimensions are determined by the selected preset or custom values. If the texture is null and Grid is Auto,
        /// the method returns (1, 1).</remarks>
        /// <param name="texture">The texture to analyze for grid division. Cannot be null when using automatic grid calculation.</param>
        /// <returns>A tuple containing the number of rows and columns that define the grid for the given texture. Both values
        /// are at least 1.</returns>
        public async Task<(int rows, int cols)> GetGridDimensionsAsync(Texture2D texture)
        {
            var width = await TextureUtils.ExecuteOnMainAsync(() => texture.width);
            var height = await TextureUtils.ExecuteOnMainAsync(() => texture.height);

            int rows = 1;
            int cols = 1;

            if (Grid == TextureGrid.Auto)
            {
                if (texture == null)
                {
                    rows = 1;
                    cols = 1;

                    return (rows, cols);
                }

                var effectiveWidth = width;
                var effectiveHeight = height;

                if (ScaleInputImage && InputImageScale != 1.0f)
                {
                    effectiveWidth = Mathf.RoundToInt(width * InputImageScale);
                    effectiveHeight = Mathf.RoundToInt(height * InputImageScale);
                }

                const int targetSectionSide = 52;

                rows = Mathf.Max(1, Mathf.CeilToInt((float)effectiveHeight / targetSectionSide));
                cols = Mathf.Max(1, Mathf.CeilToInt((float)effectiveWidth / targetSectionSide));
            }
            else
            {
                switch (Grid)
                {
                    case TextureGrid.Single:
                        rows = 1;
                        cols = 1;
                        break;

                    case TextureGrid.Grid2x2:
                        rows = 2;
                        cols = 2;
                        break;

                    case TextureGrid.Grid4x4:
                        rows = 4;
                        cols = 4;
                        break;

                    case TextureGrid.Custom:
                        rows = Mathf.Max(1, CustomRows);
                        cols = Mathf.Max(1, CustomColumns);
                        break;

                    default:
                        rows = 1;
                        cols = 1;
                        break;
                }
            }

            return (rows, cols);
        }
    }
}