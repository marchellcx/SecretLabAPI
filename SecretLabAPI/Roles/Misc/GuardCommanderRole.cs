using LabExtended.API;
using LabExtended.API.Custom.Roles;

using PlayerRoles;

using SecretLabAPI.Elements.Alerts;
using SecretLabAPI.Utilities.Roles;

using System.ComponentModel;

using YamlDotNet.Serialization;

namespace SecretLabAPI.Roles.Misc
{
    /// <summary>
    /// The guard commander custom role.
    /// </summary>
    public class GuardCommanderRole : CustomRole
    {
        /// <summary>
        /// Gets the singleton of the Guard Commander role.
        /// </summary>
        public static GuardCommanderRole Role { get; private set; }

        /// <inheritdoc/>
        public override string Id { get; } = "guardcommander";

        /// <inheritdoc/>
        public override string Name { get; set; } = "Guard Commander";

        /// <inheritdoc/>
        public override bool ClearInventory { get; set; } = true;

        /// <inheritdoc/>
        public override RoleTypeId Type { get; set; } = RoleTypeId.FacilityGuard;

        /// <inheritdoc/>
        public override List<ItemType> Items { get; set; } = new()
        {
            ItemType.GunE11SR, 
            ItemType.KeycardMTFPrivate,
            ItemType.Medkit, 
            ItemType.Adrenaline,
            ItemType.GrenadeFlash, 
            ItemType.Radio,
            ItemType.ArmorCombat
        };

        /// <inheritdoc/>
        public override Dictionary<ItemType, ushort> Ammo { get; set; } = new()
        {
            { ItemType.Ammo556x45, 120 }
        };

        /// <summary>
        /// Guard Commander spawn conditions.
        /// </summary>
        [Description("Sets the Guard Commander spawn conditions.")]
        public List<RoleRange> Conditions { get; set; } = new()
        {
            new()
            {
                MinPlayers = 3,
                MaxPlayers = 5,
                OverallChance = 20,
                MaxSpawnCount = 1
            },

            new()
            {
                MinPlayers = 6,
                MaxPlayers = 12,
                OverallChance = 40,
                MaxSpawnCount = 1
            },

            new()
            {
                MinPlayers = 13,
                MaxPlayers = 16,
                OverallChance = 60,
                MaxSpawnCount = 1
            },

            new()
            {
                MinPlayers = 17,
                MaxPlayers = 26,
                OverallChance = 80,
                MaxSpawnCount = 1
            },

            new()
            {
                MinPlayers = 27,
                MaxPlayers = -1,
                OverallChance = 100,
                MaxSpawnCount = 1
            }
        };

        /// <summary>
        /// Gets the selector for player spawns.
        /// </summary>
        [YamlIgnore]
        public RoleSelector Selector { get; private set; }

        /// <inheritdoc/>
        public override void OnRegistered()
        {
            Selector = new RoleSelector(Conditions,
                player => Give(player),
                (player, role) => role is RoleTypeId.FacilityGuard);

            base.OnRegistered();
        }

        public override void OnSpawned(ExPlayer player, ref object? data)
        {
            base.OnSpawned(player, ref data);

            player.SendAlert(AlertType.Info, 10f, "Custom Role", "<b>Tvoje role je</b>\n<size=30><color=blue><b>VELITEL HLÍDAČŮ</b></color></size>!");
        }

        internal static void Initialize()
        {
            Role = SecretLab.LoadConfig(false, "guard_commander", () => new GuardCommanderRole());
            Role.Register();
        }
    }
}