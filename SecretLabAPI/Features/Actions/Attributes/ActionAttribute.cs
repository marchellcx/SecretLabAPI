namespace SecretLabAPI.Features.Actions.Attributes
{
    /// <summary>
    /// Marks methods as actions.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class ActionAttribute : Attribute
    {
        /// <summary>
        /// The ID of the action.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// The description of the action.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Whether or not this function is an evaluator (returns a value).
        /// </summary>
        public bool IsEvaluator { get; }

        /// <summary>
        /// Whether or not to save arguments that overflow the defined parameters.
        /// </summary>
        public bool SaveArgumentsOverflow { get; }
        
        /// <summary>
        /// Initializes a new instance of the ActionAttribute class with the specified identifier and optional
        /// description.
        /// </summary>
        /// <param name="id">The unique identifier for the action. Cannot be null.</param>
        /// <param name="description">An optional description of the action. If not specified, the description is set to an empty string.</param>
        public ActionAttribute(string id, string description = "", bool isEvaluator = false, bool argsOverflow = false)
        {
            Id = id;
            Description = description;
            IsEvaluator = isEvaluator;
            SaveArgumentsOverflow = argsOverflow;
        }
    }
}