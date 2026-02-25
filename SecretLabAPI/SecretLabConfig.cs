using MapGeneration;

using Scp914;

using SecretLabAPI.Utilities.Configs;

using System.ComponentModel;
using SecretLabAPI.Features.Audio.Clips;

namespace SecretLabAPI
{
    /// <summary>
    /// Config for the plugin.
    /// </summary>
    public class SecretLabConfig
    {
        /// <summary>
        /// Gets or sets a value indicating whether the application uses shared configuration settings.
        /// </summary>
        [Description("Whether or not to use shared configs.")]
        public bool SharedConfigs { get; set; } = true;

        /// <summary>
        /// Gets or sets whether persistent overwatch is enabled.
        /// </summary>
        [Description("Whether persistent overwatch is enabled.")]
        public bool PersistentOverwatchEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to use shared storage for persistent overwatch.
        /// </summary>
        [Description("Whether or not to use shared storage for persistent overwatch.")]
        public bool PersistentOverwatchShared { get; set; } = true;

        /// <summary>
        /// Gets or sets the collection of static overlay elements and their configuration options.
        /// </summary>
        [Description("Configures a list of static string elements.")]
        public Dictionary<string, OverlayOptions> StaticOverlays { get; set; } = new()
        {
            { "ServerName", new() }
        };

        /// <summary>
        /// Gets or sets the maximum stack sizes for individual inventory item types.
        /// </summary>
        /// <remarks>Each entry specifies the maximum number of items of a given type that can be stacked together
        /// in a single inventory slot. Modifying this dictionary allows customization of stacking behavior for different
        /// item types.</remarks>
        [Description("Enables stacking for individual inventory items and sets their maximum stack size.")]
        public Dictionary<ItemType, ushort> ItemStacks { get; set; } = new()
        {
            { ItemType.Coin, 100 }
        };

        /// <summary>
        /// List of items that should be prevented from spawning on round start.
        /// </summary>
        [Description("List of items that should be prevented from spawning on round start.")]
        public List<ItemType> PreventSpawn { get; set; } = new();

        /// <summary>
        /// List of items that should be spawned at custom positions.
        /// </summary>
        [Description("List of items that should be spawned at custom positions.")]
        public Dictionary<string, List<string>> CustomSpawns { get; set; } = new()
        {
            { "ExamplePosition", new() { "None" } }
        };

        /// <summary>
        /// Gets or sets the number of frames to skip between ray manager updates.
        /// </summary>
        /// <remarks>Increasing this value reduces the frequency of ray manager updates, which may improve
        /// performance at the cost of update responsiveness. Set to 0 to update every frame.</remarks>
        [Description("Number of frames to skip between ray manager updates.")]
        public int RayManagerFrameSkip { get; set; } = 2;

        /// <summary>
        /// Gets or sets the forward offset applied to raycasts performed by the ray manager.
        /// </summary>
        [Description("Forward offset for ray manager raycasts.")]
        public float RayManagerForwardOffset { get; set; } = 0.1f;

        /// <summary>
        /// Gets or sets the maximum distance, in units, used for raycasts performed by the ray manager.
        /// </summary>
        [Description("Maximum distance for ray manager raycasts.")]
        public float RayManagerDistance { get; set; } = 100f;

        /// <summary>
        /// Gets or sets the collection of layer names used by the ray manager for raycasting operations.
        /// </summary>
        [Description("Layers used by the ray manager for raycasting.")]
        public string[] RayManagerLayers { get; set; } = new string[]
        {
            "Default",
            "TransparentFX",
            "Ignore Raycast",
            "Water",
            "UI"
        };

        /// <summary>
        /// Gets or sets the intensity of the Movement Boost effect when a player successfully escapes PD.
        /// </summary>
        [Description("Sets the intensity of the Movement Boost effect when a player succesfully escapes PD.")]
        public byte PocketSpeedBoostIntensity { get; set; } = 0;

        /// <summary>
        /// Gets or sets the duration of the Movement Boost effect when a player successfully escapes PD.
        /// </summary>
        [Description( "Sets the duration of the Movement Boost effect when a player succesfully escapes PD." )]
        public float PocketSpeedBoostDuration { get; set; } = 0f;

        /// <summary>
        /// Gets or sets the probability that SCP-914 will teleport players during operation.
        /// </summary>
        [Description("Sets the chance for SCP-914 to teleport players.")]
        public float Scp914TeleportChance { get; set; } = 0f;
        
        /// <summary>
        /// Gets or sets the chance that a player will be teleported to an SCP during SCP-914 teleportation.    
        /// </summary>
        [Description("Sets the chance of a player being teleported to an SCP during SCP-914 teleportation.")]
        public float Scp914ScpTeleportChance { get; set; } = 0f;

        /// <summary>
        /// Gets or sets the knob setting that enables SCP-914 to teleport players.
        /// </summary>
        [Description("Sets the knob setting which allows SCP-914 to teleport players.")]
        public Scp914KnobSetting Scp914TeleportSetting { get; set; } = Scp914KnobSetting.Coarse;

        /// <summary>
        /// Gets or sets the mapping of facility zones to the minimum round duration, in seconds, required for SCP-914
        /// teleportation to be enabled in each zone.
        /// </summary>
        /// <remarks>Modify this dictionary to control which facility zones are eligible for SCP-914
        /// teleportation based on the elapsed round time. Zones become eligible when the round duration meets or
        /// exceeds the specified value.</remarks>
        [Description("Sets SCP-914 teleport zone whitelists based on round duration (in seconds).")]
        public Dictionary<FacilityZone, float> Scp914TeleportZones { get; set; } = new()
        {
            { FacilityZone.Surface, 300f },
            { FacilityZone.Entrance, 120f },
            { FacilityZone.HeavyContainment, 60f },
            { FacilityZone.LightContainment, 0f },
        };

        /// <summary>
        /// Gets or sets the audio clips used by SCP-914 during teleportation.
        /// </summary>
        [Description("Sets the audio clips used by SCP-914 during teleportation.")]
        public ClipConfig<string> Scp914TeleportClips { get; set; } = new();

        /// <summary>
        /// Gets or sets the maximum number of options that can be selected in a vote.
        /// </summary>
        [Description("Maximum number of options allowed in a vote.")]
        public int VoteMaxOptions { get; set; } = 5;

        /// <summary>
        /// Gets or sets the default whitelist table that is enabled when the plugin is loaded.
        /// </summary>
        [Description("Sets the whitelist table enabled by default.")]
        public string? DefaultWhitelist { get; set; } = null;
    }
}