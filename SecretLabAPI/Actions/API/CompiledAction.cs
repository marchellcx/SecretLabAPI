namespace SecretLabAPI.Actions.API
{
    /// <summary>
    /// Represents a compiled action with its method and parameters.
    /// </summary>
    public class CompiledAction
    {
        /// <summary>
        /// The target method.
        /// </summary>
        public ActionMethod Action { get; }

        /// <summary>
        /// The array of parameters for the action.
        /// </summary>
        public CompiledParameter[] Parameters { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the parameters have been compiled.
        /// </summary>
        public bool ParametersCompiled { get; set; }

        /// <summary>
        /// Gets or sets the name of the output variable, if any.
        /// </summary>
        public string? OutputVariableName { get; set; }

        /// <summary>
        /// A dictionary for custom metadata associated with the compiled action.
        /// </summary>
        public Dictionary<string, object> Metadata { get; } = new();

        /// <summary>
        /// Initializes a new instance of the CompiledAction class with the specified action method and parameters.
        /// </summary>
        /// <param name="action">The action method to be executed. Cannot be null.</param>
        /// <param name="parameters">An array of compiled parameters to be passed to the action method. Cannot be null.</param>
        public CompiledAction(ActionMethod action, CompiledParameter[] parameters, string? outputVariable)
        {
            Action = action;
            Parameters = parameters;
            OutputVariableName = outputVariable;
        }
    }
}