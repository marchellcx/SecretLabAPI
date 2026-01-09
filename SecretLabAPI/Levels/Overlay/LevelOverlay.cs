using LabExtended.API;
using LabExtended.API.Enums;
using LabExtended.API.Hints.Elements.Personal;

using SecretLabAPI.Extensions;

using SecretLabAPI.Levels;
using SecretLabAPI.Levels.IO;
using SecretLabAPI.Levels.Events;

using SecretLabAPI.Utilities.Configs;

using UnityEngine;

namespace SecretLabAPI.Elements.Levels
{
    /// <summary>
    /// Shows information about level and experience gains.
    /// </summary>
    public class LevelOverlay : PersonalHintElement
    {
        private string? levelOverlay;

        public static OverlayOptions Settings => LevelManager.config.Overlay;

        /// <summary>
        /// Gets the saved level data.
        /// </summary>
        public LevelData Level { get; set; }

        /// <inheritdoc/>
        public override HintAlign Alignment => Settings.Align;

        /// <inheritdoc/>
        public override float VerticalOffset => Settings.VerticalOffset;

        /// <inheritdoc/>
        public override int PixelSpacing => Settings.PixelSpacing < 0
            ? base.PixelSpacing
            : Settings.PixelSpacing;

        /// <inheritdoc/>
        public override void OnEnabled()
        {
            base.OnEnabled();

            LevelManager.AddedExperience += OnAddedExperience;
        }

        /// <inheritdoc/>
        public override void OnDisabled()
        {
            base.OnDisabled();

            LevelManager.AddedExperience -= OnAddedExperience;
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

            var xp = (float)Level.Experience;
            var reqXp = (float)Level.RequiredExperience;

            var percentage = Mathf.CeilToInt((xp / reqXp) * 100);
            var color = GetBarColor(percentage);

            value = value
                .Replace("$BarColor", color)
                .Replace("$BarString", RenderBar(percentage))

                .Replace("$CurExp", Level.Experience.ToString())
                .Replace("$ReqExp", Level.RequiredExperience.ToString())
                
                .Replace("$CurLevel", Level.Level.ToString())
                .Replace("$NextLevel", (Level.Level + 1 >= LevelManager.config.Cap ? Level.ToString() : (Level.Level + 1).ToString()))
                
                .ReplaceEmojis();

            levelOverlay = value.Length > 0
                ? value
                : null;
        }

        private void OnAddedExperience(AddedExperienceEventArgs args)
        {
            if (Player?.ReferenceHub == null || Player != args.Player)
                return;

            RefreshBar();
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