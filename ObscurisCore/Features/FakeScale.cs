using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabExtended.API;
using LabExtended.Events;
using ObscurisCore.Extensions;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using UnityEngine;

namespace ObscurisCore.Features;

/// <summary>
/// Provides functionality for applying and managing fake scales for players in the game,
/// enabling dynamic changes to player appearances based on scale manipulation.
/// </summary>
public static class FakeScale
{
    /// <summary>
    /// Represents a fake instance used for modifying the scale of players.
    /// </summary>
    public struct FakeInstance
    {
        /// <summary>
        /// The fake scale value.
        /// </summary>
        public readonly Vector3 Scale;

        /// <summary>
        /// Whether or not to reset scale on death.
        /// </summary>
        public readonly bool ResetOnDeath;

        /// <summary>
        /// Whether or not to reset scale on role change.
        /// </summary>
        public readonly bool ResetOnRoleChange;

        /// <summary>
        /// Condition used to check which players to send the fake scale to.
        /// </summary>
        public readonly Predicate<ExPlayer>? JoinPredicate;

        /// <summary>
        /// Creates a new instance of the <see cref="FakeInstance"/> struct.
        /// </summary>
        /// <param name="scale">The fake scale.</param>
        /// <param name="resetOnDeath">Whether or not to reset scale on death.</param>
        /// <param name="resetOnRoleChange">Whether to reset scale on role change.</param>
        /// <param name="joinPredicate">Filter for joined players.</param>
        public FakeInstance(Vector3 scale, bool resetOnDeath, bool resetOnRoleChange, Predicate<ExPlayer>? joinPredicate)
        {
            Scale = scale;
            ResetOnDeath = resetOnDeath;
            ResetOnRoleChange = resetOnRoleChange;
            JoinPredicate = joinPredicate;
        }
    }

    private static readonly Dictionary<ExPlayer, FakeInstance> instances = new();

    /// <summary>
    /// Sets a fake scale for the specified player, optionally updating the scale for the player themselves and configuring reset conditions.
    /// </summary>
    /// <param name="player">The player for whom the fake scale will be set.</param>
    /// <param name="scale">The scale to apply to the player.</param>
    /// <param name="updateSelf">Specifies whether the fake scale should also be applied to the player themselves.</param>
    /// <param name="resetOnDeath">Determines whether the fake scale should reset when the player dies.</param>
    /// <param name="resetOnRoleChange">Indicates whether the fake scale should reset when the player changes roles.</param>
    /// <param name="predicate">An optional predicate to filter which players should receive the scale change.</param>
    /// <exception cref="ArgumentNullException">Thrown if the specified player is null or has a null reference hub.</exception>
    public static void SetFakeScale(this ExPlayer player, Vector3 scale, bool updateSelf, bool resetOnDeath, bool resetOnRoleChange, Predicate<ExPlayer>? predicate = null)
    {
        if (player?.ReferenceHub == null)
            throw new ArgumentNullException(nameof(player));

        var instance = new FakeInstance(scale, resetOnDeath, resetOnRoleChange, predicate);
        
        instances[player] = instance;

        foreach (var other in ExPlayer.Players)
        {
            if (other?.ReferenceHub == null)
                continue;
            
            if (other == player && !updateSelf)
                continue;
            
            other.Send(new SyncedScaleMessages.ScaleMessage(scale, player.ReferenceHub));
        }
    }

    /// <summary>
    /// Removes a fake scale effect applied to the specified player, restoring the player's scale to default.
    /// </summary>
    /// <param name="player">The player whose fake scale will be removed.</param>
    /// <exception cref="ArgumentNullException">Thrown when the provided player is null or invalid.</exception>
    public static void RemoveFakeScale(this ExPlayer player)
    {
        if (player?.ReferenceHub == null)
            throw new ArgumentNullException(nameof(player));

        instances.Remove(player);

        foreach (var other in ExPlayer.Players)
        {
            if (other?.ReferenceHub != null)
            {
                other.Send(new SyncedScaleMessages.ScaleMessage(Vector3.one, player.ReferenceHub));
            }
        }
    }

    private static void OnChangedRole(PlayerChangedRoleEventArgs args)
    {
        if (!args.Player.CastPlayer(out var player))
            return;

        if (!instances.TryGetValue(player, out var instance))
            return;

        if (args.ChangeReason is RoleChangeReason.Died && instance.ResetOnDeath)
            return;
        
        if (!instance.ResetOnRoleChange)
            return;
        
        player.RemoveFakeScale();
    }

    private static void OnDied(PlayerDeathEventArgs args)
    {
        if (!args.Player.CastPlayer(out var player))
            return;

        if (!instances.TryGetValue(player, out var instance))
            return;

        if (!instance.ResetOnDeath)
            return;
        
        player.RemoveFakeScale();
    }

    private static void OnLeft(ExPlayer player)
    {
        instances.Remove(player);
    }

    private static void OnVerified(ExPlayer player)
    {
        foreach (var pair in instances)
        {
            if (pair.Key?.ReferenceHub == null)
                continue;
            
            if (pair.Value.JoinPredicate != null && !pair.Value.JoinPredicate(player))
                continue;
            
            player.Send(new SyncedScaleMessages.ScaleMessage(pair.Value.Scale, pair.Key.ReferenceHub));
        }
    }

    private static void OnWaiting()
    {
        instances.Clear();
    }

    private static void Initialize()
    {
        PlayerEvents.Death += OnDied;
        PlayerEvents.ChangedRole += OnChangedRole;
        
        ExPlayerEvents.Left += OnLeft;
        ExPlayerEvents.Verified += OnVerified;

        ExRoundEvents.WaitingForPlayers += OnWaiting;
    }
}