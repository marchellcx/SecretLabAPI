using LabExtended.API;
using LabExtended.Utilities;

using LabExtended.Events;
using LabExtended.Events.Map;

namespace SecretLabAPI.Features.Items.Spawning
{
    /// <summary>
    /// Prevents items from spawning in certain areas or under certain conditions.
    /// </summary>
    public static class BlacklistedItemSpawns
    {
        /// <summary>
        /// Gets the list of item types that are prevented from spawning on round start.
        /// </summary>
        public static List<ItemType> Blacklist => SecretLab.Config.PreventSpawn;

        private static void Internal_DistributingPickup(DistributingPickupEventArgs args)
        {
            if (Blacklist.Contains(args.Pickup.Info.ItemId))
            {
                args.IsAllowed = false;
            }
        }

        private static void Internal_LockerSpawningPickup(LockerSpawningPickupEventArgs args)
        {
            if (Blacklist.Contains(args.Type))
            {
                args.IsAllowed = false;
            }
        }

        private static void Internal_RoundStarted()
        {
            TimingUtils.AfterSeconds(Internal_RemoveBlacklistedItems, 0.1f);
        }
        
        private static void Internal_RemoveBlacklistedItems()
        {
            foreach (var pickup in ExMap.Pickups.ToArray())
            {
                if (Blacklist.Contains(pickup.Info.ItemId))
                {
                    pickup.DestroySelf();
                }
            }

            foreach (var chamber in ExMap.Chambers)
            {
                chamber.AcceptableItems = chamber.AcceptableItems.Except(Blacklist).ToArray();

                foreach (var pickup in chamber.Content.ToArray())
                {
                    if (Blacklist.Contains(pickup.Info.ItemId))
                    {
                        chamber.Content.Remove(pickup);

                        pickup.DestroySelf();
                    }
                }
            }
        }

        internal static void Internal_Init()
        {
            ExMapEvents.DistributingPickup += Internal_DistributingPickup;
            ExMapEvents.LockerSpawningPickup += Internal_LockerSpawningPickup;

            ExRoundEvents.Started += Internal_RoundStarted;
        }
    }
}