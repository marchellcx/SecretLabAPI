using SecretLabAPI.Misc;
using SecretLabAPI.Actions;
using SecretLabAPI.Textures;
using SecretLabAPI.Utilities;
using SecretLabAPI.RandomPickup;

using SecretLabAPI.Elements;
using SecretLabAPI.Elements.Alerts;
using SecretLabAPI.Elements.Levels;

using SecretLabAPI.Audio.Playback;
using SecretLabAPI.Audio.Clips;

using SecretLabAPI.Items.Weapons;
using SecretLabAPI.Items.Weapons.ItemLauncher;

using SecretLabAPI.Patches.Overlays;

using LabApi.Loader;
using LabApi.Loader.Features.Yaml;
using LabApi.Loader.Features.Plugins;

using LabExtended.Core;

using LabExtended.API.Hints;
using LabExtended.Attributes;

using SecretLabAPI.Levels;
using SecretLabAPI.RandomEvents;
using SecretLabAPI.Roles.Misc;
using SecretLabAPI.Roles.ChaosSpy;
using SecretLabAPI.Rays;

namespace SecretLabAPI;

/// <summary>
/// The main class of this library.
/// </summary>
[LoaderPatch]
public class SecretLab : Plugin<SecretLabConfig>
{
    /// <summary>
    /// Gets an instance of this plugin.
    /// </summary>
    public static SecretLab Plugin { get; private set; }

    /// <summary>
    /// Gets an instance of the configuration of this plugin.
    /// </summary>
    public static new SecretLabConfig Config { get; private set; }

    public static string RootDirectory { get; private set; }

    /// <inheritdoc/>
    public override string Name { get; } = "SecretLabAPI";

    /// <inheritdoc/>
    public override string Author { get; } = "mcxsharp";

    /// <inheritdoc/>
    public override string Description { get; } = "A plugin that contains many utilities and functions.";

    /// <inheritdoc/>
    public override Version Version { get; } = new(1, 0, 0);

    /// <inheritdoc/>
    public override Version RequiredApiVersion { get; } = null!;

    /// <inheritdoc/>
    public override void Enable()
    {
        Config = base.Config!;
        Plugin = this;

        RootDirectory = Plugin.GetConfigDirectory(true).FullName;

        // New init
        
        LabEvents.Initialize();

        TextureManager.Initialize();

        PlaybackUtils.Initialize();
        PlayerClips.Initialize();

        ActionManager.Initialize();
        WeightMultipliers.Initialize();

        LevelManager.Initialize();

        ChaosSpyRole.Initialize();
        JanitorRole.Initialize();
        GuardCommanderRole.Initialize();

        SniperRifle.Initialize();
        AirsoftGun.Initialize();
        ItemLauncher.Initialize();

        RandomPickupManager.Initialize();

        AlertElement.Initialize();
        LevelHandler.Initialize();

        InitCustomOverlays();

        RayManager.Initialize();
        RandomEventManager.Initialize();

        // Old init

        SnakeExplosion.Internal_Init();
        PlayerInfoHealth.Internal_Init();
        PersistentOverwatch.Internal_Init();
    }

    /// <inheritdoc/>
    public override void Disable()
    {

    }

    /// <summary>
    /// Saves the specified configuration object to a file in either JSON or YAML format.
    /// </summary>
    /// <remarks>The configuration file is saved to the plugin's configuration directory. If an error occurs
    /// during saving, the failure is logged but no exception is thrown.</remarks>
    /// <param name="json">A value indicating whether to save the configuration as JSON. If <see langword="true"/>, the file is saved in
    /// JSON format; otherwise, it is saved in YAML format.</param>
    /// <param name="configName">The name of the configuration file to create, without extension. The appropriate file extension is added based
    /// on the format.</param>
    /// <param name="config">The configuration object to be saved. This object is serialized to the selected format.</param>
    public static void SaveConfigPath(bool json, string path, object config)
    {
        try
        {
            if (json)
            {
                if (!path.EndsWith(".json"))
                    path += ".json";

                JsonFile.WriteFile(path, config);
            }
            else
            {
                if (!path.EndsWith(".yml"))
                    path += ".yml";

                File.WriteAllText(path, YamlConfigParser.Serializer.Serialize(config));
            }
        }
        catch (Exception ex)
        {
            ApiLog.Error("SecretLabAPI", $"Failed to save config file '{path}':\n{ex}");
        }
    }

    /// <summary>
    /// Saves the specified configuration object to a file in either JSON or YAML format.
    /// </summary>
    /// <remarks>The configuration file is saved to the plugin's configuration directory. If an error occurs
    /// during saving, the failure is logged but no exception is thrown.</remarks>
    /// <param name="json">A value indicating whether to save the configuration as JSON. If <see langword="true"/>, the file is saved in
    /// JSON format; otherwise, it is saved in YAML format.</param>
    /// <param name="configName">The name of the configuration file to create, without extension. The appropriate file extension is added based
    /// on the format.</param>
    /// <param name="config">The configuration object to be saved. This object is serialized to the selected format.</param>
    public static void SaveConfig(bool json, string configName, object config)
    {
        var path = Path.Combine(RootDirectory, configName + (json
            ? ".json"
            : ".yml"));

        try
        {
            if (json)
            {
                JsonFile.WriteFile(path, config);
            }
            else
            {
                File.WriteAllText(path, YamlConfigParser.Serializer.Serialize(config));
            }
        }
        catch (Exception ex)
        {
            ApiLog.Error("SecretLabAPI", $"Failed to save config file '{configName}':\n{ex}");
        }
    }

    /// <summary>
    /// Loads a configuration object of the specified type from a file in the application's root directory, using either
    /// JSON or YAML format.
    /// </summary>
    /// <remarks>The method searches for the configuration file in the application's root directory. If the
    /// file is missing or invalid, the defaultFactory is invoked to supply a fallback configuration.</remarks>
    /// <typeparam name="T">The type of the configuration object to load.</typeparam>
    /// <param name="json">true to load the configuration from a JSON file; false to load from a YAML file.</param>
    /// <param name="configName">The base name of the configuration file, without extension. The appropriate file extension is appended based on
    /// the format.</param>
    /// <param name="defaultFactory">A function that provides a default instance of the configuration object if the file does not exist or cannot be
    /// loaded.</param>
    /// <returns>An instance of the configuration object loaded from the specified file, or the default instance provided by
    /// defaultFactory if the file is not found or cannot be loaded.</returns>
    public static T LoadConfig<T>(bool json, string configName, Func<T> defaultFactory)
    {
        var path = Path.Combine(RootDirectory, configName);
        return LoadConfigPath(json, path, defaultFactory);
    }

    /// <summary>
    /// Loads a configuration file in either JSON or YAML format, returning its contents as an object of type T. If the
    /// file does not exist or cannot be loaded, a default configuration is created and returned.
    /// </summary>
    /// <remarks>If the configuration file is missing, a new file is created with the default configuration.
    /// If a YAML file cannot be deserialized, the original file is renamed with a '.yml.error' extension and replaced
    /// with a default configuration. The method does not throw exceptions for file read or parse errors; instead, it
    /// logs errors and returns a default configuration.</remarks>
    /// <typeparam name="T">The type of the configuration object to load or create.</typeparam>
    /// <param name="json">Indicates whether to load the configuration file in JSON format. If <see langword="true"/>, the file is expected
    /// to be JSON; otherwise, YAML format is used.</param>
    /// <param name="configName">The base name of the configuration file, without extension. The appropriate file extension is appended based on
    /// the format.</param>
    /// <param name="defaultFactory">A function that creates a default configuration object of type T if the file does not exist or cannot be loaded.</param>
    /// <returns>An object of type T containing the loaded configuration. If the file is missing or invalid, a default
    /// configuration is returned and written to disk.</returns>
    public static T LoadConfigPath<T>(bool json, string path, Func<T> defaultFactory)
    {
        ApiLog.Debug("SecretLabAPI", $"Loading config file &3{Path.GetFileName(path)}&r (&6{typeof(T).Name}&r)");

        if (json)
        {
            if (!path.EndsWith(".json"))
                path += ".json";

            return JsonFile.ReadFile(path, defaultFactory());
        }
        else
        {
            if (!path.EndsWith(".yml"))
                path += ".yml";

            if (File.Exists(path))
            {
                try
                {
                    return YamlConfigParser.Deserializer.Deserialize<T>(File.ReadAllText(path));
                }
                catch (Exception ex)
                {
                    ApiLog.Error("SecretLabAPI", $"Failed to load config file '{path}':\n{ex}");

                    var defaultConfig = defaultFactory();
                    var errorPath = Path.Combine(Path.GetDirectoryName(path), Path.GetFileName(path) + ".error");

                    try { File.Delete(errorPath); } catch { }
                    try { File.Move(path, errorPath); } catch { }

                    try
                    {
                        File.WriteAllText(path, YamlConfigParser.Serializer.Serialize(defaultConfig));
                    }
                    catch (Exception subEx)
                    {
                        ApiLog.Error("SecretLabAPI", $"Failed to save default config file '{path}':\n{subEx}");
                    }

                    return defaultConfig;
                }
            }
            else
            {
                var defaultConfig = defaultFactory();

                try
                {
                    File.WriteAllText(path, YamlConfigParser.Serializer.Serialize(defaultConfig));
                }
                catch (Exception ex)
                {
                    ApiLog.Error("SecretLabAPI", $"Failed to save default config file '{path}':\n{ex}");
                }

                return defaultConfig;
            }
        }
    }

    private static void InitCustomOverlays()
    {
        foreach (var pair in Config.StaticOverlays)
        {
            if (pair.Key == "ServerName")
            {
                BasicOverlaysServerNameOverridePatch.ServerNameOverlay = pair.Value;
            }
            else
            {
                HintController.AddHintElement(new StringOverlay(pair.Value) { CustomId = pair.Key });
            }
        }
    }
}