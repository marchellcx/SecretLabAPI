using LabExtended.API;
using LabExtended.API.Hints;

using LabExtended.Events;
using LabExtended.Utilities;
using LabExtended.Extensions;
using LabExtended.Core.Storage;

using SecretLabAPI.Data;

using SecretLabAPI.Levels.IO;
using SecretLabAPI.Levels.Events;
using SecretLabAPI.Levels.Interfaces;

using SecretLabAPI.Elements.Levels;

using System.Reflection;

using LabApi.Loader;

using Utils.NonAllocLINQ;

namespace SecretLabAPI.Levels
{
    /// <summary>
    /// Manages player levels, experience points, and level rewards within the game.
    /// </summary>
    public static class LevelManager
    {
        public const string DataEntry = "LevelManager";

        internal static LevelConfig config;
        internal static StorageInstance storage;

        /// <summary>
        /// Occurs when a player's data is removed either due to leaving the server -or- due to enabling Do Not Track.
        /// </summary>
        public static event Action<ExPlayer>? DataRemoved;

        /// <summary>
        /// Occurs when level data has been loaded for a player.
        /// </summary>
        public static event Action<ExPlayer, LevelData>? DataLoaded;

        /// <summary>
        /// Occurs when experience is about to be added, allowing handlers to inspect or modify the experience addition
        /// process.
        /// </summary>
        public static event Action<AddingExperienceEventArgs>? AddingExperience;

        /// <summary>
        /// Occurs when experience points have been added.
        /// </summary>
        public static event Action<AddedExperienceEventArgs>? AddedExperience;

        /// <summary>
        /// Occurs when points are about to be added, allowing handlers to inspect or modify the operation.
        /// </summary>
        public static event Action<AddingPointsEventArgs>? AddingPoints;

        /// <summary>
        /// Occurs when points are added to the system.
        /// </summary>
        public static event Action<AddedPointsEventArgs>? AddedPoints;

        /// <summary>
        /// Occurs when a leveling up event is triggered.
        /// </summary>
        public static event Action<LevelingUpEventArgs>? LevelingUp;

        /// <summary>
        /// Occurs when a level-up event is triggered, providing details about the level change.
        /// </summary>
        public static event Action<LeveledUpEventArgs>? LeveledUp;

        public static string[] LevelToString { get; private set; }

        /// <summary>
        /// Gets the experience point thresholds required to reach each level.
        /// </summary>
        /// <remarks>The array contains the cumulative experience required for each level, where each
        /// element corresponds to a specific level index. The first element typically represents the experience needed
        /// to reach level 1, the second for level 2, and so on. The array is read-only after initialization.</remarks>
        public static int[] ExperiencePerLevel { get; private set; }

        /// <summary>
        /// Gets a list of all registered level rewards.
        /// </summary>
        public static List<ILevelReward> Rewards { get; } = new();

        /// <summary>
        /// Gets a dictionary of all loaded player levels.
        /// </summary>
        public static Dictionary<ExPlayer, LevelData> Levels { get; } = new();

        /// <summary>
        /// Scans the specified assembly for concrete types implementing the ILevelReward interface and adds them as
        /// rewards. Returns the number of reward types successfully added.
        /// </summary>
        /// <remarks>Only non-abstract, non-interface types with a public parameterless constructor that
        /// implement ILevelReward are considered. Types that are the LevelReward base class itself are
        /// excluded.</remarks>
        /// <param name="assembly">The assembly to scan for eligible ILevelReward implementations. Cannot be null.</param>
        /// <returns>The number of reward types that were successfully added from the specified assembly.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the assembly parameter is null.</exception>
        public static int AddRewards(Assembly assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));

            var rewardTypes = assembly.GetTypes();
            var addedCount = 0;

            foreach (var type in rewardTypes)
            {
                if (type.IsAbstract || type.IsInterface || type == typeof(LevelReward))
                    continue;

                if (!typeof(ILevelReward).IsAssignableFrom(type))
                    continue;

                var constructor = type.GetConstructor(Type.EmptyTypes);

                if (constructor == null || !constructor.IsPublic)
                    continue;

                if (AddReward(type) != null)
                    addedCount++;
            }

            return addedCount;
        }

        /// <summary>
        /// Adds a new reward of the specified type to the collection of level rewards.
        /// </summary>
        /// <remarks>The specified type must have a public parameterless constructor. The method creates
        /// an instance of the specified type and adds it as a reward. This method is typically used to register custom
        /// reward types at runtime.</remarks>
        /// <param name="type">The type of the reward to add. Must implement the ILevelReward interface and have a public parameterless
        /// constructor.</param>
        /// <returns>An instance of ILevelReward representing the added reward.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="type"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="type"/> does not implement the ILevelReward interface.</exception>
        public static ILevelReward AddReward(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (!typeof(ILevelReward).IsAssignableFrom(type))
                throw new ArgumentException("Type must implement ILevelReward interface.", nameof(type));

            var reward = (ILevelReward)Activator.CreateInstance(type)!;
            return AddReward(reward);
        }

        /// <summary>
        /// Creates and adds a new reward of the specified type to the collection.
        /// </summary>
        /// <typeparam name="T">The type of reward to add. Must implement the ILevelReward interface and have a parameterless constructor.</typeparam>
        /// <returns>The newly created reward of type T after it has been added to the collection.</returns>
        public static T AddReward<T>() where T : ILevelReward, new()
            => (T)AddReward(new T());

        /// <summary>
        /// Adds the specified reward to the collection of level rewards if it is not already present and enables it.
        /// </summary>
        /// <param name="reward">The reward to add and enable. Cannot be null.</param>
        /// <returns>The reward that was added or, if the reward was already present, the existing reward instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="reward"/> is null.</exception>
        public static ILevelReward AddReward(ILevelReward reward)
        {
            if (reward == null)
                throw new ArgumentNullException(nameof(reward));

            if (Rewards.Contains(reward))
                return reward;

            Rewards.Add(reward);

            reward.EnableReward();
            return reward;
        }

        /// <summary>
        /// Retrieves the saved level data associated with the specified player.
        /// </summary>
        /// <param name="player">The player for whom to retrieve level data. Cannot be null and must have a valid ReferenceHub.</param>
        /// <returns>The level data associated with the specified player.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the player is null or does not have a valid ReferenceHub.</exception>
        /// <exception cref="Exception">Thrown if the player does not have any saved level data.</exception>
        public static LevelData? GetLevelData(this ExPlayer player)
        {
            if (player?.ReferenceHub == null)
                throw new ArgumentNullException(nameof(player));

            if (!Levels.TryGetValue(player, out var data))
                return null;

            return data;
        }

        /// <summary>
        /// Adds the specified number of points to the player's level reward progress.
        /// </summary>
        /// <param name="player">The player to whom the points will be added. Cannot be null and must have a valid reference hub.</param>
        /// <param name="reward">The level reward to which the points are applied. Cannot be null.</param>
        /// <param name="points">The number of points to add. Must be greater than or equal to 1.</param>
        /// <returns>true if the points were successfully added; otherwise, false.</returns>
        public static bool AddPoints(this ExPlayer player, ILevelReward reward, int points)
        {
            if (player?.ReferenceHub == null)
                return false;

            if (reward == null)
                return false;

            if (points < 1)
                return false;

            if (!Levels.TryGetValue(player, out var data))
                return false;

            var addingPoints = new AddingPointsEventArgs(player, reward, points);

            if (!AddingPoints.InvokeBooleanEvent(addingPoints))
                return false;

            data.Points += addingPoints.Amount;

            AddedPoints.InvokeEvent(new(player, reward, addingPoints.Amount));
            return true;
        }

        /// <summary>
        /// Determines whether the specified player has at least the given number of points.
        /// </summary>
        /// <param name="player">The player whose points are to be checked. Cannot be null.</param>
        /// <param name="points">The minimum number of points required. Must be greater than or equal to 1.</param>
        /// <returns>true if the player has at least the specified number of points; otherwise, false.</returns>
        public static bool HasPoints(this ExPlayer player, int points)
        {
            if (player?.ReferenceHub == null)
                return false;

            if (points < 1)
                return false;

            if (!Levels.TryGetValue(player, out var data))
                return false;

            return data.Points >= points;
        }

        /// <summary>
        /// Attempts to subtract the specified number of points from the player's current point total.
        /// </summary>
        /// <param name="player">The player from whom points will be subtracted. Cannot be null and must have a valid reference hub.</param>
        /// <param name="points">The number of points to subtract. Must be greater than or equal to 1 and less than or equal to the player's
        /// current point total.</param>
        /// <returns>true if the points were successfully subtracted; otherwise, false.</returns>
        public static bool SubtractPoints(this ExPlayer player, int points)
        {
            if (player?.ReferenceHub == null)
                return false;

            if (points < 1)
                return false;

            if (!Levels.TryGetValue(player, out var data))
                return false;

            if (data.Points < points)
                return false;

            data.Points -= points;
            return true;
        }

        /// <summary>
        /// Attempts to add the specified amount of experience to the player and applies any associated level rewards.
        /// </summary>
        /// <remarks>If the experience addition results in a level change, the player's level is updated
        /// and relevant level-up events are triggered. No experience is added if the player, reward, or experience
        /// amount is invalid, or if event handlers prevent the operation.</remarks>
        /// <param name="player">The player to whom experience will be added. Cannot be null and must have a valid reference hub.</param>
        /// <param name="reward">The level reward to associate with the experience gain. Cannot be null.</param>
        /// <param name="experience">The amount of experience to add. Must be greater than zero.</param>
        /// <returns>true if experience was successfully added to the player; otherwise, false.</returns>
        public static bool AddExperience(this ExPlayer player, ILevelReward reward, int experience)
        {
            if (player?.ReferenceHub == null)
                return false;

            if (reward == null)
                return false;

            if (experience < 1)
                return false;

            if (!Levels.TryGetValue(player, out var data))
                return false;

            var addingExperience = new AddingExperienceEventArgs(player, reward, experience);

            if (!AddingExperience.InvokeBooleanEvent(addingExperience))
                return false;

            if (addingExperience.Amount > 0)
            {
                data.Experience += addingExperience.Amount;

                AddedExperience.InvokeEvent(new(player, reward, addingExperience.Amount));

                var newLevel = LevelAtExp(data.Experience);

                if (data.Level != newLevel)
                {
                    var levelingUp = new LevelingUpEventArgs(player, reward, data.Level, newLevel);

                    if (!LevelingUp.InvokeBooleanEvent(levelingUp))
                        return true;

                    data.Level = (byte)levelingUp.NextLevel;
                    data.RequiredExperience = ExperienceForLevel(levelingUp.NextLevel + 1);

                    LeveledUp.InvokeEvent(new(player, reward, data.Level, levelingUp.NextLevel));
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the current level of the specified player.
        /// </summary>
        /// <param name="player">The player whose level is to be retrieved. Cannot be null.</param>
        /// <returns>The level of the player as a byte value.</returns>
        public static byte GetLevel(this ExPlayer player)
            => player.GetLevelData()?.Level ?? 0;

        /// <summary>
        /// Gets the current number of points associated with the specified player.
        /// </summary>
        /// <param name="player">The player whose points are to be retrieved. Cannot be null.</param>
        /// <returns>The number of points the player currently has.</returns>
        public static int GetPoints(this ExPlayer player)
            => player.GetLevelData()?.Points ?? 0;

        /// <summary>
        /// Gets the current experience points for the specified player.
        /// </summary>
        /// <param name="player">The player whose experience points are to be retrieved. Cannot be null.</param>
        /// <returns>The total experience points accumulated by the player.</returns>
        public static int GetExperience(this ExPlayer player)
            => player.GetLevelData()?.Experience ?? 0;

        /// <summary>
        /// Determines the player level corresponding to a given amount of experience points.
        /// </summary>
        /// <param name="expAmount">The total experience points earned by the player.</param>
        /// <returns>The player level that matches the provided experience points, or 0 if the experience is insufficient for the first level.</returns>
        public static byte LevelAtExp(int expAmount)
        {
            for (var x = 1; x < config.Cap; x++)
            {
                if (ExperiencePerLevel[x] > expAmount)
                {
                    return (byte)(x - 1);
                }
            }

            return 0;
        }

        /// <summary>
        /// Retrieves the required experience points needed to reach the specified level.
        /// </summary>
        /// <param name="level">The target level for which the experience points are being queried.</param>
        /// <returns>The experience points required to reach the specified level. Returns 0 if the level exceeds the level cap.</returns>
        public static int ExperienceForLevel(int level)
        {
            if (level < 1)
                throw new ArgumentOutOfRangeException(nameof(level));

            if (level > config.Cap)
                return -1;

            return ExperiencePerLevel[level];
        }

        private static void GenerateLevels()
        {
            ExperiencePerLevel = new int[config.Cap];
            ExperiencePerLevel[0] = 0;

            LevelToString = new string[config.Cap];
            LevelToString[0] = "Lvl0";

            var exp = 0;
            var step = config.Step;

            for (var x = 1; x < config.Cap; x++)
            {
                LevelToString[x] = $"Lvl{x}";

                if (config.Offsets.TryGetValue((byte)x, out var offset))
                    step += offset;

                exp += step;

                ExperiencePerLevel[x] = exp;
            }
        }

        private static void LoadLevel(ExPlayer player, bool refresh)
        {
            if (player?.ReferenceHub == null)
                return;

            if (string.IsNullOrEmpty(player.UserId))
                return;

            if (player.DoNotTrack && !player.CanCollect(DataEntry))
            {
                if (Levels.Remove(player) || storage.Remove(player.UserId, true))
                    DataRemoved?.Invoke(player);

                player.RemoveHintElement<LevelOverlay>();
                return;
            }

            if (!refresh && Levels.ContainsKey(player))
                return;

            Levels.Remove(player);

            storage.Remove(player.UserId, false);

            var data = storage.GetOrAdd(player.UserId, () => new LevelData());

            data.RequiredExperience = ExperienceForLevel(data.Level + 1);

            Levels[player] = data;

            if (!player.TryGetHintElement<LevelOverlay>(out var overlay))
                player.AddHintElement(overlay = new());

            overlay.Level = data;
            overlay.RefreshBar();

            DataLoaded?.Invoke(player, data);
        }

        private static void OnLeft(ExPlayer player)
        {
            Levels.Remove(player);

            storage.Remove(player.UserId, false);

            DataRemoved?.Invoke(player);
        }

        private static void OnVerified(ExPlayer player)
        {
            LoadLevel(player, false);
        }

        private static void OnToggled(ExPlayer player, DataCollectionEntry entry, bool isEnabled)
        {
            if (player?.ReferenceHub == null)
                return;

            if (entry.Id != DataEntry)
                return;

            LoadLevel(player, true);
        }

        internal static void Initialize()
        {
            new DataCollectionEntry(
                DataEntry,
                "<color=red>📊</color> | <b>Level Systém</b></color>",
                "Ukládá počet dosažených XP a levelů pomocí identifikátoru vašeho účtu <i>(SteamID64 pro Steam účty a Discord ID pro Discord účty)</i>.")
                .AddEntry();

            config = FileUtils.LoadYamlFileOrDefault(SecretLab.RootDirectory, "level_manager.yml", new LevelConfig(), true);
            storage = StorageManager.CreateStorage("LevelManager", config.SharedStorage);

            GenerateLevels();

            DataCollection.EntryToggled += OnToggled;

            ExPlayerEvents.Left += OnLeft;
            ExPlayerEvents.Verified += OnVerified;

            PluginLoader.Plugins.ForEachValue(asm => AddRewards(asm));
        }
    }
}