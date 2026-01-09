using LabExtended.API;
using LabExtended.Events;
using LabExtended.Core.Storage;

using LabApi.Events.Handlers;
using LabApi.Events.Arguments.PlayerEvents;

using PlayerRoles;

namespace SecretLabAPI.Misc.Tools
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

        internal static void Internal_Init()
        {
            if (!SecretLab.Config.PersistentOverwatchEnabled)
                return;

            Storage = StorageManager.CreateStorage("PersistentOverwatch", SecretLab.Config.PersistentOverwatchShared);

            ExPlayerEvents.Verified += Internal_Verified;

            PlayerEvents.ChangedRole += Internal_RoleChanged;
        }
    }
}