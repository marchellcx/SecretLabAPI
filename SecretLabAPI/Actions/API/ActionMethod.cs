namespace SecretLabAPI.Actions.API
{
    /// <summary>
    /// Represents a registered action method.
    /// </summary>
    public class ActionMethod
    {
        /// <summary>
        /// The ID of the action.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets the compiled delegate representing the action method.
        /// </summary>
        public ActionDelegate Delegate { get; }

        /// <summary>
        /// Gets the registered parameters.
        /// </summary>
        public ActionParameter[] Parameters { get; }

        /// <summary>
        /// Whether or not this action is an evaluator.
        /// </summary>
        public bool IsEvaluator { get; }

        /// <summary>
        /// Whether or not to save overflow arguments.
        /// </summary>
        public bool SaveArgumentsOverflow { get; }

        /// <summary>
        /// Represents a method capable of executing an action within the SecretLabAPI framework,
        /// including relevant metadata, parameters, and a delegate that performs the action logic.
        /// </summary>
        public ActionMethod(string id, bool isEvaluator, bool argsOverflow, ActionDelegate actionDelegate,
            ActionParameter[] parameters)
        {
            Id = id;
            IsEvaluator = isEvaluator;
            SaveArgumentsOverflow = argsOverflow;
            Delegate = actionDelegate;
            Parameters = parameters;
        }

        /// <summary>
        /// Creates a new instance of the ActionMethod class that is a copy of the current instance, including
        /// all its parameters and their associated values.
        /// </summary>
        /// <returns>A new ActionMethod object with the same values as the current instance.</returns>
        public ActionMethod Clone()
        {
            var parameters = new ActionParameter[Parameters.Length];

            for (var i = 0; i < Parameters.Length; i++)
                parameters[i] = Parameters[i].Clone();

            return new(Id, IsEvaluator, SaveArgumentsOverflow, Delegate, parameters);
        }
    }
}