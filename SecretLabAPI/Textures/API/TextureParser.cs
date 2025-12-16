using System.Globalization;

using UnityEngine;

namespace SecretLabAPI.Textures.API
{
    /// <summary>
    /// A utility class for parsing texture data.
    /// </summary>
    public static class TextureParser
    {
        /// <summary>
        /// Asynchronously parses a texture into grid-based sections and populates the specified texture information
        /// with the resulting frames and pixel data.
        /// </summary>
        /// <remarks>This method divides the input texture into a grid as specified by the provided
        /// settings, optionally scaling and adjusting the image before processing. The resulting pixel data and
        /// associated information are added to the loaded frames of the specified texture information object. This
        /// method must be called from a context that supports asynchronous operations.</remarks>
        /// <param name="texture">The source texture to be parsed. Must not be null.</param>
        /// <param name="settings">The settings that control how the texture is processed, including scaling, grid dimensions, and color
        /// adjustments. Must not be null.</param>
        /// <param name="textureInfo">The object to receive the parsed frame and pixel data. Must not be null.</param>
        /// <returns>A task that represents the asynchronous parse operation.</returns>
        public static async Task ParseAsync(Texture2D texture, TextureSettings settings, TextureInfo textureInfo)
        {
            var textureWidth = await TextureUtils.ExecuteOnMainAsync(() => texture.width);
            var textureHeight = await TextureUtils.ExecuteOnMainAsync(() => texture.height);

            if (settings.ScaleInputImage && settings.InputImageScale != 1.0f)
            {
                var newWidth = Mathf.RoundToInt(textureWidth * settings.InputImageScale);
                var newHeight = Mathf.RoundToInt(textureHeight * settings.InputImageScale);

                newWidth = Mathf.Max(1, newWidth);
                newHeight = Mathf.Max(1, newHeight);

                texture = await ScaleTextureNearestNeighborAsync(texture, newWidth, newHeight);

                textureWidth = await TextureUtils.ExecuteOnMainAsync(() => texture.width);
                textureHeight = await TextureUtils.ExecuteOnMainAsync(() => texture.height);
            }

            var dimensions = await settings.GetGridDimensionsAsync(texture);
            var frame = new TextureFrame() { Texture = texture };

            textureInfo.loadedFrames.Add(frame);

            var sectionIndex = 0;
            var totalSections = dimensions.rows * dimensions.cols;

            frame.Args = new string[totalSections];
            frame.Pixels = new Color[totalSections][];

            var pixels = await TextureUtils.ExecuteOnMainAsync(() => texture.GetPixels());

            for (var row = 0; row < dimensions.rows && sectionIndex < totalSections; row++)
            {
                for (var col = 0; col < dimensions.cols && sectionIndex < totalSections; col++)
                {
                    var sectionWidth = textureWidth / dimensions.cols;
                    var sectionHeight = textureHeight / dimensions.rows;

                    var startX = col * sectionWidth;
                    var startY = row * sectionHeight;

                    var width = (col == dimensions.cols - 1) ? textureWidth - startX : sectionWidth;
                    var height = (row == dimensions.rows - 1) ? textureHeight - startY : sectionHeight;

                    var sectionPixels = new Color[width * height];
                    var pixelIndex = 0;

                    for (var y = 0; y < height; y++)
                    {
                        for (var x = 0; x < width; x++)
                        {
                            var srcX = startX + x;
                            var srcY = startY + y;

                            var originalColor = (srcX >= 0 && srcX < textureWidth && srcY >= 0 && srcY < textureHeight)
                                ? pixels[srcY * textureWidth + srcX]
                                : Color.clear;
                            var adjustedColor = TextureUtils.AdjustColor(originalColor, settings.Brightness, settings.Contrast, settings.Gamma,
                                settings.DisableAlpha);

                            sectionPixels[pixelIndex++] = adjustedColor;
                        }
                    }

                    frame.Pixels[sectionIndex] = sectionPixels;
                    frame.Args[sectionIndex] = GenerateOptimizedRichText(sectionPixels, width, height, settings.LetterSpacing, settings.FontSize,
                        settings.LineSpacing, settings.FillCharacter);

                    sectionIndex++;
                }
            }
        }

        /// <summary>
        /// Generates a rich text string representing a pixel grid, with color and spacing formatting optimized for
        /// display in Unity or similar environments.
        /// </summary>
        /// <remarks>Transparent pixels (alpha less than 0.01) are rendered with a transparent color tag.
        /// The output string uses Unity-style rich text tags for color, size, line height, and character spacing. This
        /// method is intended for use in environments that support Unity rich text formatting.</remarks>
        /// <param name="pixels">An array of colors representing the pixels to render. The array should contain at least width × height
        /// elements, ordered row by row.</param>
        /// <param name="width">The number of columns in the pixel grid. Must be greater than 0.</param>
        /// <param name="height">The number of rows in the pixel grid. Must be greater than 0.</param>
        /// <param name="letterSpacing">The spacing between characters, in arbitrary units. Used to calculate the <cspace> tag value in em units.</param>
        /// <param name="fontSize">The font size to use for the generated text, in points.</param>
        /// <param name="lineSpacing">The line height to use for the generated text, in points.</param>
        /// <param name="fillChar">The character to use for each pixel block in the output string.</param>
        /// <returns>A formatted rich text string representing the colored pixel grid, with specified spacing and font settings.
        /// Returns an empty string if the pixels array is null or empty.</returns>
        public static string GenerateOptimizedRichText(Color[] pixels, int width, int height, float letterSpacing, 
            float fontSize, float lineSpacing, char fillChar)
        {
            if (pixels == null || pixels.Length == 0) 
                return "";

            float cspaceEm = letterSpacing / 10f;

            var cspaceFormatted = cspaceEm.ToString(CultureInfo.InvariantCulture) + "em";
            var fontSizeFormatted = fontSize.ToString(CultureInfo.InvariantCulture);
            var lineSpacingFormatted = lineSpacing.ToString(CultureInfo.InvariantCulture);

            var result = $"<size={fontSizeFormatted}><line-height={lineSpacingFormatted}><cspace={cspaceFormatted}>";

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width;)
                {
                    var index = y * width + x;

                    if (index >= pixels.Length) 
                        break;

                    var currentColor = pixels[index];
                    var count = 1;

                    while (x + count < width && (y * width + x + count) < pixels.Length)
                    {
                        var nextColor = pixels[y * width + x + count];

                        if (!TextureUtils.ColorsAreEqual(currentColor, nextColor)) 
                            break;

                        count++;
                    }

                    string colorHex = TextureUtils.ColorToHex(currentColor);
                    string blocks = new string(fillChar, count);

                    if (currentColor.a < 0.01f)
                    {
                        result += $"<color=#0000>{blocks}</color>";
                    }
                    else
                    {
                        result += $"<color=#{colorHex}>{blocks}</color>";
                    }

                    x += count;
                }

                if (y < height - 1)
                {
                    result += "\n";
                }
            }

            result += "</cspace></line-height></size>";
            return result;
        }

        /// <summary>
        /// Asynchronously creates a new Texture2D by scaling the source texture to the specified width and height using
        /// nearest-neighbor interpolation.
        /// </summary>
        /// <remarks>This method must be called from a context that supports asynchronous operations. The
        /// returned texture uses RGBA32 format, point filtering, and clamp wrap mode. The scaling operation is
        /// performed on the main thread to ensure thread safety with Unity's texture APIs.</remarks>
        /// <param name="source">The source Texture2D to be scaled. Cannot be null.</param>
        /// <param name="newWidth">The width, in pixels, of the resulting scaled texture. Must be greater than 0.</param>
        /// <param name="newHeight">The height, in pixels, of the resulting scaled texture. Must be greater than 0.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the newly created Texture2D
        /// scaled to the specified dimensions.</returns>
        public static async Task<Texture2D> ScaleTextureNearestNeighborAsync(Texture2D source, int newWidth, int newHeight)
        {
            var scaledImage = await TextureUtils.ExecuteOnMainAsync(() => 
            {
                var texture = new Texture2D(newWidth, newHeight, TextureFormat.RGBA32, false);

                texture.filterMode = FilterMode.Point;
                texture.wrapMode = TextureWrapMode.Clamp;

                return texture;
            });

            var scaledPixels = new Color[newWidth * newHeight];

            for (var y = 0; y < newHeight; y++)
            {
                for (var x = 0; x < newWidth; x++)
                {
                    var srcX = Mathf.RoundToInt((float)x / newWidth * source.width);
                    var srcY = Mathf.RoundToInt((float)y / newHeight * source.height);

                    srcX = Mathf.Clamp(srcX, 0, source.width - 1);
                    srcY = Mathf.Clamp(srcY, 0, source.height - 1);

                    scaledPixels[y * newWidth + x] = await TextureUtils.ExecuteOnMainAsync(() => source.GetPixel(srcX, srcY));
                }
            }

            await TextureUtils.ExecuteOnMainAsync(() =>
            {
                scaledImage.SetPixels(scaledPixels);
                scaledImage.Apply();
            });

            return scaledImage;
        }
    }
}