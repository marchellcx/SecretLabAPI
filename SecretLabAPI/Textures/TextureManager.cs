using LabApi.Loader.Features.Paths;

using LabExtended.API.Toys;

using LabExtended.Core;

using LabExtended.Events;
using LabExtended.Extensions;
using LabExtended.Utilities;
using SecretLabAPI.Utilities;
using System.Collections.Concurrent;
using System.Diagnostics;

using UnityEngine;

using SecretLabAPI.Textures.API;

namespace SecretLabAPI.Textures
{
    /// <summary>
    /// Manages texture loading.
    /// </summary>
    public static class TextureManager
    {
        internal static int id = 0;

        internal static volatile string gifPath;
        internal static volatile string texturesPath;
        internal static volatile string settingsPath;

        internal static volatile string defaultGifSettingsPath;
        internal static volatile string defaultSettingsPath;

        internal static volatile bool isReloading;

        internal static volatile TextureSettings defaultSettings;
        internal static volatile AnimatedTextureSettings defaultAnimatedSettings;

        /// <summary>
        /// Gets the default texture settings.
        /// </summary>
        public static TextureSettings DefaultSettings => defaultSettings;

        /// <summary>
        /// Gets the default animated texture settings.
        /// </summary>
        public static AnimatedTextureSettings DefaultAnimatedSettings => defaultAnimatedSettings;

        /// <summary>
        /// Gets per-texture settings.
        /// </summary>
        public static ConcurrentDictionary<string, TextureSettings> TextureSettings { get; } = new();

        /// <summary>
        /// Gets a list of all loaded textures.
        /// </summary>
        public static Dictionary<string, TextureInfo> LoadedTextures { get; } = new();

        /// <summary>
        /// Gets a list of all spawned textures.
        /// </summary>
        public static Dictionary<int, TextureInstance> SpawnedTextures { get; } = new();

        /// <summary>
        /// Gets called once textures start reloading.
        /// </summary>
        public static event Action? ReloadingTextures;

        /// <summary>
        /// Gets called once textures finish reloading.
        /// </summary>
        public static event Action? ReloadedTextures;

        /// <summary>
        /// Whether or not textures are currently being reloaded.
        /// </summary>
        public static bool IsReloading => isReloading;

        /// <summary>
        /// Attempts to find a texture.
        /// </summary>
        /// <param name="name">The name of the texture (not case sensitive).</param>
        /// <param name="texture">The found texture.</param>
        /// <returns>true if the texture was found</returns>
        public static bool TryGetTexture(string name, out TextureInfo texture)
            => LoadedTextures.TryGetValue(name.ToLowerInvariant(), out texture);

        /// <summary>
        /// Attempts to find a texture instance.
        /// </summary>
        /// <param name="id">The ID of the instance.</param>
        /// <param name="instance">The instance that was found.</param>
        /// <returns>true if the instance was found</returns>
        public static bool TryGetInstance(int id, out TextureInstance instance)
            => SpawnedTextures.TryGetValue(id, out instance);

        /// <summary>
        /// Attempts to destroy a texture instance.
        /// </summary>
        /// <param name="id">The ID of the instance.</param>
        /// <returns>true if the instance was destroyed</returns>
        public static bool TryDestroyInstance(int id)
        {
            if (!TryGetInstance(id, out var instance))
                return false;

            instance.Destroy();
            return true;
        }
        
        /// <summary>
        /// Gets a texture.
        /// </summary>
        /// <param name="name">The name of the texture.</param>
        /// <returns>the texture that was found</returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public static TextureInfo GetTexture(string name)
        {
            if (LoadedTextures.TryGetValue(name.ToLowerInvariant(), out var texture))
                return texture;

            throw new KeyNotFoundException($"Texture {name} not found in loaded textures.");
        }

        /// <summary>
        /// Gets a texture instance.
        /// </summary>
        /// <param name="id">The ID of the instance.</param>
        /// <returns>the texture instance that was found</returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public static TextureInstance GetInstance(int id)
        {
            if (SpawnedTextures.TryGetValue(id, out var instance))
                return instance;

            throw new KeyNotFoundException($"Texture instance {id} not found in spawned textures.");
        }

        /// <summary>
        /// Attempts to spawn a texture by name.
        /// </summary>
        /// <param name="name">The name of the texture.</param>
        /// <param name="instance">The instance that was spawned.</param>
        /// <returns>true if an instance was spawned</returns>
        public static bool TrySpawnTexture(string name, bool usePrimitives, out TextureInstance instance)
        {
            try
            {
                instance = default!;

                if (!LoadedTextures.TryGetValue(name.ToLowerInvariant(), out var texture)
                    && !LoadedTextures.TryGetValue(name, out texture))
                    return false;

                instance = SpawnTexture(texture, usePrimitives);
                return true;
            }
            catch (Exception ex)
            {
                ApiLog.Error("TextureManager", $"Failed to spawn texture &3{name}&r:\n&3{ex}&r");

                instance = default!;
                return false;
            }
        }

        /// <summary>
        /// Spawns a texture instance.
        /// </summary>
        /// <param name="texture">The texture to spawn.</param>
        /// <returns>The spawned texture instance.</returns>
        public static TextureInstance SpawnTexture(TextureInfo texture, bool usePrimitives = false)
        {
            var id = TextureManager.id++;
            var parent = new PrimitiveToy(null, null, PrimitiveType.Sphere, AdminToys.PrimitiveFlags.Visible);
            var instance = new TextureInstance();

            instance.Id = id;
            instance.Toys = new();
            instance.Parent = parent;
            instance.Texture = texture;

            TextureUtils.SpawnTexture(ref texture, ref instance, usePrimitives);

            SpawnedTextures[id] = instance;
            return instance;
        }

        /// <summary>
        /// Destroys all spawned instances.
        /// </summary>
        public static void DestroyInstances()
        {
            if (SpawnedTextures.Count > 0)
            {
                foreach (var pair in SpawnedTextures.ToDictionary())
                    pair.Value.Destroy();

                SpawnedTextures.Clear();
            }
        }

        /// <summary>
        /// Reloads all textures.
        /// </summary>
        public static void ReloadTextures()
        {
            if (isReloading)
                throw new Exception($"Textures are currently being reloaded!");

            ApiLog.Info("TextureManager", "Reloading textures ..");

            DestroyInstances();

            LoadedTextures.Clear();

            isReloading = true;

            ReloadingTextures?.InvokeSafe();

            var time = Stopwatch.StartNew();

            if (!Directory.Exists(texturesPath))
            {
                ApiLog.Info("TextureManager", "Textures directory does not exist, creating ..");

                Directory.CreateDirectory(texturesPath);
            }

            if (!Directory.Exists(gifPath))
            {
                ApiLog.Info("TextureManager", "GIF directory does not exist, creating ..");

                Directory.CreateDirectory(gifPath);
            }

            var dict = new ConcurrentDictionary<string, TextureInfo>();
            var gifsDict = new ConcurrentDictionary<string, TextureInfo>();

            var loadAnimated = SecretLab.Config.LoadAnimatedTextures;

            Task.Run(async () =>
            {
                await TextureLoader.LoadStaticTexturesAsync(texturesPath, null, dict);

                foreach (var subDirectory in Directory.GetDirectories(texturesPath))
                {
                    if (subDirectory != gifPath)
                    {
                        await TextureLoader.LoadStaticTexturesAsync(subDirectory, Path.GetFileName(subDirectory), dict);
                    }
                    else
                    {
                        if (loadAnimated)
                        {
                            foreach (var gifDirectory in Directory.GetDirectories(subDirectory))
                            {
                                var texture = await TextureLoader.LoadAnimatedTextureAsync(gifDirectory);

                                if (texture is not null)
                                {
                                    gifsDict.TryAdd(gifDirectory, texture);
                                }
                                else
                                {
                                    ApiLog.Error("TextureManager", $"GIF texture from &6{gifDirectory}&r could not be loaded");
                                }
                            }
                        }
                    }
                }
            }).ContinueWithOnMain(_ =>
            {
                foreach (var pair in dict)
                {
                    if (pair.Value == null)
                    {
                        ApiLog.Error("TextureManager", $"Null static texture: {pair.Key}");
                        continue;
                    }

                    pair.Value.frames.Clear();

                    while (pair.Value.loadedFrames.TryTake(out var loadedFrame))
                        pair.Value.frames.Add(loadedFrame);

                    var name = Path.GetFileName(pair.Key).ToLowerInvariant();
                    var nameNoExtension = Path.GetFileNameWithoutExtension(pair.Key).ToLowerInvariant();

                    var dirPath = Path.GetDirectoryName(pair.Key);
                    var dirName = Path.GetFileName(dirPath);

                    if (dirPath != texturesPath)
                    {
                        name = string.Concat(dirName, "/", name).ToLowerInvariant();
                        nameNoExtension = string.Concat(dirName, "/", nameNoExtension).ToLowerInvariant();
                    }

                    LoadedTextures[name] = pair.Value;
                    LoadedTextures[nameNoExtension] = pair.Value;
                }

                foreach (var pair in gifsDict)
                {
                    if (pair.Value == null)
                    {
                        ApiLog.Error("TextureManager", $"Null GIF texture: {pair.Key}");
                        continue;
                    }

                    var name = Path.GetFileName(pair.Key);

                    pair.Value.frames.Clear();

                    while (pair.Value.loadedFrames.TryTake(out var loadedFrame))
                        pair.Value.frames.Add(loadedFrame);

                    pair.Value.frames.OrderBy(x => x.Index);

                    LoadedTextures[name] = pair.Value;
                }

                dict.Clear();
                gifsDict.Clear();

                time.Stop();

                isReloading = false;

                ReloadedTextures?.InvokeSafe();

                ApiLog.Info("TextureManager", $"Loaded &3{LoadedTextures.Count}&r textures from &3{texturesPath}&r in &6{Mathf.CeilToInt((float)time.Elapsed.TotalSeconds)}s&r");
            });
        }

        /// <summary>
        /// Reloads all texture settings.
        /// </summary>
        public static void ReloadSettings()
        {
            if (!Directory.Exists(settingsPath))
            {
                ApiLog.Info("TextureManager", "Texture settings directory does not exist, creating ..");

                Directory.CreateDirectory(settingsPath);
            }

            defaultSettings = JsonFile.ReadFile<TextureSettings>(defaultSettingsPath, new());
            defaultAnimatedSettings = JsonFile.ReadFile<AnimatedTextureSettings>(defaultGifSettingsPath, new());

            TextureSettings.Clear();

            foreach (var file in Directory.GetFiles(settingsPath, "*.json"))
            {
                var name = Path.GetFileNameWithoutExtension(file).ToLowerInvariant();

                if (TextureSettings.ContainsKey(name))
                    name = Path.GetFileName(file).ToLowerInvariant();

                if (TextureSettings.ContainsKey(name))
                    continue;

                TextureSettings.TryAdd(name, JsonFile.ReadFile(file, DefaultSettings));
            }
        }

        internal static void Initialize()
        {
            texturesPath = Path.GetFullPath(Path.Combine(PathManager.SecretLab.FullName, "textures"));
            settingsPath = Path.GetFullPath(Path.Combine(PathManager.SecretLab.FullName, "texture_settings"));

            gifPath = Path.GetFullPath(Path.Combine(texturesPath, "gifs"));

            defaultSettingsPath = Path.GetFullPath(Path.Combine(settingsPath, "default_settings.json"));
            defaultGifSettingsPath = Path.GetFullPath(Path.Combine(settingsPath, "default_animated.json"));

            ReloadSettings();
            ReloadTextures();

            ExRoundEvents.Restarting += DestroyInstances;
        }
    }
}