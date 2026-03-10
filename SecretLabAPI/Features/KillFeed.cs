using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;

using LabExtended.API;
using LabExtended.Extensions;

using Mirror;

using PlayerRoles;

using Respawning;

using SecretLabAPI.Extensions;

namespace SecretLabAPI.Features;

/// <summary>
/// Provides functionality for sending death notifications to players.
/// </summary>
public static class KillFeed
{
    /// <summary>
    /// The index of the KillObjective.
    /// </summary>
    public const int KillObjectiveIndex = 0;

    /// <summary>
    /// Sends a death notification to the specified target, providing details about the attacker and victim.
    /// </summary>
    /// <param name="target">The player receiving the death notification. Cannot be null.</param>
    /// <param name="attackerNick">The nickname of the attacking player.</param>
    /// <param name="attackerRole">The role type of the attacking player.</param>
    /// <param name="victimNick">The nickname of the victim player.</param>
    /// <param name="victimRole">The role type of the victim player.</param>
    /// <exception cref="ArgumentNullException">Thrown when the target is null.</exception>
    public static void SendDeath(ExPlayer target, string attackerNick, RoleTypeId attackerRole, string victimNick,
        RoleTypeId victimRole)
    {
        if (target == null)
            throw new ArgumentNullException(nameof(target));
        
        using (var writer = NetworkWriterPool.Get())
        {
            writer.WriteMessageId<ObjectiveCompletionMessage>();
            
            writer.WriteInt(KillObjectiveIndex); // ObjectiveIndex
            
            writer.WriteFloat(0f); // InfluenceReward
            writer.WriteFloat(0f); // TimeReward
            
            writer.WriteString(attackerNick); // AchievingPlayer.Nickname
            writer.WriteRoleType(attackerRole); // AchievingPlayer.Role
            
            writer.WriteString(victimNick); // VictimFootprint.Nickname
            writer.WriteRoleType(victimRole); // VictimFootprint.Role
            
            target.Connection.Send(writer);
        }
    }

    /// <summary>
    /// Sends a death notification to players that satisfy the specified condition, providing details about the attacker and victim.
    /// </summary>
    /// <param name="attackerNick">The nickname of the attacking player.</param>
    /// <param name="attackerRole">The role type of the attacking player.</param>
    /// <param name="victimNick">The nickname of the victim player.</param>
    /// <param name="victimRole">The role type of the victim player.</param>
    /// <param name="predicate">A condition to determine which players will receive the death notification. Cannot be null.</param>
    /// <exception cref="ArgumentNullException">Thrown when the predicate is null.</exception>
    public static void SendDeathToWhere(string attackerNick, RoleTypeId attackerRole, string victimNick, RoleTypeId victimRole, 
        Predicate<ExPlayer> predicate)
        => ExPlayer.Players.Where(p => p?.ReferenceHub != null && predicate(p))
                           .ForEach(target => SendDeath(target, attackerNick, attackerRole, victimNick, victimRole));

    /// <summary>
    /// Sends a death notification to all players, providing details about the attacker and victim.
    /// </summary>
    /// <param name="attackerNick">The nickname of the attacking player.</param>
    /// <param name="attackerRole">The role type of the attacking player.</param>
    /// <param name="victimNick">The nickname of the victim player.</param>
    /// <param name="victimRole">The role type of the victim player.</param>
    public static void SendDeathToAll(string attackerNick, RoleTypeId attackerRole, string victimNick, RoleTypeId victimRole) =>
        ExPlayer.Players.ForEach(target => SendDeath(target, attackerNick, attackerRole, victimNick, victimRole));

    private static void OnDied(PlayerDeathEventArgs args)
    {
        if (args.Player is not ExPlayer player
            || args.Attacker is not ExPlayer attacker)
            return;

        if (!player.IsValidPlayer() || !attacker.IsValidPlayer()
            || player == attacker)
            return;
        
        SendDeathToAll(attacker.Nickname, attacker.Role, player.Nickname, args.OldRole);
    }

    internal static void Initialize()
    {
        PlayerEvents.Death += OnDied;
    }
}