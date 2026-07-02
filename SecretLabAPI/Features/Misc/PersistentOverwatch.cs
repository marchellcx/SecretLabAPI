using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;

using LabExtended.API;
using LabExtended.Events;

using LabExtended.Core.Storage;

using NiveraAPI.IO.Configs;

using PlayerRoles;

namespace SecretLabAPI.Features.Misc
{
    /// <summary>
    /// Keeps the player¨s role as Overwatch between round restarts.
    /// </summary>
    public static class PersistentOverwatch
    {
        /// <summary>
        /// Gets the storage instance used to save Overwatch data.
        /// </summary>
        public static StorageInstance Storage { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether persistent Overwatch is enabled.
        /// </summary>
        [Config("persistentOverwatch", "enabled", "Whether or not to enable persistent Overwatch.")]
        public static bool Enabled { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to use shared storage for Overwatch data.
        /// </summary>
        [Config("persistentOverwatch", "shared", "Whether or not to use shared storage for Overwatch data.")]
        public static bool Shared { get; set; } = true;

        private static void Internal_RoleChanged(PlayerChangedRoleEventArgs args)
        {
            if (!args.Player.RemoteAdminAccess)
                return;

            if (args.NewRole.RoleTypeId != RoleTypeId.Overwatch)
            {
                if (!Storage.TryGet<StorageValue<bool>>(args.Player.UserId, out var overwatchStatus)
                    || !overwatchStatus.Value)
                    return;

                overwatchStatus.Value = false;
            }
            else
            {
                var overwatchStatus = Storage.GetOrAdd(args.Player.UserId, () => new StorageValue<bool>(false));
                
                if (!overwatchStatus.Value)
                    overwatchStatus.Value = true;
            }
        }

        private static void Internal_Verified(ExPlayer player)
        {
            if (!player.RemoteAdminAccess)
                return;

            var overwatchStatus = Storage.GetOrAdd(player.UserId, () => new StorageValue<bool>(false));

            if (!overwatchStatus.Value)
                return;

            player.IsInOverwatch = true;
        }

        private static void Initialize()
        {
            if (!Enabled)
                return;

            Storage = StorageManager.CreateStorage("PersistentOverwatch", Shared);

            ExPlayerEvents.Verified += Internal_Verified;

            PlayerEvents.ChangedRole += Internal_RoleChanged;
        }
    }
}