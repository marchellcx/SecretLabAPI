using LabExtended.API;
using LabExtended.API.Settings;

namespace SecretLabAPI.Features.Data
{
    /// <summary>
    /// Manages data collection for players with Do Not Track enabled.
    /// </summary>
    public static class DataCollection
    {
        private static readonly List<DataCollectionEntry> entries = new();

        /// <summary>
        /// Gets a list of all registered entries.
        /// </summary>
        public static IReadOnlyList<DataCollectionEntry> Entries => field ??= entries.AsReadOnly();

        /// <summary>
        /// Gets called once an entry is toggled for a player.
        /// </summary>
        public static event Action<ExPlayer, DataCollectionEntry, bool>? EntryToggled;

        /// <summary>
        /// Adds a new entry to the data collection.
        /// </summary>
        /// <param name="entry">The entry to add to the collection. The entry's <see cref="DataCollectionEntry.Id"/> must be unique within
        /// the collection.</param>
        /// <exception cref="ArgumentException">Thrown if an entry with the same <see cref="DataCollectionEntry.Id"/> already exists in the collection.</exception>
        public static void AddEntry(this DataCollectionEntry entry)
        {
            if (entries.Any(e => e.Id == entry.Id))
                throw new ArgumentException($"An entry with the ID '{entry.Id}' is already registered.");

            entries.Add(entry);

            if (entries.Count == 1 && !SettingsManager.HasBuilder("DataCollectionBuilder"))
            {
                SettingsManager.AddBuilder(new SettingsBuilder("DataCollectionBuilder")
                    .WithMenu(() => new DataCollectionMenu())
                    .WithPredicate(player => player.DoNotTrack));             
            }
        }

        /// <summary>
        /// Determines whether the specified player has permitted data collection for the given entry identifier.
        /// </summary>
        /// <remarks>If the player has not set up their data collection preferences or the entry
        /// identifier is not recognized, the method returns false. This method respects the player's Do Not Track
        /// setting and individual entry permissions.</remarks>
        /// <param name="player">The player whose data collection preferences are to be checked. Cannot be null and must have a valid
        /// ReferenceHub.</param>
        /// <param name="entryId">The identifier of the data collection entry to check. Cannot be null or empty.</param>
        /// <returns>true if the player has allowed data collection for the specified entry; otherwise, false.</returns>
        public static bool CanCollect(this ExPlayer player, string entryId)
        {
            if (player?.ReferenceHub == null)
                return false;

            if (string.IsNullOrEmpty(entryId))
                return false;

            if (!player.DoNotTrack)
                return true;

            // Lets return false if the player has not set up their data collection preferences.
            if (!player.TryGetMenu<DataCollectionMenu>(out var collectionMenu))
                return false;

            if (!collectionMenu.Buttons.TryGetValue(entryId, out var button))
                return false;

            // Button A is active means the player has allowed data collection for this entry.
            return button.IsAButtonActive;
        }

        internal static void OnToggled(ExPlayer player, DataCollectionEntry entry, bool isAllowed)
        {
            EntryToggled?.Invoke(player, entry, isAllowed);
        }
    }
}