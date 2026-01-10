using InventorySystem.Items.ThrowableProjectiles;

using LabApi.Events.Arguments.ServerEvents;

using LabExtended.API;
using LabExtended.API.Custom.Items;

using UnityEngine;

namespace SecretLabAPI.Items.Weapons
{
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

        /// <inheritdoc />
        public override void OnExploding(ProjectileExplodingEventArgs args, ref object? projectileData)
        {
            base.OnExploding(args, ref projectileData);

            args.IsAllowed = false;
        }

        internal void ProcessBounce(Scp018Projectile projectile, float velocity, Vector3 point)
        {
            ExMap.SpawnProjectile(ItemType.SCP018, point, Vector3.one, new(velocity, velocity, velocity), projectile.Rotation, velocity, 10f);
        }
    }
}