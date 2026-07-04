using LabApi.Loader;
using LabApi.Loader.Features.Plugins;

using LabExtended.Core;
using LabExtended.Attributes;

using LabExtended.Utilities.Update;

using NiveraAPI;
using NiveraAPI.Logs;
using NiveraAPI.Extensions;
using NiveraAPI.IO.Configs;

using Serialization;

namespace SecretLabAPI;

/// <summary>
/// The main class of this library.
/// </summary>
[LoaderPatch]
public class SecretLab : Plugin
{
    private static ConfigHandler configHandler;
    
    /// <summary>
    /// Gets an instance of this plugin.
    /// </summary>
    public static SecretLab Plugin { get; private set; }

    /// <summary>
    /// Gets the root directory path of SecretLabAPI's global config.
    /// </summary>
    public static string RootDirectory { get; private set; }

    /// <inheritdoc/>
    public override string Name { get; } = "SecretLabAPI";

    /// <inheritdoc/>
    public override string Author { get; } = "marchellcx";

    /// <inheritdoc/>
    public override string Description { get; } = "A plugin that contains many utilities and functions.";

    /// <inheritdoc/>
    public override Version Version { get; } = new(2, 0, 0);

    /// <inheritdoc/>
    public override Version RequiredApiVersion { get; } = null!;

    /// <inheritdoc/>
    public override void Enable()
    {
        try
        {
            Plugin = this;
            RootDirectory = Plugin.GetConfigDirectory(true).FullName;

            LibraryLoader.Initialize();
            
            LogManager.Log += OnLogged;
            LogManager.UseQueue = false;
            
            LogManager.DisabledLogs = null;
            LogManager.DisabledSources.Clear();

            configHandler = new();

            configHandler.Serialize = (type, obj) => YamlParser.Serializer.Serialize(obj);
            
            configHandler.Deserialize = (type, yaml) =>
            {
                ApiLog.Debug("SecretLab", $"Deserializing &1{type.Name}&r:\n{yaml}");
                return YamlParser.Deserializer.Deserialize(yaml, type)!;
            };
            
            configHandler.FilePath = Path.Combine(RootDirectory, "main.ini");

            typeof(SecretLab)
                .Assembly
                .GetTypes()
                .ForEach(configHandler.Register);

            configHandler.Load();
            configHandler.Save();

            InitLoaders();

            PlayerUpdateHelper.OnLateUpdate += LibraryUpdate.Invoke;
        }
        catch (Exception ex)
        {
            ApiLog.Error("SecretLab", $"Failed to enable SecretLabAPI!:\n{ex}");
        }
    }

    /// <inheritdoc/>
    public override void Disable()
    {
        
    }

    private static void InitLoaders()
    {
        foreach (var type in typeof(SecretLab).Assembly.GetTypes())
        {
            foreach (var method in type.GetAllMethods())
            {
                if (!method.IsPrivate || method.ReturnType != typeof(void) || !method.IsStatic
                    || method.Name != "Initialize" || method.GetParameters().Length != 0)
                    continue;

                ApiLog.Debug("SecretLab", $"Initializing &1{type.Name}&r ...");
                
                try
                {
                    method.Invoke(null, null);
                }
                catch (Exception ex)
                {
                    ApiLog.Error("SecretLab", $"Error while initializing &1{type.Name}&r:\n{ex}");
                }
            }
        }
    }
    
    private static void OnLogged(LogMessage msg)
    {
        switch (msg.Level)
        {
            case LogLevel.Debug:
            case LogLevel.Verbose:
                ApiLog.Debug(msg.SourceText, msg.MessageText);
                break;
            
            case LogLevel.Error:
            case LogLevel.Fatal:
                ApiLog.Error(msg.SourceText, msg.MessageText);
                break;
            
            case LogLevel.Warning:
                ApiLog.Warn(msg.SourceText, msg.MessageText);
                break;
        }
    }
}