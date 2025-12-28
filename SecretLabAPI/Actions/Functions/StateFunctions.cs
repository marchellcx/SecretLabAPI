using SecretLabAPI.Actions.API;
using SecretLabAPI.Actions.Enums;
using SecretLabAPI.Actions.Attributes;

using LabExtended.Events;

namespace SecretLabAPI.Actions.Functions
{
    /// <summary>
    /// Provides static methods for retrieving and managing state variables, either scoped to the current round or
    /// persisted across rounds.
    /// </summary>
    /// <remarks>This class is intended for use within action workflows that require access to temporary or
    /// persistent state data. All members are static and thread safety is not guaranteed; callers should ensure
    /// appropriate synchronization if accessing from multiple threads.</remarks>
    public static class StateFunctions
    {
        private static readonly Dictionary<string, object> roundStates = new();
        private static readonly Dictionary<string, object> persistentStates = new();

        /// <summary>
        /// Retrieves the value of a state variable identified by a key, either for the current round or persistently,
        /// and stores it in the action context memory.
        /// </summary>
        /// <remarks>If the specified key is null, empty, or not found in the selected state dictionary,
        /// no value is stored in the context memory and the operation is considered unsuccessful. The 'Round' parameter
        /// determines whether the state is retrieved from the round-specific or persistent storage.</remarks>
        /// <param name="context">A reference to the current action context. The context provides access to input parameters and is used to
        /// store the retrieved state value.</param>
        /// <returns>An ActionResultFlags value indicating the outcome of the operation. Returns SuccessDispose if the state
        /// variable is found and stored; otherwise, returns StopDispose.</returns>
        [Action("GetState", "Retrieves a round/persistent state variable.")]
        [ActionParameter("Key", "The name of the variable key.")]
        [ActionParameter("Round", "Whether or not the variable should be saved for this round only (true / false - defaults to false).")]
        public static ActionResultFlags GetState(ref ActionContext context)
        {
            context.EnsureCompiled((i, p) =>
            {
                return i switch
                {
                    0 => p.EnsureCompiled(string.Empty),
                    1 => p.EnsureCompiled(bool.TryParse, false),

                    _ => false
                };
            });

            var key = context.GetValue(0);
            var round = context.GetValue<bool>(1);
            var dict = round ? roundStates : persistentStates;
                
            if (string.IsNullOrEmpty(key))
                return ActionResultFlags.StopDispose;

            if (!dict.TryGetValue(key, out var value))
                return ActionResultFlags.StopDispose;

            context.SetMemory(value);
            return ActionResultFlags.SuccessDispose;
        }

        /// <summary>
        /// Sets a state variable with the specified key, either for the current round or persistently across rounds.
        /// </summary>
        /// <remarks>If the round parameter is set to <see langword="true"/>, the variable is stored only
        /// for the current round; otherwise, it is stored persistently. The key parameter must not be null or empty, or
        /// the operation will not complete successfully.</remarks>
        /// <param name="context">A reference to the current action context containing the parameters for the operation. The context must
        /// provide a string key, a Boolean indicating round scope, and the variable value to store.</param>
        /// <returns>An ActionResultFlags value indicating the outcome of the operation. Returns SuccessDispose if the variable
        /// was set successfully; otherwise, returns StopDispose if the key is null or empty.</returns>
        [Action("SetState", "Sets a round/persistent state variable.")]
        [ActionParameter("Key", "The key to save the variable under.")]
        [ActionParameter("Round", "Whether or not the variable should be saved for this round only (true / false - defaults to false).")]
        [ActionParameter("Variable", "The memory variable to save.")]
        public static ActionResultFlags SetState(ref ActionContext context)
        {
            context.EnsureCompiled((i, p) =>
            {
                return i switch
                {
                    0 => p.EnsureCompiled(string.Empty),
                    1 => p.EnsureCompiled(bool.TryParse, false),
                    2 => p.EnsureCompiled(string.Empty),

                    _ => false
                };
            });

            var key = context.GetValue(0);
            var round = context.GetValue<bool>(1);
            var value = context.GetValue<object>(2);
            var dict = round ? roundStates : persistentStates;

            if (string.IsNullOrEmpty(key))
                return ActionResultFlags.StopDispose;

            dict[key] = value;
            return ActionResultFlags.SuccessDispose;
        }

        /// <summary>
        /// Removes a state variable identified by the specified key from either the round or persistent state
        /// collection.
        /// </summary>
        /// <param name="context">A reference to the current action context containing the parameters for the operation. The first parameter
        /// must be the variable key as a string; the optional second parameter specifies whether to remove from round
        /// state (<see langword="true"/>) or persistent state (<see langword="false"/>; the default).</param>
        /// <returns>An <see cref="ActionResultFlags"/> value indicating the result of the operation. Returns <see
        /// cref="ActionResultFlags.SuccessDispose"/> if the variable was removed; otherwise, <see
        /// cref="ActionResultFlags.StopDispose"/> if the key is null or empty.</returns>
        [Action("RemoveState", "Removes a round/persistent state variable.")]
        [ActionParameter("Key", "The name of the variable key.")]
        [ActionParameter("Round", "Whether or not the variable should be removed from round only states (true / false - defaults to false).")]
        public static ActionResultFlags RemoveState(ref ActionContext context)
        {
            context.EnsureCompiled((i, p) =>
            {
                return i switch
                {
                    0 => p.EnsureCompiled(string.Empty),
                    1 => p.EnsureCompiled(bool.TryParse, false),

                    _ => false
                };
            });

            var key = context.GetValue(0);
            var round = context.GetValue<bool>(1);
            var dict = round ? roundStates : persistentStates;

            if (string.IsNullOrEmpty(key))
                return ActionResultFlags.StopDispose;

            dict.Remove(key);
            return ActionResultFlags.SuccessDispose;
        }

        internal static void Initialize()
        {
            ExRoundEvents.Restarting += roundStates.Clear;
        }
    }
}
