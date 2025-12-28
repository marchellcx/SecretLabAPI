using System.ComponentModel;

using LabExtended.Utilities;

namespace SecretLabAPI.Utilities
{
    /// <summary>
    /// Represents a configuration for calculating weighted values using a base weight and optional per-user, per-rank,
    /// and per-level multipliers.
    /// </summary>
    public class WeightMultipliers
    {
        /// <summary>
        /// Gets or sets the collection of weight multiplier groups, indexed by group name.
        /// </summary>
        public static Dictionary<string, WeightMultipliers> Groups { get; set; } = new()
        {
            ["example"] = new(),
            ["example2"] = new()
        };

        /// <summary>
        /// Gets or sets the per-player weight multipliers, keyed by user ID or rank.
        /// </summary>
        [Description("Sets the per-player weight multipliers (user ID or rank).")]
        public Dictionary<string, float> Multipliers { get; set; } = new();

        /// <summary>
        /// Gets or sets the per-player level weight multipliers used to adjust calculations based on player level.
        /// </summary>
        [Description("Sets the per-player level weight multipliers.\n" +
            "# Multipliers DO NOT stack, only the highest one is used.")]
        public Dictionary<int, float> LevelMultipliers { get; set; } = new();

        /// <summary>
        /// Calculates the effective weight for a user based on their identifier, rank, and level.
        /// </summary>
        /// <remarks>If multiple multipliers are applicable, they are applied multiplicatively in the
        /// order: user ID, user rank, and user level. The method does not throw exceptions for missing or invalid
        /// multipliers; it simply omits them from the calculation.</remarks>
        /// <param name="userId">The unique identifier of the user. If not null and a corresponding multiplier exists, it will be applied to
        /// the weight.</param>
        /// <param name="userRank">The rank of the user. If not null and a corresponding multiplier exists, it will be applied to the weight.</param>
        /// <param name="userLevel">The level of the user. Must be zero or greater. If greater than zero, the highest applicable level
        /// multiplier will be applied to the weight.</param>
        /// <returns>A floating-point value representing the calculated weight for the specified user. The value reflects all
        /// applicable multipliers; if no multipliers apply, the base weight is returned.</returns>
        public float GetWeight(float weight, string userId, string? userRank, int userLevel)
        {
            if (userId != null && Multipliers.TryGetValue(userId, out var idMultiplier))
                weight *= idMultiplier;

            if (userRank != null && Multipliers.TryGetValue(userRank, out var rankMultiplier))
                weight *= rankMultiplier;

            if (userLevel > 0)
            {
                var highestMultiplier = 0f;

                foreach (var pair in LevelMultipliers)
                {
                    if (userLevel >= pair.Key && pair.Value > highestMultiplier)
                    {
                        highestMultiplier = pair.Value;
                    }
                }

                if (highestMultiplier > 0f)
                    weight *= highestMultiplier;
            }

            return weight;
        }

        internal static void Initialize()
        {
            if (FileUtils.TryLoadYamlFile<Dictionary<string, WeightMultipliers>>(SecretLab.RootDirectory,
                    "weight_groups.yml", out var groups))
            {
                Groups = groups;
            }
            else
            {
                FileUtils.TrySaveYamlFile(SecretLab.RootDirectory, "weight_groups.yml", Groups);
            }
        }
    }
}