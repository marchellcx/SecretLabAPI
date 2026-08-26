using LabExtended.API;
using LabExtended.Core;
using LabExtended.Events;

using NiveraAPI.IO.Storage;

using ObscurisCore.Extensions;
using ObscurisCore.Utilities.Storage;

namespace ObscurisCore.Features;

/// <summary>
/// Provides a static collection of alternative user nicknames mapped by unique identifiers.
/// </summary>
public static class AlternativeNicks
{
    /// <summary>
    /// Represents the primary storage container used to manage and persist alternative user nicknames.
    /// </summary>
    [Storage("alternative-nicks", true, typeof(ByteReaderWriterSerializer<string>))]
    public static StorageDirectory Storage;

    /// <summary>
    /// Sets the alternative nickname for the specified user identifier.
    /// </summary>
    /// <param name="userId">The unique identifier of the user whose alternative nickname will be set.</param>
    /// <param name="nick">The alternative nickname to associate with the user.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if the <paramref name="userId"/> or <paramref name="nick"/> is null or an empty string.
    /// </exception>
    public static void Set(string userId, string nick)
    {
        if (string.IsNullOrEmpty(userId))
            throw new ArgumentNullException(nameof(userId));

        if (string.IsNullOrEmpty(nick))
            throw new ArgumentNullException(nameof(nick));

        var value = Storage.AddStorageValue(userId, () => nick);
        
        value.Value = nick;

        if (ExPlayer.TryGetByUserId(userId, out var player))
        {
            player.ReferenceHub.nicknameSync.Network_myNickSync = nick;
            player.SendConsoleMessage($"Updated alternative nick to: {nick}");
        }
    }

    /// <summary>
    /// Removes the alternative nickname associated with the specified user identifier.
    /// </summary>
    /// <param name="userId">The unique identifier of the user whose alternative nickname should be removed.</param>
    /// <returns>
    /// True if the alternative nickname was successfully removed; otherwise, false.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if the <paramref name="userId"/> is null or an empty string.
    /// </exception>
    public static bool Remove(string userId)
    {
        if (string.IsNullOrEmpty(userId))
            throw new ArgumentNullException(nameof(userId));

        return Storage.RemoveStorageValue(userId, true);
    }

    private static void OnVerified(ExPlayer player)
    {
        if (Storage.TryGetValue(player.UserId, out string? nick) && !string.IsNullOrEmpty(nick))
        {
            player.ReferenceHub.nicknameSync.Network_myNickSync = nick;
            player.SendConsoleMessage($"Updated alternative nick to: {nick}");
        }
    }

    private static void StorageInit_Storage()
    {
        foreach (var player in ExPlayer.Players)
        {
            if (!player.IsValidPlayer())
                continue;

            if (!Storage.TryGetValue(player.UserId, out string? nick) || string.IsNullOrEmpty(nick))
                continue;

            player.ReferenceHub.nicknameSync.Network_myNickSync = nick;
            player.SendConsoleMessage($"Updated alternative nick to: {nick}");
        }

        ExPlayerEvents.Verified += OnVerified;
        
        ApiLog.Info($"Alternative nicknames initialized ({Storage.ValueCount} saved nicknames).");
    }
}