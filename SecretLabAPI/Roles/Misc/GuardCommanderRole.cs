using LabExtended.API;
using LabExtended.API.Custom.Roles;

using LabExtended.Utilities;

using PlayerRoles;

using SecretLabAPI.Utilities;
using SecretLabAPI.Elements.Alerts;

using System.ComponentModel;

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
        public List<SpawnRange> Conditions { get; set; } = new()
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

        /// <inheritdoc/>
        public override void OnRegistered()
        {
            base.OnRegistered();
            this.RegisterForSpawning(Conditions, p => p.Role.Type is RoleTypeId.FacilityGuard);
        }

        public override void OnSpawned(ExPlayer player, ref object? data)
        {
            base.OnSpawned(player, ref data);

            player.SendAlert(AlertType.Info, 10f, "Custom Role", "<b>Tvoje role je</b>\n<size=30><color=blue><b>VELITEL HLÍDAČŮ</b></color></size>!");
        }

        internal static void Initialize()
        {
            Role = FileUtils.LoadYamlFileOrDefault(FileUtils.CreatePath(SecretLab.RootDirectory, "roles", "guard_commander.yml"), new GuardCommanderRole(), true);
            Role.Register();
        }
    }
}