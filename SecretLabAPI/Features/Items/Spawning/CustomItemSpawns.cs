using LabExtended.API;
using LabExtended.API.Custom.Items;

using LabExtended.Core;
using LabExtended.Events;
using UnityEngine;

namespace SecretLabAPI.Features.Items.Spawning
{
    /// <summary>
    /// Spawns items at predefined positions.
    /// </summary>
    public static class CustomItemSpawns
    {
        /// <summary>
        /// Gets the list of custom item spawn positions from the config.
        /// </summary>
        public static Dictionary<string, List<string>> Positions => SecretLab.Config.CustomSpawns;

        private static void Internal_RoundStarted()
        {
            /*
            foreach (var pair in Positions)
            {
                if (pair.Key != "ExamplePosition")
                {
                    if (MapUtilities.TryGet(pair.Key, null, out var position, out var rotation))
                    {
                        foreach (var item in pair.Value)
                        {
                            if (Enum.TryParse<ItemType>(item, true, out var itemType))
                            {
                                if (itemType != ItemType.None)
                                {
                                    ExMap.SpawnItem(itemType, position, Vector3.one, rotation);
                                }
                                else
                                {
                                    ApiLog.Error("Item Spawnpoint", "Cannot spawn item of type &1None&r");
                                }
                            }
                            else if (CustomItem.RegisteredObjects.TryGetValue(item, out var customItem))
                            {
                                customItem.SpawnItem(position, rotation);
                            }
                            else
                            {
                                ApiLog.Warn("Item Spawnpoints", $"Unknown item: &3{item}&r");
                            }
                        }
                    }
                    else
                    {
                        ApiLog.Error("Item Spawnpoints", $"Could not find spawn position &1{pair.Key}&r");
                    }
                }
            }
            */
        }

        internal static void Internal_Init()
        {
            ExRoundEvents.Started += Internal_RoundStarted;
        }
    }
}