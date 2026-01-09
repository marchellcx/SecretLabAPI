using LabExtended.API;
using LabExtended.API.Custom.Roles;

using LabExtended.Events;

using NorthwoodLib.Pools;

using SecretLabAPI.Utilities;

namespace SecretLabAPI.Roles
{
    /// <summary>
    /// Provides functionality for registering and managing custom role spawning logic based on specified conditions and
    /// optional player predicates.
    /// </summary>
    public static class CustomRoleSpawner
    {
        /// <summary>
        /// Represents the information required to determine whether a custom role should be spawned, including the
        /// role, spawn conditions, and an optional predicate for additional logic.
        /// </summary>
        /// <remarks>Use this structure to specify custom spawning logic for roles based on defined
        /// conditions and optional player-specific criteria. The predicate, if provided, allows for advanced filtering
        /// beyond the basic spawn ranges.</remarks>
        public struct CustomRoleSpawnInfo
        {
            /// <summary>
            /// Represents the custom role associated with this instance.
            /// </summary>
            public readonly CustomRole Role;

            /// <summary>
            /// Gets the collection of spawn range conditions that determine when spawning is allowed.
            /// </summary>
            public readonly List<SpawnRange> Conditions;

            /// <summary>
            /// Gets the predicate used to determine whether an ExPlayer instance satisfies specific conditions.
            /// </summary>
            public readonly Predicate<ExPlayer>? Predicate;

            /// <summary>
            /// Initializes a new instance of the CustomRoleSpawnInfo class with the specified role, spawn conditions,
            /// and optional predicate.
            /// </summary>
            /// <param name="role">The custom role to associate with this spawn information.</param>
            /// <param name="conditions">A list of spawn ranges that define the conditions under which the role can spawn. Cannot be null.</param>
            /// <param name="predicate">An optional predicate used to further filter eligible players for spawning. If null, no additional
            /// filtering is applied.</param>
            public CustomRoleSpawnInfo(CustomRole role, List<SpawnRange> conditions, Predicate<ExPlayer>? predicate = null)
            {
                Role = role;
                Conditions = conditions;
                Predicate = predicate;
            }
        }

        /// <summary>
        /// Gets the collection of custom role spawn information used by the application.
        /// </summary>
        public static List<CustomRoleSpawnInfo> Roles { get; } = new();

        /// <summary>
        /// Registers a custom role for spawning with the specified spawn conditions and an optional predicate.
        /// </summary>
        /// <param name="role">The custom role to register for spawning. Cannot be null.</param>
        /// <param name="conditions">A list of spawn ranges that define the conditions under which the role can spawn. Cannot be null.</param>
        /// <param name="predicate">An optional predicate that determines whether a player is eligible to spawn as this role. If null, all
        /// players are considered eligible.</param>
        /// <exception cref="ArgumentNullException">Thrown if either the role or conditions parameter is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the specified role is already registered for spawning.</exception>
        public static void RegisterForSpawning(this CustomRole role, List<SpawnRange> conditions, Predicate<ExPlayer>? predicate = null)
        {
            if (role is null)
                throw new ArgumentNullException(nameof(role));

            if (conditions is null)
                throw new ArgumentNullException(nameof(conditions));

            if (Roles.Any(x => x.Role == role))
                throw new ArgumentException($"Role {role.Name} is already registered for spawning.");

            Roles.Add(new CustomRoleSpawnInfo(role, conditions, predicate));
        }

        private static void OnStarted()
        {
            var players = ListPool<ExPlayer>.Shared.Rent(ExPlayer.Players.Where(p => p?.ReferenceHub != null));

            if (players.Count == 0)
            {
                ListPool<ExPlayer>.Shared.Return(players);
                return;
            }

            for (var x = 0; x < Roles.Count; x++)
            {
                if (players.Count == 0)
                    break;

                var info = Roles[x];

                info.Conditions.SetRoles(players, player =>
                {
                    info.Role.Give(player);

                    players.Remove(player);
                }, info.Predicate);
            }
        }

        internal static void Initialize()
        {
            ExRoundEvents.Started += OnStarted;
        }
    }
}