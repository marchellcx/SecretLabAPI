using LabApi.Loader;
using LabApi.Loader.Features.Plugins;

using LabExtended.API;
using LabExtended.API.Hints;
using LabExtended.API.Custom.Effects;

using LabExtended.Events;
using LabExtended.Utilities;
using LabExtended.Attributes;

using SecretLabAPI.Rays;
using SecretLabAPI.Misc;
using SecretLabAPI.Levels;
using SecretLabAPI.Effects;
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

        SniperRifle.Initialize();
        AirsoftGun.Initialize();
        ItemLauncher.Initialize();

        RandomPickupManager.Initialize();
        DeveloperMode.Initialize();
        Scp914Teleport.Initialize();
        VoteManager.Initialize();

        AlertElement.Initialize();
        LevelHandler.Initialize();

        InitCustomOverlays();

        RayManager.Initialize();
        RandomEventManager.Initialize();
        
        CustomPlayerEffect.Effects.Add(typeof(RocketEffect));
        CustomPlayerEffect.Effects.Add(typeof(DoorInteractExplosionEffect));

        new ReplicatingScp018().Register();

        ExPlayerEvents.Verified += AddCustomEffects;

        // Old init

        SnakeExplosion.Internal_Init();
        PlayerInfoHealth.Internal_Init();
        PersistentOverwatch.Internal_Init();
    }

    /// <inheritdoc/>
    public override void Disable()
    {

    }

    private static void AddCustomEffects(ExPlayer player)
    {
        var rocketEffect = FileUtils.LoadYamlFileOrDefault(RootDirectory, "rocket_effect.yml", new RocketEffect(), true);
        var doorInteractExplosionEffect = FileUtils.LoadYamlFileOrDefault(RootDirectory, "door_interact_explosion_effect.yml", new DoorInteractExplosionEffect(), true);

        player.Effects.AddCustomEffect(rocketEffect, false);
        player.Effects.AddCustomEffect(doorInteractExplosionEffect, false);
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