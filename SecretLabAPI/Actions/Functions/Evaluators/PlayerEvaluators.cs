using LabExtended.Extensions;

using SecretLabAPI.Actions.API;
using SecretLabAPI.Actions.Enums;
using SecretLabAPI.Actions.Attributes;

namespace SecretLabAPI.Actions.Functions.Evaluators
{
    /// <summary>
    /// Provides evaluators for player-related actions.
    /// </summary>
    public static class PlayerEvaluators
    {
        /// <summary>
        /// Checks whether or not a player has at least a specific amount of a specified item.
        /// </summary>
        /// <param name="context">The action context that provides the player's current state and parameters for the evaluation.</param>
        /// <returns>
        /// An <see cref="ActionResultFlags"/> value indicating the success or failure of the evaluation,
        /// as well as whether the context should be disposed of after execution.
        /// </returns>
        [Action("HasItems", "Checks whether or not a player has at least a specific amount of an item.")]
        [ActionParameter("Type", "The type of the item.")]
        [ActionParameter("Amount", "The minimum required amount of items.")]
        public static ActionResultFlags HasItems(ref ActionContext context)
        {
            context.EnsureCompiled((i, p) =>
            {
                return i switch
                {
                    0 => p.EnsureCompiled(Enum.TryParse, ItemType.None),
                    1 => p.EnsureCompiled(int.TryParse, 1),
                    
                    _ => false
                };
            });
            
            var type = context.GetValue<ItemType>(0);
            var amount = context.GetValue<int>(1);
            var result = false;

            if (context.Player?.ReferenceHub != null)
            {
                if (type != ItemType.None && amount > 0)
                {
                    if (type.IsAmmo())
                    {
                        result = context.Player.Ammo.HasAmmo(type, (ushort)amount);
                    }
                    else
                    {
                        result = context.Player.Inventory.HasItems(type, amount);
                    }
                }
            }

            context.SetMemory(result);
            return ActionResultFlags.SuccessDispose;
        }
    }
}