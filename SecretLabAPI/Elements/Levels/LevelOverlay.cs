using LabExtended.API;
using LabExtended.API.Enums;
using LabExtended.API.Hints.Elements.Personal;

using LabExtended.Extensions;

using SecretLabAPI.Extensions;

using UnityEngine;

using SecretLabAPI.Levels;
using SecretLabAPI.Levels.Storage;

using SecretLabAPI.Elements.Levels.Entries;

namespace SecretLabAPI.Elements.Levels
{
    /// <summary>
    /// Shows information about level and experience gains.
    /// </summary>
    public class LevelOverlay : PersonalHintElement
    {
        private float levelTime;
        private float experienceTime;

        private string? levelOverlay;

        /// <summary>
        /// Gets the options for the level overlay.
        /// </summary>
        public static LevelSettings Settings => SecretLab.Config.LevelOverlay;

        /// <summary>
        /// Gets the saved level data.
        /// </summary>
        public SavedLevel Level { get; set; }

        /// <summary>
        /// Gets the currently displayed level entry.
        /// </summary>
        public LevelGainEntry? CurrentLevelEntry { get; private set; }

        /// <summary>
        /// Gets the currently displayed experience entry.
        /// </summary>
        public ExperienceGainEntry? CurrentExperienceEntry { get; private set; }

        /// <summary>
        /// Gets a list of level entries.
        /// </summary>
        public List<LevelGainEntry> LevelEntries { get; } = new();

        /// <summary>
        /// Gets a list of experience entries.
        /// </summary>
        public List<ExperienceGainEntry> ExperienceEntries { get; } = new();

        /// <inheritdoc/>
        public override HintAlign Alignment => Settings.Align;

        /// <inheritdoc/>
        public override float VerticalOffset => Settings.VerticalOffset;

        /// <inheritdoc/>
        public override int PixelSpacing => Settings.PixelSpacing < 0
            ? base.PixelSpacing
            : Settings.PixelSpacing;

        /// <inheritdoc/>
        public override void OnUpdate()
        {
            base.OnUpdate();

            if (CurrentLevelEntry.HasValue && Time.realtimeSinceStartup >= levelTime)
                CurrentLevelEntry = null;

            if (CurrentExperienceEntry.HasValue && Time.realtimeSinceStartup >= experienceTime)
                CurrentExperienceEntry = null;

            if (LevelEntries.Count > 0 && !CurrentLevelEntry.HasValue)
                CurrentLevelEntry = LevelEntries.RemoveAndTake(0);

            if (ExperienceEntries.Count > 0 && !CurrentExperienceEntry.HasValue)
                CurrentExperienceEntry = ExperienceEntries.RemoveAndTake(0);

            if (CurrentLevelEntry != null)
                levelTime = Time.realtimeSinceStartup + Settings.LevelGainDuration;

            if (CurrentExperienceEntry != null)
                experienceTime = Time.realtimeSinceStartup + Settings.ExperienceGainDuration;
        }

        /// <inheritdoc/>
        public override bool OnDraw(ExPlayer player)
        {
            if (Builder is null)
                return false;

            if (Level != null && levelOverlay != null)
            {
                Builder.Append(levelOverlay);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Refreshes the progress bar display.
        /// </summary>
        public void RefreshBar()
        {
            if (Level is null)
            {
                levelOverlay = null;
                return;
            }

            var value = Settings.OverlayString.GetValue() ?? string.Empty;

            if (value.Length < 1)
                return;

            var percentage = 0;
            
            var curXp = 0;
            
            var reqXp = 0;
            var reqXpString = string.Empty;

            var nextLevelString = (Level.Level + 1 > LevelProgress.Cap)
                ? "MAX"
                : (Level.Level + 1).ToString();

            if (!Level.IsCapped)
            {
                percentage = Mathf.CeilToInt((Level.Experience / Level.RequiredExperience) * 100);
                
                curXp = Level.Experience;
                
                reqXp = Level.RequiredExperience;
                reqXpString = reqXp.ToString();
            }
            else
            {
                percentage = 100;
                
                curXp = Level.Experience;
                
                reqXp = Level.Experience;
                reqXpString = "MAX";
            }

            var color = GetBarColor(percentage);

            value = value
                .Replace("$BarColor", color)
                .Replace("$BarString", RenderBar(percentage))

                .Replace("$CurExp", curXp.ToString())
                .Replace("$ReqExp", reqXpString)
                
                .Replace("$CurLevel", Level.Level.ToString())
                .Replace("$NextLevel", nextLevelString)
                
                .ReplaceEmojis();

            levelOverlay = value.Length > 0
                ? value
                : null;
        }

        private static string GetBarColor(int percentage)
        {
            if (percentage >= 85)
                return "#1dde37";

            if (percentage >= 70)
                return "#9deb21";

            if (percentage >= 50)
                return "#d6f233";

            if (percentage >= 30)
                return "#f2dc33";

            if (percentage >= 15)
                return "#f27933";

            return "#eb220c";
        }

        private static string RenderBar(int percentage)
        {
            percentage = Math.Min(percentage, 100);

            var symbols = "";
            var i = 5;

            for (; i <= 95; i += 10)
            {
                if (i >= percentage)
                    break;

                symbols += "█";
            }

            if (i - 5 < percentage)
                symbols += "▄";

            var symbolCount = symbols.Length;
            var invisibleBar = false;

            if (symbols.Length < 10)
            {
                symbols += "<alpha=#00>█";
                invisibleBar = true;
                symbolCount++;
            }

            for (i = 0; i < 10 - symbolCount; i++)
                symbols += "█";

            if (invisibleBar)
                symbols += "<alpha=#FF>";

            return symbols;
        }
    }
}