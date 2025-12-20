using SecretLabAPI.Actions.API;

using SecretLabAPI.Actions.Enums;
using SecretLabAPI.Actions.Attributes;

namespace SecretLabAPI.Levels
{
    /// <summary>
    /// Provides static methods to perform various game-related actions at the level,
    /// such as modifying player experience points.
    /// </summary>
    public static class LevelActions
    {
        /// <summary>
        /// Removes a specific amount of experience points from the player.
        /// </summary>
        /// <param name="context">The action context containing the parameters, including the reason for the removal and the amount of experience points to remove.</param>
        /// <returns>An ActionResultFlags value indicating the result of the action, such as success or failure.</returns>
        [Action("RemoveExperience", "Removes a specific amount of experience points from the player.")]
        [ActionParameter("Reason", "The reason for the removal.")]
        [ActionParameter("Amount", "The amount of experience points to remove.")]
        public static ActionResultFlags RemoveExperience(ref ActionContext context)
        {
            context.EnsureCompiled((index, p) =>
            {
                return index switch
                {
                    0 => p.EnsureCompiled(string.Empty),
                    1 => p.EnsureCompiled(int.TryParse, 0),
                    
                    _ => false
                };
            });

            var reason = context.GetValue(0);
            var xp = context.GetValue<int>(1);

            if (xp <= 0 || context.Player == null)
                return ActionResultFlags.SuccessDispose;

            context.Player.SubtractExperience(reason, xp);
            return ActionResultFlags.SuccessDispose;
        }
        
        /// <summary>
        /// Adds a specific amount of experience points to the player.
        /// </summary>
        /// <param name="context">The action context containing the parameters and the current player state.</param>
        /// <returns>An ActionResultFlags value indicating the status/result of the action.</returns>
        [Action("AddExperience", "Adds a specific amount of experience points to the player.")]
        [ActionParameter("Reason", "The reason for the reward.")]
        [ActionParameter("Amount", "The amount of experience points to grant.")]
        public static ActionResultFlags AddExperience(ref ActionContext context)
        {
            context.EnsureCompiled((index, p) =>
            {
                return index switch
                {
                    0 => p.EnsureCompiled(string.Empty),
                    1 => p.EnsureCompiled(int.TryParse, 0),
                    
                    _ => false
                };
            });

            var reason = context.GetValue(0);
            var xp = context.GetValue<int>(1);

            if (xp <= 0 || context.Player == null)
                return ActionResultFlags.SuccessDispose;

            context.Player.AddExperience(reason, xp);
            return ActionResultFlags.SuccessDispose;
        }
    }
}