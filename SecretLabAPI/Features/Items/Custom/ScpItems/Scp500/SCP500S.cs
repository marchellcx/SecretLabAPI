using LabApi.Events.Arguments.PlayerEvents;

using PlayerRoles;

using SecretLabAPI.Extensions;

namespace SecretLabAPI.Features.Items.Custom.ScpItems.Scp500;

/// <summary>
/// Represents the SCP-500-S custom item. It is a subclass of <see cref="SpawnableCustomItem"/>,
/// providing unique behavior when the item is used by players. The item alters the player's role
/// based on their current team membership.
/// </summary>
public class SCP500S : SpawnableCustomItem
{
    /// <summary>
    /// The unique identifier for the item.
    /// </summary>
    public override string Id { get; } = "scp500s";

    /// <summary>
    /// The name of the item.
    /// </summary>
    public override string Name { get; } = "SCP-500-S";

    /// <summary>
    /// The type of item that the item can be picked up as.
    /// </summary>
    public override ItemType PickupType { get; set; } = ItemType.SCP500;

    /// <summary>
    /// The type of item that the item can be stored in.
    /// </summary>
    public override ItemType InventoryType { get; set; } = ItemType.SCP500;

    /// <summary>
    /// Executes logic when the SCP-500-S item is used by a player. This method determines the player's team, clears their inventory,
    /// and changes their role based on their team affiliation.
    /// </summary>
    /// <param name="args">The event arguments containing information about the player who used the item.</param>
    /// <param name="itemData">Reference to any additional data associated with the item.</param>
    public override void OnUsedItem(PlayerUsedItemEventArgs args, ref object? itemData)
    {
        base.OnUsedItem(args, ref itemData);

        if (!args.Player.CastPlayer(out var player))
            return;

        switch (player.Role.Team)
        {
            case Team.Scientists:
            case Team.ChaosInsurgency:
                player.Inventory.DropItems();
                player.Role.Set(Team.FoundationForces.GetRandomRole(RoleTypeId.FacilityGuard), RoleChangeReason.None, RoleSpawnFlags.AssignInventory);
                break;
            
            case Team.ClassD:
            case Team.FoundationForces:
                player.Inventory.DropItems();
                player.Role.Set(Team.ChaosInsurgency.GetRandomRole(), RoleChangeReason.None, RoleSpawnFlags.AssignInventory);
                break;
        }
    }
}