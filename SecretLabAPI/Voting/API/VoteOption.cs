namespace SecretLabAPI.Voting.API
{
    /// <summary>
    /// Represents a selectable option in a voting system.
    /// </summary>
    public class VoteOption
    {
        /// <summary>
        /// Gets the name of the vote option.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Initializes a new instance of the VoteOption class with the specified option name.
        /// </summary>
        /// <param name="name">The name of the vote option. Cannot be null or empty.</param>
        /// <exception cref="ArgumentException">Thrown if the name parameter is null or empty.</exception>
        public VoteOption(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Vote option name cannot be null or empty.", nameof(name));

            Name = name;
        }
    }
}
