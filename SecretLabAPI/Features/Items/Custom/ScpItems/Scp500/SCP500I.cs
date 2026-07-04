using System.ComponentModel;

using CustomPlayerEffects;

using LabApi.Events.Arguments.PlayerEvents;

using SecretLabAPI.Extensions;

namespace SecretLabAPI.Features.Items.Custom.ScpItems.Scp500;

/// <summary>
/// Represents the custom SCP-500-I item, which grants players temporary invisibility
/// upon use. This item is designed as a derived implementation of <see cref="SpawnableCustomItem"/>.
/// </summary>
public class SCP500I : SpawnableCustomItem
{
    /// <summary>
    /// The unique identifier for the item.
    /// </summary>
    public override string Id { get; } = "scp500i";

    /// <summary>
    /// The name of the item.
    /// </summary>
    public override string Name { get; } = "SCP-500-I";

    /// <summary>
    /// The type of item that the item can be picked up as.
    /// </summary>
    public override ItemType PickupType { get; set; } = ItemType.SCP500;

    /// <summary>
    /// The type of item that the item can be stored in.
    /// </summary>
    public override ItemType InventoryType { get; set; } = ItemType.SCP500;

    /// <summary>
    /// The duration of the invisibility effect in seconds.
    /// </summary>
    [Description("Sets the duration of the invisibility effect in seconds.")]
    public float InvisibilityDuration { get; set; } = 10f;

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

        player.EnableEffect<Invisible>(1, InvisibilityDuration, true);
    }
}