using System.ComponentModel;

using LabExtended.API;
using LabExtended.API.Custom.Items;
using LabExtended.API.Custom.Items.Events;

using SecretLabAPI.Extensions;
using SecretLabAPI.Features.Elements.Alerts;

namespace SecretLabAPI.Features.Items.Custom.Weapons;

/// <summary>
/// An airsoft gun.
/// </summary>
public class AirsoftGun : CustomFirearm
{
    public static AirsoftGun Instance { get; private set; }

    /// <inheritdoc/>
    public override string Id { get; } = "ptc.airsoftgun";

    /// <inheritdoc/>
    public override string Name { get; } = "Airsoft Gun";

    /// <inheritdoc/>
    public override ItemType PickupType { get; set; } = ItemType.GunCrossvec;

    /// <inheritdoc/>
    public override ItemType InventoryType { get; set; } = ItemType.GunCrossvec;

    /// <summary>
    /// Gets or sets the damage the firearm deals.
    /// </summary>
    [Description("Sets the damage the airsoft gun deals.")]
    public float Damage { get; set; } = 5f;
    
    /// <summary>
    /// Message to display when the player picks up the item.
    /// </summary>
    [Description("Message to display when the player picks up the item.")]
    public string PickUpMessage { get; set; }

    /// <summary>
    /// Called when the impact grenade is added to a player's inventory.
    /// </summary>
    /// <param name="args">The event arguments containing details about the item being added, including the player and related data.</param>
    public override void OnItemAdded(CustomItemAddedEventArgs args)
    {
        base.OnItemAdded(args);
        
        args.Player.SendFormattedAlert(PickUpMessage, true, AlertType.Info, 5f, "Custom Items");
    }

    /// <inheritdoc/>
    public override float ModifyDamage(ExPlayer target, float damage)
    {
        return Damage;
    }

    public override void OnRegistered()
    {
        base.OnRegistered();

        Instance = this;
    }
}