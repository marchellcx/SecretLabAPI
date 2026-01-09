using LabExtended.Core;
using LabExtended.Utilities;

using LabExtended.API.Images;

using System.Collections.Concurrent;

using SecretLabAPI.Textures.API;

namespace SecretLabAPI.Textures
{
    /// <summary>
    /// Used to load textures.
    /// </summary>
    public static class TextureLoader
    {
        /// <summary>
        /// Asynchronously loads an animated texture from the specified directory containing frame image files and
        /// returns a TextureInfo object representing the animation.
        /// </summary>
        /// <remarks>Frame files in the directory must be named using their frame numbers (e.g., '0.png',
        /// '1.png', etc.) to be recognized as part of the animation. The method applies settings from a corresponding
        /// JSON configuration file if present. The order of frames is determined by the numeric value in each file
        /// name.</remarks>
        /// <param name="directoryPath">The path to the directory containing the frame image files for the animated texture. The directory must
        /// exist and contain files named with their frame numbers.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a TextureInfo object with the
        /// loaded animated texture data.</returns>
        /// <exception cref="ArgumentException">Thrown if directoryPath is null or empty.</exception>
        public static async Task<TextureInfo> LoadAnimatedTextureAsync(string directoryPath)
        {
            if (string.IsNullOrEmpty(directoryPath))
                throw new ArgumentException("Directory path cannot be null or empty.", nameof(directoryPath));

            var gifFrameFilesDict = new ConcurrentDictionary<string, int>();
            var gifFrameFiles = new ConcurrentBag<string>();

            var gifInfo = new TextureInfo();
            var gifName = Path.GetFileName(directoryPath);
            var gifSettings = TextureManager.DefaultAnimatedSettings;
            var gifSettingsPath = Path.GetFullPath(Path.Combine(TextureManager.settingsPath, $"gif_{gifName}.json"));

            gifSettings = FileUtils.LoadJsonFileOrDefault(gifSettingsPath, TextureManager.DefaultAnimatedSettings);

            gifInfo.path = directoryPath;
            gifInfo.settings = gifSettings.FrameSettings;
            gifInfo.animatedSettings = gifSettings;

            gifInfo.delay = gifSettings.DelayTime;
            gifInfo.loop = gifSettings.Loop;

            foreach (var frameFile in Directory.GetFiles(directoryPath))
            {
                var frameName = Path.GetFileNameWithoutExtension(frameFile);

                frameName = new string(frameName.Where(x => char.IsNumber(x)).ToArray());

                if (!int.TryParse(frameName, out var frameIndex))
                {
                    ApiLog.Warn("TextureManager", $"GIF frame files must be named after the number of their frame ({frameName} - {frameFile})!");
                    continue;
                }

                gifFrameFilesDict[frameFile] = frameIndex;
            }

            foreach (var key in gifFrameFilesDict.Keys)
                gifFrameFiles.Add(key);

            gifFrameFiles.OrderBy(x => gifFrameFilesDict[x]);

            foreach (var frameFile in gifFrameFiles)
            {
                var frameTexture = await TextureUtils.ExecuteOnMainAsync(() =>
                {
                    if (ImageLoader.TryLoadTexture2D(frameFile, out var frameTexture))
                    {
                        return frameTexture;
                    }
                    else
                    {
                        ApiLog.Error("TextureManager", $"Failed to load texture from file &3{frameFile}&r: Unsupported format");
                        return null!;
                    }
                });

                await TextureParser.ParseAsync(frameTexture, gifSettings.FrameSettings, gifInfo);

                if (gifInfo.loadedFrames.Count > 0)
                    gifInfo.loadedFrames.ElementAt(gifInfo.loadedFrames.Count - 1).Index = gifFrameFilesDict[frameFile];
            }

            gifFrameFilesDict.Clear();
            return gifInfo;
        }

        /// <summary>
        /// Asynchronously loads all static textures from the specified directory and populates the provided dictionary
        /// with their information.
        /// </summary>
        /// <remarks>This method clears the provided dictionary before loading textures. The operation is
        /// performed asynchronously, and the method does not return until all textures in the directory have been
        /// processed and added to the dictionary. The method is thread-safe and suitable for use in concurrent
        /// environments.</remarks>
        /// <param name="directoryPath">The full path to the directory containing the texture files to load. Cannot be null or empty.</param>
        /// <param name="dirName">An optional display name for the directory. Used for logging and identification purposes. Can be null.</param>
        /// <param name="dict">A thread-safe dictionary to be populated with texture information, keyed by file path. The dictionary is
        /// cleared before loading begins. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous load operation. The task completes when all textures have been
        /// loaded and the dictionary has been populated.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="directoryPath"/> is null or empty.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="dict"/> is null.</exception>
        public static async Task LoadStaticTexturesAsync(string directoryPath, string? dirName, ConcurrentDictionary<string, TextureInfo> dict)
        {
            if (string.IsNullOrEmpty(directoryPath))
                throw new ArgumentException("Directory path cannot be null or empty.", nameof(directoryPath));

            if (dict is null)
                throw new ArgumentNullException(nameof(dict), "The texture dictionary cannot be null.");

            dict.Clear();

            var fileList = new ConcurrentBag<string>();

            foreach (var file in Directory.GetFiles(directoryPath))
            {
                fileList.Add(file);

                Task.Run(async () => await LoadStaticTextureAsync(file, dict));
            }

            while (true)
            {
                var anyMissing = false;

                foreach (var file in fileList)
                {
                    if (!dict.ContainsKey(file))
                    {
                        anyMissing = true;
                        break;
                    }
                }

                if (!anyMissing)
                    break;

                await Task.Delay(100);
            }
        }

        private static async Task LoadStaticTextureAsync(string filePath, ConcurrentDictionary<string, TextureInfo> dict)
        {
            var name = Path.GetFileNameWithoutExtension(filePath).ToLowerInvariant();
            var nameExtension = Path.GetFileName(filePath).ToLowerInvariant();

            var settings = TextureManager.DefaultSettings;

            if (TextureManager.TextureSettings.TryGetValue(name, out var textureSettings)
                || TextureManager.TextureSettings.TryGetValue(nameExtension, out textureSettings))
                settings = textureSettings;

            var texture = await TextureUtils.ExecuteOnMainAsync(() =>
            {
                if (ImageLoader.TryLoadTexture2D(filePath, out var texture))
                    return texture;

                ApiLog.Error("TextureManager", $"Failed to load texture from file &3{filePath}&r: Unsupported format");
                return null!;
            });

            if (texture is null)
            {
                dict.TryAdd(filePath, null!);
                return;
            }

            var textureInfo = new TextureInfo();

            textureInfo.path = filePath;
            textureInfo.settings = settings;

            await TextureParser.ParseAsync(texture, settings, textureInfo);

            dict.TryAdd(filePath, textureInfo);
        }
    }
}