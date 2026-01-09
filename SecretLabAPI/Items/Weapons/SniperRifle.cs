using InventorySystem.Items.Firearms.Attachments;

using LabExtended.API;

using LabExtended.API.Custom.Items;
using LabExtended.API.Custom.Items.Events;

using SecretLabAPI.Elements.Alerts;

using System.ComponentModel;

namespace SecretLabAPI.Items.Weapons
{
    /// <summary>
    /// A custom sniper rifle item with configurable properties such as damage, ammo capacity, and attachments.
    /// </summary>
    public class SniperRifle : CustomFirearm
    {
        public static SniperRifle Instance { get; private set; }

        /// <inheritdoc/>
        public override string Id { get; } = "sniperrifle";

        /// <inheritdoc/>
        public override string Name { get; } = "Sniper Rifle";

        /// <inheritdoc/>
        public override int MaxAmmo { get; set; } = 1;

        /// <inheritdoc/>
        public override bool CanChangeAttachments { get; set; } = false;

        /// <inheritdoc/>
        public override ItemType PickupType { get; set; } = ItemType.GunE11SR;

        /// <inheritdoc/>
        public override ItemType InventoryType { get; set; } = ItemType.GunE11SR;

        /// <inheritdoc/>
        public override AttachmentName[]? DefaultAttachments { get; set; } =
        [
            AttachmentName.ScopeSight,
            AttachmentName.LightweightStock,
            AttachmentName.SoundSuppressor,
            AttachmentName.StandardMagFMJ,
            AttachmentName.RifleBody
        ];

        /// <summary>
        /// Gets or sets the damage the sniper rifle deals.
        /// </summary>
        [Description("Sets the damage of the sniper rifle.")]
        public float Damage { get; set; } = 250f;

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

        /// <inheritdoc/>
        public override void OnItemAdded(CustomItemAddedEventArgs args)
        {
            base.OnItemAdded(args);

            args.Player.SendAlert(AlertType.Info, 10f, "Custom Items",
                "Dostal si <color=red>Sniper Rifle</color>!\n" +
                "Tato zbraň dává damage <color=yellow>250 HP</color> při <b>každé</b> ráně!");
        }
    }
}