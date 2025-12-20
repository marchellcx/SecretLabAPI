using LabApi.Features.Wrappers;

using LabExtended.API;
using LabExtended.Core.Storage;

using LabExtended.Events;
using LabExtended.Extensions;

using SecretLabAPI.Data;

using System.Text;

using UnityEngine;

using SecretLabAPI.Levels.Storage;
using SecretLabAPI.Levels.Events;

namespace SecretLabAPI.Levels
{
    /// <summary>
    /// Manages player levels and experience points.
    /// </summary>
    public static class LevelManager
    {
        /// <summary>
        /// Gets the singleton instance of the storage.
        /// </summary>
        public static StorageInstance Storage { get; private set; }

        /// <summary>
        /// Gets the collection of saved levels, indexed by their user IDs.
        /// </summary>
        public static Dictionary<string, SavedLevel> Levels { get; } = new();

        /// <summary>
        /// Retrieves the saved level associated with the specified player.
        /// </summary>
        /// <param name="player">The player whose saved level is to be retrieved. Cannot be null.</param>
        /// <returns>The saved level of the player if it exists; otherwise, <see langword="null"/>.</returns>
        public static SavedLevel? GetSavedLevel(this Player player)
            => GetSavedLevel(player.UserId);

        /// <summary>
        /// Resets the player's level to the initial state.
        /// </summary>
        /// <param name="player">The player whose level is to be reset. Cannot be null.</param>
        /// <returns><see langword="true"/> if the level was successfully reset; otherwise, <see langword="false"/>.</returns>
        public static bool ResetLevel(this Player player)
            => ResetLevel(player.UserId);

        /// <summary>
        /// Retrieves the level of the specified player based on their user ID.
        /// </summary>
        /// <param name="player">The player whose level is to be retrieved. Cannot be null.</param>
        /// <returns>The level of the player as an integer.</returns>
        public static int GetLevel(this Player player)
            => GetLevel(player.UserId);

        /// <summary>
        /// Retrieves the total experience points accumulated by the specified player.
        /// </summary>
        /// <param name="player">The player whose experience points are to be retrieved. Cannot be null.</param>
        /// <returns>The total experience points of the player as a floating-point integer.</returns>
        public static float GetExperience(this Player player)
            => GetExperience(player.UserId);

        /// <summary>
        /// Adds a specified amount of experience to the player.
        /// </summary>
        /// <param name="player">The player to whom the experience will be added.</param>
        /// <param name="amount">The amount of experience to add. Must be a positive value.</param>
        /// <returns><see langword="true"/> if the experience was successfully added; otherwise, <see langword="false"/>.</returns>
        public static bool AddExperience(this Player player, string reason, int amount)
            => AddExperience(player.UserId, reason, amount);

        /// <summary>
        /// Subtracts a specified amount of experience from the player's total experience.
        /// </summary>
        /// <remarks>This method affects the player's experience points and may impact their level or
        /// abilities. Ensure that the <paramref name="amount"/> does not exceed the player's current
        /// experience.</remarks>
        /// <param name="player">The player from whom experience will be subtracted.</param>
        /// <param name="amount">The amount of experience to subtract. Must be a positive value.</param>
        /// <returns><see langword="true"/> if the experience was successfully subtracted; otherwise, <see langword="false"/>.</returns>
        public static bool SubtractExperience(this Player player, string reason, int amount)
            => SubtractExperience(player.UserId, reason, amount);

        /// <summary>
        /// Retrieves the saved level for a specified user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user whose saved level is to be retrieved. Cannot be null.</param>
        /// <returns>The <see cref="SavedLevel"/> associated with the specified user if found; otherwise, <see langword="null"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="userId"/> is null.</exception>
        public static SavedLevel? GetSavedLevel(string userId)
        {
            if (userId is null)
                throw new ArgumentNullException(nameof(userId));

            if (Levels.TryGetValue(userId, out var savedLevel))
                return savedLevel;

            return null;
        }

        /// <summary>
        /// Retrieves the level associated with the specified user identifier.
        /// </summary>
        /// <param name="userId">The unique identifier of the user whose level is to be retrieved. Cannot be null.</param>
        /// <returns>The level of the specified user if found; otherwise, 0.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="userId"/> is null.</exception>
        public static int GetLevel(string userId)
        {
            if (userId is null)
                throw new ArgumentNullException(nameof(userId));

            if (Levels.TryGetValue(userId, out var savedLevel))
                return savedLevel.Level;

            return 0;
        }

        /// <summary>
        /// Retrieves the experience points for a specified user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user whose experience points are to be retrieved. Cannot be null.</param>
        /// <returns>The total experience points of the user. Returns 0 if the user does not have any recorded experience.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="userId"/> is null.</exception>
        public static float GetExperience(string userId)
        {
            if (userId is null)
                throw new ArgumentNullException(nameof(userId));

            if (Levels.TryGetValue(userId, out var savedLevel))
                return savedLevel.Experience;

            return 0;
        }

        /// <summary>
        /// Sets the level for a specified user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user whose level is to be set. Cannot be null.</param>
        /// <param name="level">The new level to assign to the user.</param>
        /// <returns><see langword="true"/> if the user's level was successfully set; otherwise, <see langword="false"/> if the
        /// user does not exist.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="userId"/> is null.</exception>
        public static bool SetLevel(string userId, byte level)
        {
            if (userId is null)
                throw new ArgumentNullException(nameof(userId));

            if (!Levels.TryGetValue(userId, out var savedLevel))
                return false;

            var newExperience = LevelProgress.ExperienceForLevel(level);

            if (newExperience != savedLevel.Experience)
            {
                var changingExperienceArgs = new ChangingExperienceEventArgs(savedLevel, userId, "Command", savedLevel.Experience, newExperience);

                LevelEvents.OnChangingExperience(changingExperienceArgs);

                savedLevel.Experience = newExperience;

                LevelEvents.OnChangedExperience(new(savedLevel, userId, "Command", changingExperienceArgs.CurrentExp, savedLevel.Experience), changingExperienceArgs.target);
            }

            var changingLevelArgs = new ChangingLevelEventArgs(savedLevel, userId, "Command", savedLevel.Level, level);

            LevelEvents.OnChangingLevel(changingLevelArgs);

            savedLevel.Level = level;
            savedLevel.RequiredExperience = LevelProgress.ExperienceForLevel(level + 1);

            LevelEvents.OnChangedLevel(new(savedLevel, userId, "Command", changingLevelArgs.CurrentLevel, savedLevel.Level), changingLevelArgs.target);
            return true;
        }

        /// <summary>
        /// Sets the experience level for a specified user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user whose experience level is to be set. Cannot be <see langword="null"/>.</param>
        /// <param name="level">The experience level to assign to the user.</param>
        /// <returns><see langword="true"/> if the user's experience level was successfully set; otherwise, <see
        /// langword="false"/> if the user does not exist.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="userId"/> is <see langword="null"/>.</exception>
        public static bool SetExperience(string userId, int exp)
        {
            if (userId is null)
                throw new ArgumentNullException(nameof(userId));

            if (!Levels.TryGetValue(userId, out var savedLevel))
                return false;

            var newLevel = LevelProgress.LevelAtExp(exp);

            if (newLevel != savedLevel.Level)
            {
                var changingLevelArgs = new ChangingLevelEventArgs(savedLevel, userId, "Command", savedLevel.Level, newLevel);

                LevelEvents.OnChangingLevel(changingLevelArgs);

                savedLevel.Level = newLevel;
                savedLevel.RequiredExperience = LevelProgress.ExperienceForLevel(newLevel + 1);

                LevelEvents.OnChangedLevel(new(savedLevel, userId, "Command", changingLevelArgs.CurrentLevel, savedLevel.Level), changingLevelArgs.target);
            }

            var chaningExperienceArgs = new ChangingExperienceEventArgs(savedLevel, userId, "Command", savedLevel.Experience, exp);

            LevelEvents.OnChangingExperience(chaningExperienceArgs);

            savedLevel.Experience = exp;
            savedLevel.RequiredExperience = LevelProgress.ExperienceForLevel(savedLevel.Level + 1);

            LevelEvents.OnChangedExperience(new(savedLevel, userId, "Command", chaningExperienceArgs.CurrentExp, savedLevel.Experience), chaningExperienceArgs.target);
            return true;
        }

        /// <summary>
        /// Adds experience points to the specified user's level.
        /// </summary>
        /// <param name="userId">The unique identifier of the user to whom experience is being added. Cannot be <see langword="null"/>.</param>
        /// <param name="amount">The amount of experience points to add. Must be a positive value.</param>
        /// <returns><see langword="true"/> if the experience was successfully added; otherwise, <see langword="false"/> if the
        /// user does not exist.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="userId"/> is <see langword="null"/>.</exception>
        public static bool AddExperience(string userId, string reason, int amount)
        {
            if (userId is null)
                throw new ArgumentNullException(nameof(userId));

            if (!Levels.TryGetValue(userId, out var savedLevel))
                return false;

            var changingExperienceArgs = new ChangingExperienceEventArgs(savedLevel, userId, reason, savedLevel.Experience, savedLevel.Experience + amount);

            if (!LevelEvents.OnChangingExperience(changingExperienceArgs))
                return false;

            savedLevel.Experience += amount;

            LevelEvents.OnChangedExperience(new(savedLevel, userId, reason, changingExperienceArgs.CurrentExp, savedLevel.Experience), changingExperienceArgs.target);
            LevelProgress.CheckProgress(userId, reason, savedLevel);

            return true;
        }

        /// <summary>
        /// Subtracts a specified amount of experience from the user's current level.
        /// </summary>
        /// <param name="userId">The unique identifier of the user whose experience is to be subtracted. Cannot be null.</param>
        /// <param name="amount">The amount of experience to subtract from the user's current level.</param>
        /// <returns><see langword="true"/> if the user's experience was successfully subtracted; otherwise, <see
        /// langword="false"/> if the user ID does not exist.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="userId"/> is null.</exception>
        public static bool SubtractExperience(string userId, string reason, int amount)
        {
            if (userId is null)
                throw new ArgumentNullException(nameof(userId));

            if (!Levels.TryGetValue(userId, out var savedLevel))
                return false;

            var changingExperienceArgs = new ChangingExperienceEventArgs(savedLevel, userId, reason, savedLevel.Experience, savedLevel.Experience - amount);

            if (!LevelEvents.OnChangingExperience(changingExperienceArgs))
                return false;

            savedLevel.Experience -= amount;

            LevelEvents.OnChangedExperience(new(savedLevel, userId, reason, changingExperienceArgs.CurrentExp, savedLevel.Experience), changingExperienceArgs.target);
            LevelProgress.CheckProgress(userId, reason, savedLevel);

            return true;
        }

        /// <summary>
        /// Resets a player's level and experience to the initial state.
        /// </summary>
        /// <param name="userId">The player's user ID.</param>
        /// <returns>true if the level was reset</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static bool ResetLevel(string userId, string reason = "Command")
        {
            if (userId is null)
                throw new ArgumentNullException(nameof(userId));

            if (!Levels.TryGetValue(userId, out var savedLevel))
                return false;

            var changingLevelArgs = new ChangingLevelEventArgs(savedLevel, userId, reason, savedLevel.Level, 1);
            var changingExperienceArgs = new ChangingExperienceEventArgs(savedLevel, userId, reason, savedLevel.Experience, 0);

            LevelEvents.OnChangingLevel(changingLevelArgs);
            LevelEvents.OnChangingExperience(changingExperienceArgs);

            savedLevel.Level = 1;
            savedLevel.Experience = 0;
            savedLevel.RequiredExperience = LevelProgress.ExperienceForLevel(2);

            LevelEvents.OnChangedLevel(new(savedLevel, userId, reason, changingLevelArgs.CurrentLevel, savedLevel.Level), changingLevelArgs.target);
            LevelEvents.OnChangedExperience(new(savedLevel, userId, reason, changingExperienceArgs.CurrentExp, savedLevel.Experience), changingExperienceArgs.target);

            return true;
        }

        /// <summary>
        /// Resets all levels.
        /// </summary>
        /// <returns>true if any levels have been reset</returns>
        public static bool ResetLevels(string reason = "Command")
        {
            if (Storage.RemoveAll(true) > 0)
            {
                foreach (var level in Levels)
                {
                    Storage.Add(level.Value);

                    var changingLevelArgs = new ChangingLevelEventArgs(level.Value, level.Key, reason, level.Value.Level, 1);
                    var changingExperienceArgs = new ChangingExperienceEventArgs(level.Value, level.Key, reason, level.Value.Experience, 0);

                    if (changingLevelArgs.Target != null)
                        LevelEvents.OnRemoved(changingLevelArgs.Target, level.Value);

                    LevelEvents.OnChangingLevel(changingLevelArgs);
                    LevelEvents.OnChangingExperience(changingExperienceArgs);

                    level.Value.Level = 1;
                    level.Value.Experience = 0;
                    level.Value.RequiredExperience = LevelProgress.ExperienceForLevel(2);

                    LevelEvents.OnChangedLevel(new(level.Value, level.Key, reason, changingLevelArgs.CurrentLevel, level.Value.Level), changingLevelArgs.target);
                    LevelEvents.OnChangedExperience(new(level.Value, level.Key, reason, changingExperienceArgs.CurrentExp, level.Value.Experience), changingExperienceArgs.target);

                    if (changingLevelArgs.Target != null)
                        LevelEvents.OnLoaded(changingLevelArgs.Target, level.Value);
                }

                return true;
            }

            return false;
        }

        private static void Left(ExPlayer player)
        {
            Levels.Remove(player.UserId);
        }

        private static void Verified(ExPlayer player)
        {
            if (!player.CanCollect("Levels"))
                return;

            var level = 
                Levels[player.UserId] 
                        = Storage.GetOrAdd(player.UserId, () => new SavedLevel());

            level.RequiredExperience = LevelProgress.ExperienceForLevel(level.Level + 1);

            LevelEvents.OnLoaded(player, level);
        }

        private static void BuildingInfo(ExPlayer player, StringBuilder builder)
        {
            if (!Levels.TryGetValue(player.UserId, out var level))
                return;

            builder.AppendLine($"LVL {level.Level} ({Mathf.CeilToInt(level.Experience)} XP / {Mathf.CeilToInt(level.RequiredExperience)} XP)");
        }

        private static void OnEntryToggled(ExPlayer player, DataCollectionEntry entry, bool isAllowed)
        {
            if (entry.Id != "Levels")
                return;

            if (!isAllowed)
            {
                if (Levels.TryGetValue(player.UserId, out var level))
                {
                    Levels.Remove(player.UserId);
                    LevelEvents.OnRemoved(player, level);
                }
            }
            else
            {
                if (Levels.ContainsKey(player.UserId))
                    return;

                var level =
                    Levels[player.UserId]
                            = Storage.GetOrAdd(player.UserId, () => new SavedLevel());

                level.RequiredExperience = LevelProgress.ExperienceForLevel(level.Level + 1);

                LevelEvents.OnLoaded(player, level);
            }
        }

        internal static void Removed(StorageValue value)
        {
            if (value is not SavedLevel level)
                return;

            if (!Levels.TryGetKey(level, out var userId))
                return;

            if (ExPlayer.TryGet(userId, out var player))
                LevelEvents.OnRemoved(player, level);

            Levels.Remove(userId);
        }

        internal static void Initialize()
        {
            Storage = StorageManager.CreateStorage("LevelManager", SecretLab.Config.LevelsUseShared);

            if (Storage != null)
            {
                LevelProgress.Initialize();
                
                DataCollection.AddEntry(new("Levels",
                    "<color=red>📊</color> | <b>Level Systém</b></color>", 
                    "Ukládá počet dosažených XP a levelů pomocí identifikátoru vašeho účtu <i>(SteamID64 pro Steam účty a Discord ID pro Discord účty)</i>."));

                Storage.Removed += Removed;

                ExPlayerEvents.Left += Left;
                ExPlayerEvents.Verified += Verified;

                DataCollection.EntryToggled += OnEntryToggled;

                if (SecretLab.Config.LevelsShowInCustomInfo)
                    ExPlayerEvents.RefreshingCustomInfo += BuildingInfo;
            }
        }
    }
}