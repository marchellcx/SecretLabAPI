using LabExtended.API;

using LabExtended.Commands;
using LabExtended.Commands.Attributes;
using LabExtended.Commands.Interfaces;

using PlayerRoles;

using SecretLabAPI.Features;

namespace SecretLabAPI.Commands;

/// <summary>
/// Represents a command to send a kill feed message to specified players.
/// </summary>
/// <remarks>
/// The KillFeedCommand is used to broadcast a kill feed event to a defined set of players.
/// It provides functionality to customize and manage in-game feed updates for player interactions.
/// </remarks>
[Command("killfeed", "Sends a kill feed to the specified players.")]
public class KillFeedCommand : CommandBase, IServerSideCommand
{
    [CommandOverload("Sends a kill feed to the specified players.", null)]
    private void Invoke(
        [CommandParameter("Players", "List of players to send the kill log to.")] List<ExPlayer> players,
        [CommandParameter("Attacker Nickname", "Nickname of the attacking player.")] string attackerNick,
        [CommandParameter("Attacker Role", "Role of the attacking player.")] RoleTypeId attackerRole,
        [CommandParameter("Victim Nick", "Nickname of the victim player.")] string victimNick,
        [CommandParameter("Victim Role", "Role of the victim player.")] RoleTypeId victimRole)
    {
        KillFeed.SendDeathToWhere(attackerNick, attackerRole, victimNick, victimRole, players.Contains);

        Ok($"Sent kill feed to &3{players.Count}&r player(s)!.");
    }
}