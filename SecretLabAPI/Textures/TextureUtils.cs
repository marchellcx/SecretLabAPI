using LabExtended.API;
using LabExtended.API.Toys;

using LabExtended.Core;
using LabExtended.Extensions;

using UnityEngine;

using SecretLabAPI.Textures.API;

namespace SecretLabAPI.Textures
{
    /// <summary>
    /// Utility methods for texture parsing.
    /// </summary>
    public static class TextureUtils
    {
        /// <summary>
        /// Executes the specified action asynchronously on the main thread and returns a task that completes when the
        /// action has finished executing.
        /// </summary>
        /// <remarks>If the action throws an exception, the returned task will be faulted with that
        /// exception. This method is useful for scheduling work that must run on the main thread from a background
        /// thread.</remarks>
        /// <param name="action">The action to execute on the main thread. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous operation. The task completes when the action has finished executing
        /// on the main thread.</returns>
        public static async Task ExecuteOnMainAsync(Action action)
        {
            var tcs = new TaskCompletionSource<object>();

            MainThreadDispatcher.UpdateDispatcher.Dispatch(() =>
            {
                try
                {
                    action();

                    tcs.SetResult(default!);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);

                    ApiLog.Debug("TextureUtils", $"Action &3{action.Method}&r completed with exception:\n{ex}");
                }
            });

            await tcs.Task;
        }

        /// <summary>
        /// Executes the specified function on the main thread and returns its result asynchronously.
        /// </summary>
        /// <remarks>If the function throws an exception, the returned task will be faulted with that
        /// exception. This method is useful for invoking code that must run on the main thread from a background
        /// thread.</remarks>
        /// <typeparam name="T">The type of the value returned by the function.</typeparam>
        /// <param name="func">The function to execute on the main thread. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the value returned by the
        /// function.</returns>
        public static async Task<T> ExecuteOnMainAsync<T>(Func<T> func)
        {
            var tcs = new TaskCompletionSource<T>();

            MainThreadDispatcher.UpdateDispatcher.Dispatch(() =>
            {
                try
                {
                    var result = func();

                    tcs.SetResult(result);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);

                    ApiLog.Debug("TextureUtils", $"Function &3{func.Method}&r completed with exception:\n{ex}");
                }
            });

            return await tcs.Task;
        }

        /// <summary>
        /// Animates a texture by updating the format and arguments of each toy in the specified texture instance based
        /// on the provided frame data.
        /// </summary>
        /// <remarks>This method updates the format and arguments of each toy in the texture instance
        /// according to the grid layout and frame data. The number of toys updated is determined by the grid dimensions
        /// and the length of the frame's pixel and argument arrays. If the argument array is shorter than required,
        /// only the available sections are processed.</remarks>
        /// <param name="texture">The texture information containing settings and grid dimensions used to process the animation.</param>
        /// <param name="instance">The texture instance whose toys will be updated to reflect the current animation frame.</param>
        /// <param name="frame">The frame data containing pixel and argument information used to animate the texture.</param>
        public static void AnimateTexture(TextureInfo texture, TextureInstance instance, TextureFrame frame)
        {
            var dimensions = texture.Settings.GetGridDimensions(frame.Texture);

            var toyIndex = 0;
            var sectionIndex = 0;

            var list = new List<string>();

            for (var row = 0; row < dimensions.rows && sectionIndex < frame.Pixels.Length; row++)
            {
                for (var col = 0; col < dimensions.cols && sectionIndex < frame.Pixels.Length; col++)
                {
                    if (sectionIndex >= frame.Args.Length)
                    {
                        ApiLog.Debug("TextureUtils", "sectionIndex out of range");
                        break;
                    }

                    toyIndex++;

                    var toy = instance.Toys[toyIndex];

                    list.Clear();

                    frame.Args[sectionIndex].SplitByLengthUtf8(MirrorMethods.MaxStringLength, list);

                    toy.Format = string.Empty;

                    for (var i = 0; i < list.Count; i++)
                        toy.Format += string.Concat("{", i, "}");

                    toy.Arguments.Clear();
                    toy.Arguments.AddRange(list);

                    sectionIndex++;
                }
            }
        }

        /// <summary>
        /// Initializes and configures a texture instance based on the specified texture information, preparing it for
        /// rendering or animation.
        /// </summary>
        /// <remarks>This method modifies both the texture information and the instance to reflect the
        /// desired grid, scaling, and animation settings. It should be called before rendering or displaying the
        /// texture instance. If the texture contains multiple frames, animation is automatically started. The method
        /// does not perform any rendering itself.</remarks>
        /// <param name="texture">A reference to the TextureInfo structure containing settings and data for the texture to be spawned. The
        /// structure's properties determine grid layout, scaling, animation, and spacing. Must be properly initialized
        /// before calling this method.</param>
        /// <param name="instance">A reference to the TextureInstance to be configured. The method updates this instance with the calculated
        /// layout, animation settings, and child elements based on the provided texture information.</param>
        public static void SpawnTexture(ref TextureInfo texture, ref TextureInstance instance, bool usePrimitives)
        {
            var dimensions = texture.Settings.GetGridDimensions(texture.Texture.Texture);

            instance.AnimationDelay = texture.Delay;
            instance.IsAnimationLooped = texture.Loop;

            var calculatedSpacing = texture.Settings.Spacing;

            if (texture.Settings.Grid == TextureGrid.Auto)
                calculatedSpacing = ((float)texture.Texture.Texture.width / dimensions.cols * texture.Settings.AutoSpacingMultiplier * texture.Settings.FontSize) 
                    + texture.Settings.AutoSpacingOffset;

            var sectionWidth = (float)texture.Texture.Texture.width / dimensions.cols;
            var sectionHeight = (float)texture.Texture.Texture.height / dimensions.rows;

            if (texture.Settings.ScaleInputImage && texture.Settings.InputImageScale != 1.0f)
            {
                var scaledWidth = Mathf.RoundToInt(texture.Texture.Texture.width * texture.Settings.InputImageScale);
                var scaledHeight = Mathf.RoundToInt(texture.Texture.Texture.height * texture.Settings.InputImageScale);

                sectionWidth = (float)scaledWidth / dimensions.cols;
                sectionHeight = (float)scaledHeight / dimensions.rows;
            }

            var naturalGridWidth = dimensions.cols * sectionWidth * (texture.Settings.Grid == TextureGrid.Auto 
                ? texture.Settings.AutoSpacingMultiplier * texture.Settings.FontSize 
                : texture.Settings.Spacing);

            var naturalGridHeight = dimensions.rows * sectionHeight * (texture.Settings.Grid == TextureGrid.Auto 
                ? texture.Settings.AutoSpacingMultiplier * texture.Settings.FontSize 
                : texture.Settings.Spacing);

            var maxNaturalSize = Mathf.Max(naturalGridWidth, naturalGridHeight);
            var uniformScale = texture.Settings.TargetSize / maxNaturalSize;

            uniformScale = Mathf.Round(uniformScale * 1000f) / 1000f;

            instance.Parent.Rotation = Quaternion.Euler(0f, 0f, 180f);
            instance.Parent.Scale = Vector3.one * uniformScale;

            var actualSpacingX = texture.Settings.Grid == TextureGrid.Auto
                ? Mathf.Round(sectionWidth * texture.Settings.AutoSpacingMultiplier * texture.Settings.FontSize * 1000f) / 1000f
                : calculatedSpacing;

            var actualSpacingY = texture.Settings.Grid == TextureGrid.Auto
                ? Mathf.Round(sectionHeight * texture.Settings.AutoSpacingMultiplier * texture.Settings.FontSize * 1000f) / 1000f
                : calculatedSpacing;

            var toyIndex = 0;
            var sectionIndex = 0;

            var list = new List<string>();

            for (var row = 0; row < dimensions.rows && sectionIndex < texture.Texture.Pixels.Length; row++)
            {
                for (var col = 0; col < dimensions.cols && sectionIndex < texture.Texture.Pixels.Length; col++)
                {
                    if (sectionIndex >= texture.Texture.Args.Length)
                    {
                        ApiLog.Error("TextureUtils", "sectionIndex out of range");
                        break;
                    }

                    toyIndex++;

                    var toy = new TextToy() { Parent = instance.Parent.Identity };

                    var posX = Mathf.Round((col * actualSpacingX + (texture.Settings.Grid == TextureGrid.Auto
                        ? texture.Settings.AutoSpacingMultiplier : 0)) * 1000f) / 1000f;

                    var posY = Mathf.Round((-row * actualSpacingY - (texture.Settings.Grid == TextureGrid.Auto
                        ? texture.Settings.AutoSpacingMultiplier : 0)) * 1000f) / 1000f;

                    list.Clear();

                    texture.Texture.Args[sectionIndex].SplitByLengthUtf8(MirrorMethods.MaxStringLength, list);

                    toy.Position = new Vector3(posX, posY, 0);
                    toy.Rotation = Quaternion.Euler(0, 0, 0);

                    toy.Format = string.Empty;

                    for (var i = 0; i < list.Count; i++)
                        toy.Format += string.Concat("{", i, "}");

                    toy.Arguments.Clear();
                    toy.Arguments.AddRange(list);

                    instance.Toys.Add(toyIndex, toy);

                    sectionIndex++;
                }
            }

            if (texture.Frames.Count > 1)
                instance.StartAnimation();
        }

        /// <summary>
        /// Performs bilinear interpolation between four colors using premultiplied alpha blending.
        /// </summary>
        /// <remarks>This method uses premultiplied alpha to ensure correct blending of semi-transparent
        /// colors. Colors with an alpha value less than or equal to 0.01 are treated as fully transparent and are
        /// excluded from the interpolation. If only one input color is non-transparent, that color is returned
        /// directly.</remarks>
        /// <param name="c00">The color at the top-left corner of the interpolation grid.</param>
        /// <param name="c01">The color at the top-right corner of the interpolation grid.</param>
        /// <param name="c10">The color at the bottom-left corner of the interpolation grid.</param>
        /// <param name="c11">The color at the bottom-right corner of the interpolation grid.</param>
        /// <param name="tx">The horizontal interpolation factor, typically in the range [0, 1], where 0 selects the left edge and 1
        /// selects the right edge.</param>
        /// <param name="ty">The vertical interpolation factor, typically in the range [0, 1], where 0 selects the top edge and 1 selects
        /// the bottom edge.</param>
        /// <returns>A color resulting from bilinear interpolation of the input colors using premultiplied alpha. If all input
        /// colors are fully transparent, returns a fully transparent color.</returns>
        public static Color PremultipliedAlphaLerp(Color c00, Color c01, Color c10, Color c11, float tx, float ty)
        {
            var validColors = new List<Color>();

            if (c00.a > 0.01f) validColors.Add(c00);
            if (c01.a > 0.01f) validColors.Add(c01);
            if (c10.a > 0.01f) validColors.Add(c10);
            if (c11.a > 0.01f) validColors.Add(c11);

            if (validColors.Count == 0)
                return Color.clear;

            if (validColors.Count == 1)
                return validColors[0];

            var pc00 = c00.a > 0.01f ? PremultiplyAlpha(c00) : Color.clear;
            var pc01 = c01.a > 0.01f ? PremultiplyAlpha(c01) : Color.clear;
            var pc10 = c10.a > 0.01f ? PremultiplyAlpha(c10) : Color.clear;
            var pc11 = c11.a > 0.01f ? PremultiplyAlpha(c11) : Color.clear;

            var pc0 = Color.Lerp(pc00, pc01, ty);
            var pc1 = Color.Lerp(pc10, pc11, ty);
            var premultResult = Color.Lerp(pc0, pc1, tx);

            return UnpremultiplyAlpha(premultResult);
        }

        /// <summary>
        /// Returns a new color with its RGB components multiplied by its alpha value, resulting in a premultiplied
        /// alpha color.
        /// </summary>
        /// <remarks>Premultiplied alpha is commonly used in graphics rendering to improve blending
        /// performance and quality. The returned color's RGB values are each multiplied by the alpha component, while
        /// the alpha value remains unchanged.</remarks>
        /// <param name="color">The color to convert to premultiplied alpha format.</param>
        /// <returns>A new Color instance with premultiplied RGB components and the original alpha value.</returns>
        public static Color PremultiplyAlpha(Color color)
        {
            return new Color(
                color.r * color.a,
                color.g * color.a,
                color.b * color.a,
                color.a);
        }

        /// <summary>
        /// Converts a color with premultiplied alpha to its non-premultiplied (straight alpha) representation.
        /// </summary>
        /// <remarks>Use this method when you need to recover the original color values from a
        /// premultiplied alpha color, such as when compositing or performing color corrections. The resulting color's
        /// RGB components are clamped to the [0, 1] range.</remarks>
        /// <param name="premultColor">The color value with premultiplied alpha to convert. The RGB components are assumed to have already been
        /// multiplied by the alpha component.</param>
        /// <returns>A new Color instance representing the non-premultiplied version of the input color. If the alpha component
        /// is less than or equal to 0.001, returns a fully transparent black color.</returns>
        public static Color UnpremultiplyAlpha(Color premultColor)
        {
            if (premultColor.a <= 0.001f)
                return new Color(0, 0, 0, 0);

            return new Color(
                Mathf.Clamp01(premultColor.r / premultColor.a),
                Mathf.Clamp01(premultColor.g / premultColor.a),
                Mathf.Clamp01(premultColor.b / premultColor.a),

                premultColor.a);
        }

        /// <summary>
        /// Adjusts the brightness, contrast, and gamma of the specified color, with an option to disable alpha
        /// transparency.
        /// </summary>
        /// <remarks>The red, green, and blue channels of the resulting color are clamped to the range [0,
        /// 1] after adjustment. This method does not modify the original color instance.</remarks>
        /// <param name="originalColor">The original color to be adjusted.</param>
        /// <param name="brightness">The factor by which to scale the color's brightness. A value of 1.0 leaves brightness unchanged; values
        /// greater than 1.0 increase brightness, and values less than 1.0 decrease it.</param>
        /// <param name="contrast">The factor by which to adjust the color's contrast. A value of 1.0 leaves contrast unchanged; values greater
        /// than 1.0 increase contrast, and values less than 1.0 decrease it.</param>
        /// <param name="gamma">The gamma correction factor to apply to the color. A value of 1.0 leaves gamma unchanged; values greater
        /// than 1.0 apply stronger gamma correction.</param>
        /// <param name="disableAlpha">true to set the alpha channel to fully opaque (1.0); otherwise, false to preserve the original alpha value.</param>
        /// <returns>A new Color instance with adjusted brightness, contrast, and gamma. The alpha channel is set to 1.0 if
        /// disableAlpha is true; otherwise, it matches the original color's alpha.</returns>
        public static Color AdjustColor(Color originalColor, float brightness, float contrast, float gamma, bool disableAlpha)
        {
            var adjustedColor = originalColor;

            adjustedColor.r *= brightness;
            adjustedColor.g *= brightness;
            adjustedColor.b *= brightness;

            if (contrast != 1.0f)
            {
                adjustedColor.r = ((adjustedColor.r - 0.5f) * contrast) + 0.5f;
                adjustedColor.g = ((adjustedColor.g - 0.5f) * contrast) + 0.5f;
                adjustedColor.b = ((adjustedColor.b - 0.5f) * contrast) + 0.5f;
            }

            if (gamma != 1.0f)
            {
                adjustedColor.r = Mathf.Pow(adjustedColor.r, gamma);
                adjustedColor.g = Mathf.Pow(adjustedColor.g, gamma);
                adjustedColor.b = Mathf.Pow(adjustedColor.b, gamma);
            }

            adjustedColor.r = Mathf.Clamp01(adjustedColor.r);
            adjustedColor.g = Mathf.Clamp01(adjustedColor.g);
            adjustedColor.b = Mathf.Clamp01(adjustedColor.b);

            adjustedColor.a = disableAlpha ? 1.0f : originalColor.a;
            return adjustedColor;
        }

        /// <summary>
        /// Converts the specified color to its hexadecimal RGB string representation.
        /// </summary>
        /// <remarks>The returned string does not include a leading '#' character. The alpha component of
        /// the color is not included in the output.</remarks>
        /// <param name="color">The color to convert to a hexadecimal string. The color's red, green, and blue components are used; the
        /// alpha component is ignored.</param>
        /// <returns>A six-character string representing the color in hexadecimal RGB format (RRGGBB).</returns>
        public static string ColorToHex(Color color)
        {
            var r = Mathf.RoundToInt(color.r * 255);
            var g = Mathf.RoundToInt(color.g * 255);
            var b = Mathf.RoundToInt(color.b * 255);

            return $"{r:X2}{g:X2}{b:X2}";
        }

        /// <summary>
        /// Determines whether two colors are equal within a small tolerance for each color channel.
        /// </summary>
        /// <remarks>This method compares each color channel (red, green, blue, and alpha) using a fixed
        /// tolerance to account for minor floating-point differences. Use this method when exact equality is not
        /// required due to potential floating-point imprecision.</remarks>
        /// <param name="a">The first color to compare.</param>
        /// <param name="b">The second color to compare.</param>
        /// <returns>true if the red, green, blue, and alpha components of both colors differ by less than the allowed tolerance;
        /// otherwise, false.</returns>
        public static bool ColorsAreEqual(Color a, Color b)
        {
            const float tolerance = 0.01f;

            return Mathf.Abs(a.r - b.r) < tolerance &&
                   Mathf.Abs(a.g - b.g) < tolerance &&
                   Mathf.Abs(a.b - b.b) < tolerance &&
                   Mathf.Abs(a.a - b.a) < tolerance;
        }

    }
}