using System.ComponentModel;

using InventorySystem.Items.ThrowableProjectiles;

using LabApi.Events.Arguments.ServerEvents;

using LabExtended.API;
using LabExtended.API.Custom.Items;
using LabExtended.API.Custom.Items.Events;

using SecretLabAPI.Extensions;
using SecretLabAPI.Features.Elements.Alerts;

using UnityEngine;

namespace SecretLabAPI.Features.Items.Custom.Grenades;

/// <summary>
/// Represents a custom projectile named "Replicating SCP-018", based on SCP-018.
/// This class defines the behavior of the projectile when it collides or interacts
/// with the environment.
/// </summary>
public class ReplicatingScp018 : CustomProjectile
{
    /// <inheritdoc />
    public override string Id { get; } = "replicating_ball";

    /// <inheritdoc />
    public override string Name { get; } = "Replicating SCP-018";

    /// <inheritdoc />
    public override ItemType PickupType { get; set; } = ItemType.SCP018;

    /// <inheritdoc />
    public override ItemType InventoryType { get; set; } = ItemType.SCP018;

    /// <inheritdoc />
    public override bool ExplodeOnCollision { get; set; } = false;

    /// <inheritdoc />
    public override bool ExplodeOnExplosion { get; set; } = false;

    /// <inheritdoc />
    public override float FuseTime { get; set; } = float.MaxValue;

    /// <inheritdoc />
    public override bool LockProjectile { get; set; } = true;
    
    /// <summary>
    /// Message to display when the player picks up the item.
    /// </summary>
    [Description("Message to display when the player picks up the item.")]
    public string PickUpMessage { get; set; }

    /// <summary>
    /// Invoked when a custom item is added to a player's inventory.
    /// This method processes the event and sends a formatted alert message
    /// to the player who picked up the item.
    /// </summary>
    /// <param name="args">The event arguments containing information about the custom item addition,
    /// such as the player involved and the item details.</param>
    public override void OnItemAdded(CustomItemAddedEventArgs args)
    {
        base.OnItemAdded(args);

        args.Player.SendFormattedAlert(PickUpMessage, true, AlertType.Info, 5f, "Custom Items");
    }

    /// <inheritdoc />
    public override void OnExploding(ProjectileExplodingEventArgs args, ref object? projectileData)
    {
        base.OnExploding(args, ref projectileData);

        args.IsAllowed = false;
    }

    internal void ProcessBounce(Scp018Projectile projectile, float velocity, Vector3 point)
    {
        ExMap.SpawnProjectile(ItemType.SCP018, point, Vector3.one, new(velocity, velocity, velocity),
            projectile.Rotation, velocity, 10f);
    }
}