namespace SecretLabAPI.Features.Data
{
    /// <summary>
    /// Used to provide setting entries for different data collection sources.
    /// </summary>
    public struct DataCollectionEntry
    {
        /// <summary>
        /// The ID of the source.
        /// </summary>
        public readonly string Id;

        /// <summary>
        /// The user-friendly name of the source.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// The description of the source.
        /// </summary>
        public readonly string Description;

        /// <summary>
        /// Initializes a new instance of the DataCollectionEntry class with the specified identifier, name, and
        /// description.
        /// </summary>
        /// <param name="id">The unique identifier for the data collection entry. Cannot be null or empty.</param>
        /// <param name="name">The display name of the data collection entry. Cannot be null or empty.</param>
        /// <param name="description">A description of the data collection entry. Can be null or empty if no description is available.</param>
        public DataCollectionEntry(string id, string name, string description)
        {
            Id = id;
            Name = name;
            Description = description;
        }
    }
}