using System.ComponentModel;

using LabApi.Events.Arguments.PlayerEvents;

using LabExtended.API;
using LabExtended.Extensions;

using PlayerRoles;

using SecretLabAPI.Extensions;
using SecretLabAPI.Features.Elements.Alerts;

namespace SecretLabAPI.Features.Items.Custom.ScpItems.Scp500;

/// <summary>
/// Represents a custom SCP-500-R item that can be spawned, picked up, and used by players.
/// This item inherits functionality from the <see cref="SpawnableCustomItem"/> base class.
/// When used, the item teleports the player to a random room within predefined facility zones.
/// </summary>
public class SCP500R : SpawnableCustomItem
{
    /// <summary>
    /// The unique identifier for the item.
    /// </summary>
    public override string Id { get; } = "scp500r";

    /// <summary>
    /// The name of the item.
    /// </summary>
    public override string Name { get; } = "SCP-500-R";

    /// <summary>
    /// The type of item that the item can be picked up as.
    /// </summary>
    public override ItemType PickupType { get; set; } = ItemType.SCP500;

    /// <summary>
    /// The type of item that the item can be stored in.
    /// </summary>
    public override ItemType InventoryType { get; set; } = ItemType.SCP500;

    /// <summary>
    /// Message to display when the player tries to use the item without spectators.
    /// </summary>
    [Description("Message to display when the player tries to use the item without spectators.")]
    public string NoSpectatorsMessage { get; set; }

    /// <summary>
    /// Message to display to the target when they get revived.
    /// </summary>
    [Description("Message to display when to the target when they get revived.")]
    public string TargetRevivedMessage { get; set; }
    
    /// <summary>
    /// Message to display to the player who used the SCP-500-R when they get revived.
    /// </summary>
    [Description("Message to display to the player who used the SCP-500-R when they get revived.")]
    public string PlayerRevivedMessage { get; set; }

    /// Handles the logic when the SCP-500-R item is being used by a player. Checks if there are any spectators available
    /// and sends an alert message if none are present. Prevents the usage of the item if the condition is not met.
    /// <param name="args">The event arguments containing details about the player using the item.</param>
    /// <param name="itemData">A reference to the custom item data, which can be modified as necessary to apply effects
    /// or alterations to the item during its use.</param>
    public override void OnUsingItem(PlayerUsingItemEventArgs args, ref object? itemData)
    {
        base.OnUsingItem(args, ref itemData);

        if (!args.Player.CastPlayer(out var player))
            return;

        if (!ExPlayer.Players.Any(p => p.Role.IsSpectator))
        {
            args.IsAllowed = false;
            
            player.SendFormattedAlert(NoSpectatorsMessage, true, AlertType.Warn, 5f, "SCP-500-R");
        }
    }

    /// Handles the logic when the item is used by a player. Overrides the base implementation to provide
    /// additional functionality specific to the "SCP-500-T" custom item. If the item is successfully used,
    /// the player is teleported to a random room within the defined facility zones.
    /// <param name="args">The event arguments containing details of the player using the item.</param>
    /// <param name="itemData">A reference to the custom item data. This can be modified as needed by the method
    /// to apply any specific effects or alterations to the item after use.</param>
    public override void OnUsedItem(PlayerUsedItemEventArgs args, ref object? itemData)
    {
        base.OnUsedItem(args, ref itemData);

        if (!args.Player.CastPlayer(out var player))
            return;
        
        var spectator = ExPlayer.Players.GetRandomItem(p => p != null && p.Role.IsSpectator);

        if (spectator != null)
        {
            spectator.Role.Set(player.Role, RoleChangeReason.Revived, RoleSpawnFlags.AssignInventory);
            spectator.Position.Position = player.PositionAdjustY(0.5f);
            spectator.SendFormattedAlert(TargetRevivedMessage.Replace("$Nick", player.Nickname), true, AlertType.Info, 5f, "SCP-500-R");
            
            player.SendFormattedAlert(PlayerRevivedMessage.Replace("$Nick", spectator.Nickname), true, AlertType.Info, 5f, "SCP-500-R");
        }
    }
}