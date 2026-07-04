using System.ComponentModel;

using LabExtended.API;
using LabExtended.API.Custom.Gamemodes;
using LabExtended.Utilities;
using LabExtended.Extensions;

using LabExtended.Events.Round;

using PlayerRoles;

using SecretLabAPI.Extensions;
using SecretLabAPI.Features.Elements.Alerts;

namespace SecretLabAPI.Features.RandomEvents.Events;

/// <summary>
/// Represents a custom random event in the game wherein players face off in a Boss Fight scenario.
/// </summary>
public class BossFightEvent : RandomEventBase
{
    /// <summary>
    /// The unique identifier for the event.
    /// </summary>
    public override string Id { get; } = "BossFight";

    /// <summary>
    /// Determines whether the event can be combined with other events in a group.
    /// If true, this event may occur alongside other compatible events.
    /// </summary>
    public override bool CanBeGrouped { get; set; } = true;
    
    /// <summary>
    /// Determines whether the event can be activated during the mid-round.
    /// </summary>
    public override bool CanActivateMidRound { get; set; } = false;

    /// <summary>
    /// Determines whether the event should be disabled when the round ends.
    /// </summary>
    public override bool ShouldDisableOnRoundEnd { get; set; } = true;
    
    /// <summary>
    /// The message to be sent to players when the event is enabled.
    /// </summary>
    [Description("The message to be sent to players when the event is enabled.")]
    public string EnabledMessage { get; set; }

    /// <summary>
    /// The message to be sent to players when they join the server with the event enabled.
    /// </summary>
    [Description("The message to be sent to players when they join the server with the event enabled.")]
    public string JoinedMessage { get; set; }
    
    /// <summary>
    /// Whether or not to use the round start event to replace SCPs.
    /// </summary>
    [Description("Whether or not to use the round start event to replace SCPs.")]
    public bool UseRoundStart { get; set; }
    
    /// <summary>
    /// The minimum number of players required to participate in the event.
    /// </summary>
    [Description("The minimum number of players required to participate in the event.")]
    public int MinPlayerCount { get; set; }

    /// <summary>
    /// The possible SCP roles and their health values.
    /// </summary>
    [Description("The possible SCP roles and their health values.")]
    public Dictionary<RoleTypeId, float> Roles { get; set; } = new()
    {
        { RoleTypeId.Scp173, 10000f },
    };

    /// <summary>
    /// Determines whether the random event can be enabled based on current conditions.
    /// </summary>
    /// <param name="otherModes">A list of other active custom gamemodes in the game.</param>
    /// <returns>True if the event can be enabled; otherwise, false.</returns>
    public override bool CanBeEnabled(List<CustomGamemode> otherModes)
    {
        return base.CanBeEnabled(otherModes) && MinPlayerCount > 0 && ExPlayer.Players.Count >= MinPlayerCount;
    }

    /// <summary>
    /// Handles the initialization logic when the random event is enabled.
    /// </summary>
    public override void OnEnabled()
    {
        base.OnEnabled();

        ExPlayer.Players.ForEach(p => p.SendFormattedAlert(EnabledMessage, true, AlertType.Info, 5f, "Boss Fight"));
    }

    /// <summary>
    /// Executes logic when a player joins the server while the Double Trouble event is enabled.
    /// Sends a formatted alert message to the player to indicate their participation in the event.
    /// </summary>
    /// <param name="player">The player who has just joined the server.</param>
    public override void OnPlayerJoined(ExPlayer player)
    {
        base.OnPlayerJoined(player);
        
        player.SendFormattedAlert(JoinedMessage, true, AlertType.Info, 5f, "Boss Fight");
    }

    /// <summary>
    /// Handles the logic for assigning roles to players during the random event.
    /// Modifies the roles of SCPs and assigns other roles based on predefined conditions.
    /// </summary>
    /// <param name="args">The event arguments containing the roles of players to be assigned.</param>
    public override void OnAssigningRoles(AssigningRolesEventArgs args)
    {
        base.OnAssigningRoles(args);

        if (UseRoundStart)
            return;

        var scps = args.Roles.Where(kvp => kvp.Value.IsScp()).ToPooledList();

        if (scps.Count < 1)
        {
            scps.ReturnToPool();
            return;
        }

        var scp = scps.RandomItem();

        scps.Remove(scp);

        args.Roles[scp.Key] = Roles.GetRandomItem().Key;

        foreach (var otherScp in scps)
        {
            args.Roles[otherScp.Key] = WeightUtils.GetBool(30f)
                ? RoleTypeId.FacilityGuard
                : (WeightUtils.GetBool(40f)
                    ? RoleTypeId.Scientist
                    : RoleTypeId.ClassD);
        }
        
        scps.ReturnToPool();
    }

    /// <summary>
    /// Executes the logic to modify player roles at the start of a round
    /// when the random event is active and configured to use the round start trigger.
    /// Specifically, this method evaluates the roles of SCP players and reassigns them
    /// to predetermined roles based on specified conditions and probabilities.
    /// </summary>
    public override void OnRoundStarted()
    {
        base.OnRoundStarted();

        if (!UseRoundStart)
        {
            var player = ExPlayer.Players.First(p => p.Role.IsScp);

            if (player.IsValidPlayer())
            {
                player.MaxHealth = Roles[player.Role.Type];
                player.Health = player.MaxHealth;
            }

            return;
        }

        var scps = ExPlayer.Players.Where(p => p.Role.IsScp);

        if (scps.Count() < 1)
            return;

        var target = scps.GetRandomItem();
        var role = Roles.GetRandomItem();
        
        target.Role.Set(role.Key, RoleChangeReason.RoundStart, RoleSpawnFlags.All);
        target.MaxHealth = role.Value;
        target.Health = role.Value;

        foreach (var other in scps)
        {
            if (other != target)
            {
                other.Role.Set(WeightUtils.GetBool(30f)
                    ? RoleTypeId.FacilityGuard
                    : (WeightUtils.GetBool(40f)
                        ? RoleTypeId.Scientist
                        : RoleTypeId.ClassD), RoleChangeReason.RoundStart, RoleSpawnFlags.All);
            }
        }
    }
}