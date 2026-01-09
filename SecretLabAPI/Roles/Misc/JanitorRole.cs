using LabExtended.API;
using LabExtended.API.Custom.Roles;

using LabExtended.Utilities;
using LabExtended.Extensions;

using PlayerRoles;

using SecretLabAPI.Utilities;
using SecretLabAPI.Elements.Alerts;

using System.ComponentModel;
using SecretLabAPI.Extensions;

namespace SecretLabAPI.Roles.Misc
{
    /// <summary>
    /// The janitor custom role.
    /// </summary>
    public class JanitorRole : CustomRole
    {
        /// <summary>
        /// Gets the singleton of the Janitor role.
        /// </summary>
        public static JanitorRole Role { get; private set; }

        /// <inheritdoc/>
        public override string Id { get; } = "janitor";

        /// <inheritdoc/>
        public override string Name { get; set; } = "Janitor";

        /// <inheritdoc/>
        public override bool ClearInventory { get; set; } = true;

        /// <inheritdoc/>
        public override RoleTypeId Type { get; set; } = RoleTypeId.ClassD;

        /// <inheritdoc/>
        public override List<ItemType> Items { get; set; } = new()
        {
            ItemType.Medkit,
            ItemType.KeycardJanitor
        };

        /// <summary>
        /// Janitor spawn conditions.
        /// </summary>
        [Description("Sets the Janitor spawn conditions.")]
        public List<SpawnRange> Conditions { get; set; } = new()
        {
            new()
            { 
                MinPlayers = 1,
                MaxPlayers = 6,
                OverallChance = 20,
                MaxSpawnCount = 1
            },

            new()
            {
                MinPlayers = 7,
                MaxPlayers = 11,
                OverallChance = 50,
                MaxSpawnCount = 1
            },

            new()
            {
                MinPlayers = 12,
                MaxPlayers = 18,
                OverallChance = 80,
                MaxSpawnCount = 1
            },

            new()
            {
                MinPlayers = 19,
                OverallChance = 100,
                MaxPlayers = -1,
                MaxSpawnCount = 1
            }
        };

        public override void OnRegistered()
        {
            base.OnRegistered();
            this.RegisterForSpawning(Conditions, p => p.Role.Type is RoleTypeId.ClassD);
        }

        public override void OnSpawned(ExPlayer player, ref object? data)
        {
            base.OnSpawned(player, ref data);

            player.RandomSpawnPositionTeleport([Team.Scientists]);
            player.SendAlert(AlertType.Info, 10f, "Custom Role", "<b>Tvoje role je</b>\n<size=30><color=yellow><b>UKLÍZEČ</b></color></size>!");
        }

        internal static void Initialize()
        {
            Role = FileUtils.LoadYamlFileOrDefault(FileUtils.CreatePath(SecretLab.RootDirectory, "roles", "janitor.yml"), new JanitorRole(), true);
            Role.Register();
        }
    }
}