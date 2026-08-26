using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;

using LabExtended.API;
using LabExtended.Core;
using LabExtended.Events;

using NiveraAPI.IO.Configs;
using NiveraAPI.IO.Storage;

using ObscurisCore.Utilities.Storage;

using PlayerRoles;

namespace ObscurisCore.Features;

/// <summary>
/// Keeps the player¨s role as Overwatch between round restarts.
/// </summary>
public static class PersistentOverwatch
{
    /// <summary>
    /// Represents the storage mechanism for persisting Overwatch-related data
    /// across sessions, allowing retrieval and updates of player-specific states.
    /// </summary>
    [Storage("persistent-overwatch", true, typeof(ByteReaderWriterSerializer<string>))]
    public static StorageDirectory Storage;

    /// <summary>
    /// Gets or sets a value indicating whether persistent Overwatch is enabled.
    /// </summary>
    [Config("persistentOverwatch", "enabled", "Whether or not to enable persistent Overwatch.")]
    public static bool Enabled { get; set; } = true;

    private static void OnRoleChanged(PlayerChangedRoleEventArgs args)
    {
        if (!args.Player.RemoteAdminAccess)
            return;

        if (args.NewRole.RoleTypeId != RoleTypeId.Overwatch)
        {
            if (!Storage.TryGetStorageValue<bool>(args.Player.UserId, out var storageValue)
                || !storageValue.Value)
                return;

            storageValue.Value = false;
        }
        else
        {
            var overwatchStatus = Storage.AddStorageValue(args.Player.UserId, () => true);

            if (!overwatchStatus.Value)
                overwatchStatus.Value = true;
        }
    }

    private static void OnVerified(ExPlayer player)
    {
        if (!player.RemoteAdminAccess)
            return;

        if (!Storage.TryGetStorageValue<bool>(player.UserId, out var storageValue))
            return;

        if (!storageValue.Value)
            return;

        player.IsInOverwatch = true;
    }

    private static void StorageInit_Storage()
    {
        if (!Enabled)
        {
            ApiLog.Warn("Persistent Overwatch is disabled.");
            return;
        }

        ExPlayerEvents.Verified += OnVerified;
        PlayerEvents.ChangedRole += OnRoleChanged;
        
        ApiLog.Info($"Persistent Overwatch initialized ({Storage?.ValueCount ?? -1} saved states).");
    }
}