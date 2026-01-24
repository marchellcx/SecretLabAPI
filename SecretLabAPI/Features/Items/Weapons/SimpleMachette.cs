using LabExtended.API.Custom.Items;

namespace SecretLabAPI.Features.Items.Weapons
{
    /// <summary>
    /// Just SCP-1509 without the ability to respawn spectators.
    /// </summary>
    public class SimpleMachette : CustomItem
    {
        /// <inheritdoc/>
        public override string Id { get; } = "machette";

        /// <inheritdoc/>
        public override string Name { get; } = "Simple Machette";

        /// <inheritdoc/>
        public override ItemType PickupType { get; set; } = ItemType.SCP1509;

        /// <inheritdoc/>
        public override ItemType InventoryType { get; set; } = ItemType.SCP1509;
    }
}