using LabExtended.API;
using LabExtended.API.Custom.Roles;

using LabExtended.Extensions;

using PlayerRoles;

using SecretLabAPI.Elements.Alerts;
using SecretLabAPI.Utilities.Roles;

using System.ComponentModel;
using LabExtended.Utilities;
using YamlDotNet.Serialization;

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
        public List<RoleRange> Conditions { get; set; } = new()
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
                (player, role) => role is RoleTypeId.ClassD);

            base.OnRegistered();
        }

        public override void OnSpawned(ExPlayer player, ref object? data)
        {
            base.OnSpawned(player, ref data);

            player.Position.Position = RoleTypeId.Scientist.GetSpawnPosition().position;
            player.SendAlert(AlertType.Info, 10f, "Custom Role", "<b>Tvoje role je</b>\n<size=30><color=yellow><b>UKLÍZEČ</b></color></size>!");
        }

        internal static void Initialize()
        {
            if (FileUtils.TryLoadYamlFile<JanitorRole>(SecretLab.RootDirectory, "janitor.yml", out var role))
            {
                Role = role;
            }
            else
            {
                FileUtils.TrySaveYamlFile(SecretLab.RootDirectory, "janitor.yml", Role = new());
            }

            Role.Register();
        }
    }
}