using LabApi.Loader;
using LabApi.Loader.Features.Plugins;

using LabExtended.Attributes;
using LabExtended.Core;
using LabExtended.Utilities.Update;

using NiveraAPI;
using NiveraAPI.Logs;
using NiveraAPI.Extensions;
using NiveraAPI.IO.Configs;

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
        Plugin = this;
        RootDirectory = Plugin.GetConfigDirectory(true).FullName;
        
        LogManager.Log += OnLogged;
        LogManager.UseQueue = true;
        
        LibraryLoader.Initialize();

        configHandler = new();
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

    /// <inheritdoc/>
    public override void Disable()
    {
        
    }

    private static void InitLoaders()
    {
        typeof(SecretLab).Assembly
            .InvokeStaticMethods(m => 
                m.IsStatic && 
                m.GetAllParameters().Length == 0 && 
                m.ReturnType == typeof(void) &&
                m.Name == "Initialize");
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