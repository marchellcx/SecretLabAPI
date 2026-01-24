using LabApi.Loader;
using LabApi.Loader.Features.Plugins;

using LabExtended.API.Hints;
using LabExtended.Attributes;

using SecretLabAPI.Features;
using SecretLabAPI.Utilities;

using SecretLabAPI.Patches.Overlays;

using SecretLabAPI.Features.Rays;
using SecretLabAPI.Features.Items;
using SecretLabAPI.Features.Roles;
using SecretLabAPI.Features.Voting;
using SecretLabAPI.Features.Levels;
using SecretLabAPI.Features.Actions;
using SecretLabAPI.Features.Effects;
using SecretLabAPI.Features.Elements;
using SecretLabAPI.Features.RandomPickup;
using SecretLabAPI.Features.RandomEvents;

using SecretLabAPI.Features.Elements.Alerts;
using SecretLabAPI.Features.Misc.Functions;

using SecretLabAPI.Features.Roles.ChaosSpy;
using SecretLabAPI.Features.Roles.Misc;

using SecretLabAPI.Features.Audio.Clips;
using SecretLabAPI.Features.Audio.Playback;

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

    /// <summary>
    /// Gets the root directory path of SecretLabAPI's global config.
    /// </summary>
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

        RootDirectory = Plugin.GetConfigDirectory(Config.SharedConfigs).FullName;

        // New init
        
        LabEvents.Initialize();

        PlaybackUtils.Initialize();
        PlayerClips.Initialize();

        ActionManager.Initialize();
        LevelManager.Initialize();

        ChaosSpyRole.Initialize();
        JanitorRole.Initialize();
        GuardCommanderRole.Initialize();

        RandomPickupManager.Initialize();
        DeveloperMode.Initialize();
        Scp914Teleport.Initialize();
        VoteManager.Initialize();
        AlternativeNicks.Initialize();

        CustomRoleSpawner.Initialize();
        CustomItemsHandler.Initialize();
        CustomEffectsHandler.Initialize();

        AlertElement.Initialize();

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