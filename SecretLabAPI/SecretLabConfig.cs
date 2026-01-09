using MapGeneration;
using Scp914;
using SecretLabAPI.Elements.Levels;
using SecretLabAPI.Utilities.Configs;

using System.ComponentModel;

namespace SecretLabAPI
{
    /// <summary>
    /// Config for the plugin.
    /// </summary>
    public class SecretLabConfig
    {
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
        /// Whether or not to load animated textures.
        /// </summary>
        [Description("Whether or not to load animated textures.")]
        public bool LoadAnimatedTextures { get; set; } = true;

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
        /// Gets or sets the probability that SCP-914 will teleport players during operation.
        /// </summary>
        [Description("Sets the chance for SCP-914 to teleport players.")]
        public float Scp914TeleportChance { get; set; } = 0f;

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
        /// Gets or sets the maximum number of options that can be selected in a vote.
        /// </summary>
        [Description("Maximum number of options allowed in a vote.")]
        public int VoteMaxOptions { get; set; } = 5;
    }
}