using LabExtended.API.Containers;
using LabExtended.API.Custom.Items;
using LabExtended.API.Hints;

using LabExtended.Core;
using LabExtended.Extensions;

using PlayerRoles;

using SecretLabAPI.Actions.API;
using SecretLabAPI.Actions.Enums;
using SecretLabAPI.Actions.Attributes;

using SecretLabAPI.Audio.Clips;
using SecretLabAPI.Elements.Alerts;
using SecretLabAPI.Extensions;
using SecretLabAPI.Utilities;

using UnityEngine;

using StringExtensions = SecretLabAPI.Extensions.StringExtensions;

namespace SecretLabAPI.Actions.Functions
{
    /// <summary>
    /// Contains player functions.
    /// </summary>
    public static class PlayerFunctions
    {
        /// <summary>
        /// Initiates the playback of an audio clip for a player, optionally with specified volume and playback mode.
        /// </summary>
        /// <remarks>The method retrieves parameters from the provided context to determine the clip name, volume, and
        /// whether the playback is personalized for the target player. It ensures the parameters are validated before proceeding
        /// with the playback.</remarks>
        /// <param name="context">A reference to the action context containing the information necessary to play the clip, including
        /// clip name, playback volume, and personalization flag.</param>
        /// <returns>An ActionResultFlags value indicating the result of the action. Returns SuccessDispose if the playback operation
        /// was successful and resources can be released.</returns>
        [Action("PlayClip", "Starts playing an audio clip.")]
        [ActionParameter("Name", "The name of the clip to play.")]
        [ActionParameter("Volume", "The volume to play the clip at (0 - 1).")]
        [ActionParameter("Personal", "Whether or not the audio should only be heard by the target player.")]
        public static ActionResultFlags PlayClip(ref ActionContext context)
        {
            context.EnsureCompiled((index, p) =>
            {
                return index switch
                {
                    0 => p.EnsureCompiled(string.Empty),
                    1 => p.EnsureCompiled(float.TryParse, 1f),
                    2 => p.EnsureCompiled(bool.TryParse, false),
                    
                    _ => false
                };
            });
            
            var name = context.GetValue(0);
            var volume = context.GetValue<float>(1);
            var personal = context.GetValue<bool>(2);

            context.Player.PlayClip(name, volume, personal);
            return ActionResultFlags.SuccessDispose;
        }

        /// <summary>
        /// Stops the playback of any currently playing audio clips for the target player.
        /// </summary>
        /// <remarks>This method halts any active audio clip associated with the player specified in the provided context.
        /// Resources associated with the playback are disposed of upon successful termination.</remarks>
        /// <param name="context">A reference to the action context containing the player for which any active audio playback
        /// should be stopped. This includes any metadata necessary to identify the player and clip being acted upon.</param>
        /// <returns>An ActionResultFlags value that signals the result of the operation. Returns SuccessDispose if the clip was
        /// successfully stopped and resources were released.</returns>
        [Action("StopClip", "Stops any playing audio clips.")]
        public static ActionResultFlags StopClip(ref ActionContext context)
        {
            context.Player.StopClip(out _);
            return ActionResultFlags.SuccessDispose;
        }
        
        /// <summary>
        /// Detonates a grenade at each player in the specified context, causing them to explode with customizable
        /// effects and ragdoll velocity.
        /// </summary>
        /// <remarks>The explosion parameters are determined by values in the context: grenade type, death
        /// reason, whether the explosion affects other players, and the ragdoll velocity multiplier. This method
        /// applies the explosion to all players in the context.</remarks>
        /// <param name="context">A reference to the action context containing player information and parameters for the explosion, including
        /// grenade type, death reason, effect-only mode, and velocity multiplier.</param>
        /// <returns>An ActionResultFlags value indicating the result of the action. Returns SuccessDispose if the explosion was
        /// performed successfully.</returns>
        [Action("Explode", "Explodes a player.")]
        [ActionParameter("Type", "The type of the grenade to spawn.")]
        [ActionParameter("Amount", "The amount of grenades to spawn.")]
        [ActionParameter("Reason", "The reason for the player's death.")]
        [ActionParameter("EffectOnly", "Whether or not the grenade should damage other players.")]
        [ActionParameter("KillPlayer", "Whether or not the grenade should kill the targeted player.")]
        [ActionParameter("Velocity", "The velocity multiplier for the player's ragdoll.")]
        public static ActionResultFlags Explode(ref ActionContext context)
        {
            context.EnsureCompiled((index, p) =>
            {
                return index switch
                {
                    0 => p.EnsureCompiled(Enum.TryParse, ItemType.GrenadeHE),
                    1 => p.EnsureCompiled(int.TryParse, 1),
                    2 => p.EnsureCompiled("You get five big booms!"),
                    3 => p.EnsureCompiled(bool.TryParse, true),
                    4 => p.EnsureCompiled(bool.TryParse, true),
                    5 => p.EnsureCompiled(float.TryParse, 10f),

                    _ => false
                };
            });

            var type = context.GetValue<ItemType>(0);
            var amount = context.GetValue<int>(1);
            var reason = context.GetValue(2);
            var effect = context.GetValue<bool>(3);
            var killPlayer = context.GetValue<bool>(4);
            var velocity = context.GetValue<float>(5);

            context.Player.Explode(amount, type, reason, effect, killPlayer, velocity);
            return ActionResultFlags.SuccessDispose;
        }

        /// <summary>
        /// Adds an item or a specified amount of items to each player's inventory within the provided action context.
        /// Supports both standard item types and custom items, as well as ammo types.
        /// </summary>
        /// <remarks>The item type can be specified as a value from the ItemType enumeration or as the
        /// identifier of a custom item. If the item type represents ammo, the amount parameter determines the quantity
        /// of ammo to add; otherwise, it determines the number of items to add. The operation is performed for all
        /// players in the context.</remarks>
        /// <param name="context">A reference to the action context containing player information and parameters specifying the item type and
        /// amount to add. Must not be null.</param>
        /// <returns>An ActionResultFlags value indicating the result of the operation. Returns SuccessDispose if the items were
        /// added successfully.</returns>
        [Action("AddItem", "Adds an item to a player's inventory.")]
        [ActionParameter("Type", "Sets the type of the item to add (can be an item from the ItemType enum or an ID of a custom item).")]
        [ActionParameter("Amount", "Sets the amount of items to add (or the amount of ammo to add if the Type is an ammo).")]
        public static ActionResultFlags AddItem(ref ActionContext context)
        {
            context.EnsureCompiled((index, p) =>
            {
                return index switch
                {
                    0 => p.EnsureCompiled("None"),
                    1 => p.EnsureCompiled(int.TryParse, 1),

                    _ => false
                };
            });

            var item = context.GetMemoryOrValue("ItemType", 0);
            var amount = context.GetMemoryOrValue<int>("ItemAmount", 1);
            var p = context.Player;

            if (Enum.TryParse<ItemType>(item, true, out var itemType))
            {
                if (itemType.IsAmmo())
                {
                    p.Ammo.AddAmmo(itemType, (ushort)amount);
                }
                else
                {
                    for (var i = 0; i < amount; i++)
                    {
                        p.Inventory.AddItem(itemType);
                    }
                }
            }
            else if (CustomItem.TryGet(item, out var customItem))
            {
                for (var i = 0; i < amount; i++)
                {
                    customItem.AddItem(p);
                }
            }
            else
            {
                ApiLog.Error("ActionManager", $"&6[AddItem]&r Item &3{item}&r could not be parsed!");
            }

            return ActionResultFlags.SuccessDispose;
        }

        /// <summary>
        /// Adds multiple items to the player's inventory based on the provided item list in the action context.
        /// </summary>
        /// <remarks>The item list in the context can include both standard item types and custom item
        /// IDs. Each entry must specify the item and the amount, separated by a colon. Invalid or unrecognized items
        /// are skipped, and warnings are logged. The method processes all players in the context, adding the specified
        /// items to each player's inventory.</remarks>
        /// <param name="context">A reference to the action context containing the item list to add. The context must include a value
        /// formatted as 'ItemTypeOrCustomItemID:Amount,Item2:Amount2,Item3:Amount3'.</param>
        /// <returns>An ActionResultFlags value indicating the result of the operation. Returns SuccessDispose if the items were
        /// added successfully.</returns>
        [Action("AddItems", "Adds multiple items to a player's inventory.")]
        [ActionParameter("List", "The list of items to add (formatted as 'ItemTypeOrCustomItemID:Amount,Item2:Amount2,Item3:Amount3')")]
        public static ActionResultFlags AddItems(ref ActionContext context)
        {
            context.EnsureCompiled((index, p) => p.EnsureCompiled(string.Empty));

            var str = context.GetValue(0);

            var items = context.GetMetadata<List<(ItemType BaseItem, CustomItem? CustomItem, int Amount)>>("ParsedItems", () =>
            {
                var list = new List<(ItemType, CustomItem?, int)>();
                var array = str.SplitEscaped(',');

                foreach (var item in array)
                {
                    var parts = item.SplitEscaped(':');

                    if (parts.Length != 2)
                    {
                        if (parts.Length == 1)
                            parts = [parts[0], "1"];

                        ApiLog.Warn("ActionManager", $"[&6AddItems&r] Invalid formatting: &3{item}&r");
                        continue;
                    }

                    var itemPart = parts[0];
                    var amountPart = parts[1];

                    if (!int.TryParse(amountPart, out var amount))
                    {
                        ApiLog.Warn("ActionManager", $"[&6AddItems&r] Invalid formatting: &3{item}&r (amount could not be parsed)");
                        continue;
                    }

                    if (amount < 1)
                    {
                        ApiLog.Warn("ActionManager", $"[&6AddItems&r] Invalid formatting: &3{item}&r (amount is less than one)");
                        continue;
                    }

                    if (Enum.TryParse<ItemType>(itemPart, true, out var itemType))
                    {
                        if (itemType != ItemType.None)
                        {
                            list.Add((itemType, null, amount));
                        }
                        else
                        {
                            ApiLog.Warn("ActionManager", $"[&6AddItems&r] Invalid formatting: &3{item}&r (item type cannot be None)");
                        }
                    }
                    else if (CustomItem.TryGet(itemPart, out var customItem))
                    {
                        list.Add((ItemType.None, customItem, amount));
                    }
                    else
                    {
                        ApiLog.Warn("ActionManager", $"[&6AddItems&r] Invalid formatting: &3{item}&r (item could not be parsed and no custom items were found)");
                    }
                }

                return list;
            });

            var p = context.Player;

            foreach (var item in items)
            {
                for (var i = 0; i < item.Amount; i++)
                {
                    if (item.CustomItem != null)
                    {
                        item.CustomItem.AddItem(p);
                    }
                    else
                    {
                        p.Inventory.AddItem(item.BaseItem);
                    }
                }
            }

            return ActionResultFlags.SuccessDispose;
        }

        /// <summary>
        /// Clears items and/or ammunition from the player's inventory based on the specified context parameters.
        /// </summary>
        /// <remarks>Set the 'Items' and 'Ammo' parameters in the context to control which parts of the
        /// inventory are cleared. This method affects all players referenced in the context.</remarks>
        /// <param name="context">A reference to the action context containing parameters that determine whether to clear items and/or
        /// ammunition. Must be compiled and provide boolean values for 'Items' and 'Ammo'.</param>
        /// <returns>An ActionResultFlags value indicating the result of the operation. Returns SuccessDispose if the inventory
        /// was cleared as specified.</returns>
        [Action("ClearInventory", "Clears the player's inventory.")]
        [ActionParameter("Items", "Whether or not to clear items.")]
        [ActionParameter("Ammo", "Whether or not to clear ammo.")]
        public static ActionResultFlags ClearInventory(ref ActionContext context)
        {
            context.EnsureCompiled((index, p) => p.EnsureCompiled<bool>(bool.TryParse, true));

            var items = context.GetValue<bool>(0);
            var ammo = context.GetValue<bool>(1);
            var p = context.Player;

            if (items)
                p.Inventory.Clear();

            if (!ammo) 
                return ActionResultFlags.SuccessDispose;
            
            p.Ammo.ClearAmmo();
            p.Ammo.ClearCustomAmmo();

            return ActionResultFlags.SuccessDispose;
        }

        /// <summary>
        /// Removes a specified number of items from each player's inventory, optionally filtering by item type.
        /// </summary>
        /// <remarks>If the item type is specified, only items matching that type are removed. If no type
        /// is specified, items are removed in order from the inventory. The amount parameter determines how many items
        /// to remove per player. This method modifies the inventories of all players included in the context.</remarks>
        /// <param name="context">A reference to the action context containing parameters for item type and amount, and the target players
        /// whose inventories will be modified.</param>
        /// <returns>An ActionResultFlags value indicating the result of the operation. Returns SuccessDispose if the items were
        /// cleared successfully.</returns>
        [Action("ClearItems", "Clears a specific amount of items from a player's inventory.")]
        [ActionParameter("Type", "The type of the item to clear.")]
        [ActionParameter("Amount", "The amount of items to clear.")]
        public static ActionResultFlags ClearItems(ref ActionContext context)
        {
            context.EnsureCompiled((index, p) =>
            {
                return index switch
                {
                    0 => p.EnsureCompiled(Enum.TryParse, ItemType.None),
                    1 => p.EnsureCompiled(int.TryParse, 1),

                    _ => false
                };
            });

            var type = context.GetValue<ItemType>(0);
            var amount = context.GetValue<int>(1);
            var p = context.Player;

            if (amount <= 1)
            {
                if (type != ItemType.None)
                {
                    if (p.Inventory.Items.TryGetFirst(x => x.ItemTypeId == type, out var targetItem))
                    {
                        p.Inventory.RemoveItem(targetItem);
                    }
                }
                else
                {
                    if (p.Inventory.ItemCount > 0)
                    {
                        p.Inventory.RemoveItem(p.Inventory.Items.First());
                    }
                }
            }
            else
            {
                if (type != ItemType.None)
                {
                    p.Inventory.RemoveItems(type, amount);
                }
                else
                {
                    amount = Mathf.Min(amount, p.Inventory.ItemCount);

                    var items = p.Inventory.Items.Take(amount);

                    items.ToList().ForEach(item => p.Inventory.RemoveItem(item));
                }
            }

            return ActionResultFlags.SuccessDispose;
        }

        /// <summary>
        /// Removes a specified number of items from each player's inventory, optionally filtering by item type.
        /// </summary>
        /// <remarks>If the item type is specified, only items matching that type are removed. If no type
        /// is specified, items are removed in order from the inventory. The amount parameter determines how many items
        /// to remove per player. This method modifies the inventories of all players included in the context.</remarks>
        /// <param name="context">A reference to the action context containing parameters for item type and amount, and the target players
        /// whose inventories will be modified.</param>
        /// <returns>An ActionResultFlags value indicating the result of the operation. Returns SuccessDispose if the items were
        /// dropped successfully.</returns>
        [Action("DropItems", "Drops a specific amount of items from a player's inventory.")]
        [ActionParameter("Type", "The type of the item to drop.")]
        [ActionParameter("Amount", "The amount of items to drop.")]
        public static ActionResultFlags DropItems(ref ActionContext context)
        {
            context.EnsureCompiled((index, p) =>
            {
                return index switch
                {
                    0 => p.EnsureCompiled(Enum.TryParse, ItemType.None),
                    1 => p.EnsureCompiled(int.TryParse, 1),

                    _ => false
                };
            });

            var type = context.GetValue<ItemType>(0);
            var amount = context.GetValue<int>(1);
            var player = context.Player;

            if (amount <= 1)
            {
                if (type != ItemType.None)
                {
                    if (context.Player.Inventory.Items.TryGetFirst(x => x.ItemTypeId == type, out var targetItem))
                    {
                        context.Player.Inventory.DropItem(targetItem);
                    }
                }
                else
                {
                    if (context.Player.Inventory.ItemCount > 0)
                    {
                        context.Player.Inventory.DropItem(context.Player.Inventory.Items.First());
                    }
                }
            }
            else
            {
                if (type != ItemType.None)
                {
                    var count = context.Player.Inventory.Items.Count(x => x.ItemTypeId == type);
                    var items = context.Player.Inventory.Items.Where(x => x.ItemTypeId == type).Take(Mathf.Min(amount, count));

                    items.ToList().ForEach(item => player.Inventory.DropItem(item));
                }
                else
                {
                    var items = player.Inventory.Items.Take(amount);

                    items.ToList().ForEach(item => player.Inventory.DropItem(item));
                }
            }

            return ActionResultFlags.SuccessDispose;
        }

        /// <summary>
        /// Drops the currently held item from each player's inventory within the specified action context.
        /// </summary>
        /// <param name="context">A reference to the action context containing the players whose held items will be dropped. Cannot be null.</param>
        /// <returns>An ActionResultFlags value indicating the result of the drop operation. Returns SuccessDispose if the
        /// operation completes successfully.</returns>
        [Action("DropHeldItem", "Drops the currently held item from a player's inventory.")]
        public static ActionResultFlags DropHeldItem(ref ActionContext context)
        {
            context.Player.Inventory.DropHeldItem();
            return ActionResultFlags.SuccessDispose;
        }

        /// <summary>
        /// Removes the currently held item from each player's inventory within the specified action context.
        /// </summary>
        /// <param name="context">A reference to the action context containing the players whose held items will be removed.</param>
        /// <returns>An ActionResultFlags value indicating that the held items were successfully removed and the action should be
        /// disposed.</returns>
        [Action("RemoveHeldItem", "Removes the currently held item from the player's inventory.")]
        public static ActionResultFlags RemoveHeldItem(ref ActionContext context)
        {
            context.Player.Inventory.RemoveHeldItem();
            return ActionResultFlags.SuccessDispose;
        }

        /// <summary>
        /// Terminates the targeted player based on the provided context and death reason.
        /// </summary>
        /// <remarks>
        /// The method retrieves the death reason from the action context and applies it to eliminate the associated player. This action is executed in accordance with the context's compiled parameters.
        /// </remarks>
        /// <param name="context">A reference to the action context containing player-specific details and the specified reason for the player's death.</param>
        /// <returns>An ActionResultFlags value indicating the result of the action. Returns SuccessDispose if the player was successfully terminated and the action completed.</returns>
        [Action("Kill", "Kills the targeted player.")]
        [ActionParameter("Reason", "The reason for the player's death.")]
        public static ActionResultFlags Kill(ref ActionContext context)
        {
            context.EnsureCompiled((_, p) => p.EnsureCompiled(string.Empty));
            
            var reason = context.GetValue(0);

            context.Player.Kill(reason);
            return ActionResultFlags.SuccessDispose;
        }

        /// <summary>
        /// Applies damage to a player based on the specified parameters, either reducing their health by a fixed value
        /// or by a percentage of their maximum health. If the player's health is reduced to zero or below, they are killed.
        /// </summary>
        /// <param name="context">A reference to the action context containing the player's information and parameters for the damage action,
        /// such as the damage value and whether it is a percentage of the player's max health.</param>
        /// <returns>An ActionResultFlags value indicating the outcome of the action. Returns SuccessDispose if the damage operation
        /// was successfully executed.</returns>
        [Action("Damage", "Damages a player.")]
        [ActionParameter("Value",
            "The damage value (HP if not using percentage, otherwise the percentage of the player's max health to subtract).")]
        [ActionParameter("IsPercentage", "Whether or not the value should be used as a percentage of the player's max health.")]
        public static ActionResultFlags Damage(ref ActionContext context)
        {
            context.EnsureCompiled((index, p) =>
            {
                return index switch
                {
                    0 => p.EnsureCompiled(int.TryParse, 0),
                    1 => p.EnsureCompiled(bool.TryParse, false),
                    
                    _ => false
                };
            });
            
            var value = context.GetValue<int>(0);
            var isPercentage = context.GetValue<bool>(1);

            if (isPercentage)
                context.Player.Health -= (context.Player.MaxHealth * value) / 100;
            else
                context.Player.Health -= value;

            if (context.Player.Health <= 0)
                context.Player.Kill();

            return ActionResultFlags.SuccessDispose;
        }

        /// <summary>
        /// Heals the player by a specified amount, which can be treated as a flat value or a percentage of the player's maximum health.
        /// </summary>
        /// <remarks>
        /// This method modifies the player's health based on the provided parameters. If `IsPercentage` is true, the value is treated as a percentage of the player's max health.
        /// Overflow behavior and whether the max health is adjusted depend on the corresponding parameters.
        /// </remarks>
        /// <param name="context">A reference to the action context containing the player's data and parameters for the heal action, including value, percentage treatment,
        /// maximum health adjustment, and overflow handling.</param>
        /// <returns>An ActionResultFlags value indicating the result of the action. Returns SuccessDispose if the heal operation is completed successfully.</returns>
        [Action("Heal", "Heals the player by a specified amount of health.")]
        [ActionParameter("Value",
            "The amount of health to give the player (or a percentage of the player's max health).")]
        [ActionParameter("IsPercentage", "Whether or not the value should be treated as the percentage of the player's max health.")]
        [ActionParameter("SetMaxHealth", "Whether or not the player's max health should be increased in case of an overflow.")]
        [ActionParameter("AllowOverflow", "Whether or not the added health should be able to overflow the player's max health.")]
        public static ActionResultFlags Heal(ref ActionContext context)
        {
            context.EnsureCompiled((index, p) =>
            {
                return index switch
                {
                    0 => p.EnsureCompiled(int.TryParse, 0),
                    1 or 2 => p.EnsureCompiled(bool.TryParse, false),
                    3 => p.EnsureCompiled(bool.TryParse, true),
                    
                    _ => false
                };
            });
            
            var value = context.GetValue<int>(0);
            var isPercentage = context.GetValue<bool>(1);
            var setMaxHealth = context.GetValue<bool>(2);
            var allowOverflow = context.GetValue<bool>(3);

            if (isPercentage)
                context.Player.Health += (context.Player.MaxHealth * value) / 100;
            else
                context.Player.Health += value;

            if (setMaxHealth && context.Player.Health > context.Player.MaxHealth)
                context.Player.MaxHealth = context.Player.Health;
            else if (!allowOverflow && context.Player.Health > context.Player.MaxHealth)
                context.Player.Health = context.Player.MaxHealth;

            return ActionResultFlags.SuccessDispose;
        }

        /// <summary>
        /// Adjusts the player's current health and optionally updates their maximum health.
        /// </summary>
        /// <remarks>
        /// This method allows modifying the current health of a player as well as setting their maximum health if specified.
        /// The health values and flags are retrieved from the provided action context, and the method ensures
        /// the input parameters are validated before applying any changes to the player object.
        /// </remarks>
        /// <param name="context">A reference to the action context containing information about the health value to set
        /// and a flag indicating whether to update the player's maximum health.</param>
        /// <returns>An ActionResultFlags value indicating the result of the operation. Returns SuccessDispose if the health
        /// values were successfully updated and resources can be safely disposed.</returns>
        [Action("SetHealth", "Sets the player's current health.")]
        [ActionParameter("Health", "The value to set.")]
        [ActionParameter("SetMax", "Whether or not to set the player's max health to this value.")]
        public static ActionResultFlags SetHealth(ref ActionContext context)
        {
            context.EnsureCompiled((index, p) =>
            {
                return index switch
                {
                    0 => p.EnsureCompiled(float.TryParse, 0f),
                    1 => p.EnsureCompiled(bool.TryParse, true),
                    
                    _ => false
                };
            });
            
            var health = context.GetValue<float>(0);
            var setMax = context.GetValue<bool>(1);

            context.Player.Health = health;

            if (setMax)
                context.Player.MaxHealth = health;

            return ActionResultFlags.SuccessDispose;
        }

        /// <summary>
        /// Adjusts the maximum health of the specified player and optionally updates their current health to match the new maximum value.
        /// </summary>
        /// <remarks>
        /// This method sets the player's maximum health to the specified value. If the "SetMax" parameter is true, the player's current health
        /// will also be updated to the same value as the maximum health.
        /// </remarks>
        /// <param name="context">A reference to the action context containing the parameters needed to execute the action.
        /// Includes the desired maximum health value and a flag indicating whether to synchronize the player's current health with the maximum.</param>
        /// <returns>An ActionResultFlags value indicating the outcome of the operation. Returns SuccessDispose if the action
        /// was successful and the resources related to the context can be released.</returns>
        [Action("SetMaxHealth", "Sets the player's max health.")]
        [ActionParameter("Health", "The value to set.")]
        [ActionParameter("SetMax", "Whether or not to set the player's current health to this value.")]
        public static ActionResultFlags SetMaxHealth(ref ActionContext context)
        {
            context.EnsureCompiled((index, p) =>
            {
                return index switch
                {
                    0 => p.EnsureCompiled(float.TryParse, 0f),
                    1 => p.EnsureCompiled(bool.TryParse, false),
                    
                    _ => false
                };
            });
            
            var health = context.GetValue<float>(0);
            var setMax = context.GetValue<bool>(1);

            context.Player.MaxHealth = health;

            if (setMax)
                context.Player.Health = health;

            return ActionResultFlags.SuccessDispose;
        }

        /// <summary>
        /// Sets the scale of the player's model to the specified vector value.
        /// </summary>
        /// <remarks>
        /// This method retrieves the target scale from the provided action context and applies it to the player's model.
        /// The scale is validated and parsed before being applied. If the operation succeeds, the action completes with
        /// the appropriate success flag, and resources are disposed of accordingly.
        /// </remarks>
        /// <param name="context">A reference to the action context that contains the scale vector to be applied.
        /// The context must provide a valid Vector3 scale parameter.</param>
        /// <returns>An ActionResultFlags value indicating the outcome of the operation. Returns SuccessDispose if the action
        /// completes successfully and resources can be released.</returns>
        [Action("Scale", "Sets the scale of the player's model.")]
        [ActionParameter("Scale", "The scale to set.")]
        public static ActionResultFlags Scale(ref ActionContext context)
        {
            context.EnsureCompiled((_, p) => p.EnsureCompiled(StringExtensions.TryParseVector3, Vector3.one));
            
            var scale = context.GetValue<Vector3>(0);

            context.Player.Scale = scale;
            return ActionResultFlags.SuccessDispose;
        }

        /// <summary>
        /// Sets the voice pitch of a player to a specified value.
        /// </summary>
        /// <remarks>This method retrieves the pitch parameter from the action context, validates it, and applies the value to the player's voice pitch.
        /// The operation ensures that the pitch value is parsed and valid before applying the change.</remarks>
        /// <param name="context">A reference to the action context containing information required to execute the pitch adjustment,
        /// including the desired pitch value and player reference.</param>
        /// <returns>An ActionResultFlags value indicating the result of the operation. Typically returns SuccessDispose upon successful completion.</returns>
        [Action("Pitch", "Sets the voice pitch of a player.")]
        [ActionParameter("Pitch", "The voice pitch to apply.")]
        public static ActionResultFlags Pitch(ref ActionContext context)
        {
            context.EnsureCompiled((_, p) => p.EnsureCompiled(float.TryParse, 1f));
            
            var pitch = context.GetValue<float>(0);

            context.Player.VoicePitch = pitch;
            return ActionResultFlags.SuccessDispose;
        }

        /// <summary>
        /// Sets the gravity for a player to the specified value.
        /// </summary>
        /// <remarks>
        /// This method modifies the player's gravity using a value retrieved from the provided context.
        /// The gravity value is parsed and validated using the context's compiled parameters before being applied to the player.
        /// </remarks>
        /// <param name="context">A reference to the action context containing the parameters necessary for setting the player's gravity.
        /// The context must include a vector representing the desired gravity value.</param>
        /// <returns>An ActionResultFlags value indicating the result of the operation. Returns SuccessDispose upon successful application of the gravity value and cleanup.</returns>
        [Action("Gravity", "Sets a player's gravity.")]
        [ActionParameter("Value", "The value to apply.")]
        public static ActionResultFlags Gravity(ref ActionContext context)
        {
            context.EnsureCompiled((_, p) => p.EnsureCompiled(StringExtensions.TryParseVector3, PositionContainer.DefaultGravity));
            
            var gravity = context.GetValue<Vector3>(0);

            context.Player.Gravity = gravity;
            return ActionResultFlags.SuccessDispose;
        }

        /// <summary>
        /// Resets the gravity of the player to its default value.
        /// </summary>
        /// <remarks>This method modifies the player's gravity property, setting it to the predefined default value for gravity.</remarks>
        /// <param name="context">A reference to the action context that provides access to the target player's properties.</param>
        /// <returns>An ActionResultFlags value indicating the outcome of the operation. Returns SuccessDispose if the gravity was successfully reset and resources can be released.</returns>
        [Action("ResetGravity", "Resets a player's gravity to the default value.")]
        public static ActionResultFlags ResetGravity(ref ActionContext context)
        {
            context.Player.Gravity = PositionContainer.DefaultGravity;
            return ActionResultFlags.SuccessDispose;
        }

        /// <summary>
        /// Disintegrates the player's model, optionally configuring the flying direction and overriding god mode.
        /// </summary>
        /// <remarks>This method ensures that the parameters required for disintegration are retrieved and validated
        /// from the provided context before invoking the action on the player's model.</remarks>
        /// <param name="context">A reference to the action context containing the player's data and parameters necessary for disintegration.
        /// It includes the flying direction (Vector3) and a flag indicating whether god mode should be overridden.</param>
        /// <returns>An ActionResultFlags value indicating the outcome of the action. Returns SuccessDispose if the disintegration
        /// operation was successful and resources can be released.</returns>
        [Action("Disintegrate", "Disintegrates the player's model.")]
        [ActionParameter("FlyDirection", "The direction the player's model will fly to (Vector3).")]
        [ActionParameter("OverrideGodMode", "Whether or not the player's god mode should be disabled before being disintegrated (true).")]
        public static ActionResultFlags Disintegrate(ref ActionContext context)
        {
            context.EnsureCompiled((index, p) =>
            {
                return index switch
                {
                    0 => p.EnsureCompiled(StringExtensions.TryParseVector3, Vector3.up),
                    1 => p.EnsureCompiled(bool.TryParse, true),
                    
                    _ => false
                };
            });
            
            var flyDirection = context.GetValue<Vector3>(0);
            var overrideGodMode = context.GetValue<bool>(1);

            context.Player.Disintegrate(flyDirection, overrideGodMode);
            return ActionResultFlags.SuccessDispose;
        }

        /// <summary>
        /// Sets the role of a specified player, including associated options such as spawn flags.
        /// </summary>
        /// <remarks>
        /// This method updates the role of the target player within the provided context. The operation includes role type and optional
        /// spawn flags that determine the behavior for spawn conditions. Validation is performed on the input parameters before changing the role.
        /// </remarks>
        /// <param name="context">A reference to the action context containing the player information, role type, and spawn flags.</param>
        /// <returns>An ActionResultFlags value indicating the result of the operation. Returns SuccessDispose if the role assignment
        /// succeeds and associated resources can be safely released.</returns>
        [Action("SetRole", "Sets the player's role.")]
        [ActionParameter("Type", "The type of role to set (RoleTypeId)")]
        [ActionParameter("Flags", "The combination of spawn flags to apply (RoleSpawnFlags) - UseSpawnpoint, AssignInventory, All, None")]
        public static ActionResultFlags SetRole(ref ActionContext context)
        {
            context.EnsureCompiled((index, p) =>
            {
                return index switch
                {
                    0 => p.EnsureCompiled(Enum.TryParse, RoleTypeId.None),
                    1 => p.EnsureCompiled(Enum.TryParse, RoleSpawnFlags.All),
                    
                    _ => false
                };
            });

            var type = context.GetValue<RoleTypeId>(0);
            var flags = context.GetValue<RoleSpawnFlags>(1);
            
            context.Player.Role.Set(type, RoleChangeReason.RemoteAdmin, flags);
            return ActionResultFlags.SuccessDispose;
        }

        /// <summary>
        /// Clears all queued broadcasts for the player.
        /// </summary>
        /// <remarks>This method removes all messages in the broadcast queue for the player associated with the provided context, ensuring that no further broadcasts are displayed.</remarks>
        /// <param name="context">A reference to the action context containing the target player whose broadcast queue will be cleared.</param>
        /// <returns>An ActionResultFlags value indicating the result of the action. Returns SuccessDispose if the operation was successfully completed and resources can be released.</returns>
        [Action("ClearBroadcasts", "Clears the player's broadcast queue.")]
        public static ActionResultFlags ClearBroadcasts(ref ActionContext context)
        {
            context.Player.ClearBroadcasts();
            return ActionResultFlags.SuccessDispose;
        }

        /// <summary>
        /// Sends a broadcast message to the specified player with a custom text and duration.
        /// </summary>
        /// <remarks>
        /// This method retrieves the broadcast message and its duration from the provided action context,
        /// validates the parameters, and sends the broadcast to the target player.
        /// </remarks>
        /// <param name="context">A reference to the action context containing parameters for the broadcast,
        /// including the message text and its duration in seconds.</param>
        /// <returns>An ActionResultFlags value indicating the result of the operation. Returns SuccessDispose
        /// if the broadcast was successfully sent and resources can be disposed of.</returns>
        [Action("Broadcast", "Sends a broadcast to the player.")]
        [ActionParameter("Message", "The text of the broadcast.")]
        [ActionParameter("Duration", "The duration of the message (in seconds).")]
        public static ActionResultFlags Broadcast(ref ActionContext context)
        {
            context.EnsureCompiled((index, p) =>
            {
                return index switch
                {
                    0 => p.EnsureCompiled(string.Empty),
                    1 => p.EnsureCompiled(ushort.TryParse, (ushort)0),
                    
                    _ => false
                };
            });
            
            var msg = context.GetValue(0);
            var duration = context.GetValue<ushort>(1);

            context.Player.SendBroadcast(msg, duration);
            return ActionResultFlags.SuccessDispose;
        }

        /// <summary>
        /// Sends a hint message to a player with specified text, display duration, and priority.
        /// </summary>
        /// <remarks>
        /// The method processes parameters from the provided <paramref name="context"/> to retrieve the message content,
        /// duration (in seconds), and priority status. It ensures that the hint is displayed to the player accordingly.
        /// </remarks>
        /// <param name="context">A reference to the action context containing the required parameters, including the
        /// hint message, duration of the hint, and its priority.</param>
        /// <returns>An ActionResultFlags value indicating the outcome of the operation. Typically returns SuccessDispose upon
        /// successful execution, allowing resources to be released.</returns>
        [Action("Hint", "Sends a hint to the player.")]
        [ActionParameter("Message", "The text of the hint.")]
        [ActionParameter("Duration", "The duration of the message (in seconds).")]
        [ActionParameter("Priority", "Whether or not to show this message before the queue.")]
        public static ActionResultFlags Hint(ref ActionContext context)
        {
            context.EnsureCompiled((index, p) =>
            {
                return index switch
                {
                    0 => p.EnsureCompiled(string.Empty),
                    1 => p.EnsureCompiled(ushort.TryParse, (ushort)0),
                    2 => p.EnsureCompiled(bool.TryParse, false),
                    
                    _ => false
                };
            });
            
            var msg = context.GetValue(0);
            var duration = context.GetValue<ushort>(1);
            var priority = context.GetValue<bool>(2);

            context.Player.ShowHint(msg, duration, priority);
            return ActionResultFlags.SuccessDispose;
        }

        /// <summary>
        /// Sends an alert hint to the player, displaying a message of a specified type for a defined duration.
        /// </summary>
        /// <remarks>
        /// The method retrieves the alert type, duration, and message from the provided action context
        /// and validates them. If the parameters are not valid, the method will terminate without processing
        /// further actions. Otherwise, the alert will be sent to the target player.
        /// </remarks>
        /// <param name="context">A reference to the action context containing the parameters for the alert: the alert type, duration, and message content.</param>
        /// <returns>An ActionResultFlags value indicating the outcome of the method. Returns SuccessDispose when the alert is successfully sent or if the operation completes without errors and resources can be released.</returns>
        [Action("Alert", "Sends an alert hint to the player.")]
        [ActionParameter("Type", "The type of the alert (Info / Warn).")]
        [ActionParameter("Duration", "The duration of the alert (in seconds).")]
        [ActionParameter("Message", "The content of the alert.")]
        public static ActionResultFlags Alert(ref ActionContext context)
        {
            context.EnsureCompiled((index, p) =>
            {
                return index switch
                {
                    0 => p.EnsureCompiled(Enum.TryParse, AlertType.Info),
                    1 => p.EnsureCompiled(float.TryParse, 5f),
                    2 => p.EnsureCompiled(string.Empty),

                    _ => false
                };
            });
            
            var type = context.GetValue<AlertType>(0);
            var duration = context.GetValue<float>(1);
            var msg = context.GetValue(2);

            if (duration <= 0f || string.IsNullOrEmpty(msg) || context.Player == null)
                return ActionResultFlags.SuccessDispose;
            
            context.Player.SendAlert(type, duration, msg, true);
            return ActionResultFlags.SuccessDispose;
        }
    }
}