using LabApi.Loader;
using LabApi.Loader.Features.Plugins;

using LabExtended.API;
using LabExtended.API.Hints;
using LabExtended.API.Custom.Effects;

using LabExtended.Events;
using LabExtended.Utilities;
using LabExtended.Attributes;

using SecretLabAPI.Rays;
using SecretLabAPI.Levels;
using SecretLabAPI.Actions;
using SecretLabAPI.Textures;
using SecretLabAPI.Utilities;
using SecretLabAPI.RandomEvents;
using SecretLabAPI.RandomPickup;

using SecretLabAPI.Roles.Misc;
using SecretLabAPI.Roles.ChaosSpy;

using SecretLabAPI.Elements;
using SecretLabAPI.Elements.Alerts;
using SecretLabAPI.Elements.Levels;

using SecretLabAPI.Audio.Playback;
using SecretLabAPI.Audio.Clips;

using SecretLabAPI.Items.Weapons;
using SecretLabAPI.Items.Weapons.ItemLauncher;

using SecretLabAPI.Patches.Overlays;
using SecretLabAPI.Voting;
using SecretLabAPI.Roles;
using SecretLabAPI.Effects.Misc;
using SecretLabAPI.Effects;
using SecretLabAPI.Items;
using SecretLabAPI.Misc.Tools;
using SecretLabAPI.Misc.Functions;
using SecretLabAPI.Misc.Grabbing;

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

        RandomPickupManager.Initialize();
        DeveloperMode.Initialize();
        Scp914Teleport.Initialize();
        VoteManager.Initialize();
        GrabHandler.Initialize();

        CustomRoleSpawner.Initialize();
        CustomItemsHandler.Initialize();
        CustomEffectsHandler.Initialize();

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