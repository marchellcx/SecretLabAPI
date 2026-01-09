using SecretLabAPI.Utilities.Configs;

using System.ComponentModel;

namespace SecretLabAPI.RandomEvents
{
    public class RandomEventConfig
    {
        /// <summary>
        /// Represents the base weight (chance) of an event being selected at the start of the round.
        /// </summary>
        [Description("Sets the base weight (chance) of an event being selected at the start of the round.")]
        public float EventWeight { get; set; } = 0f;

        /// <summary>
        /// Gets or sets the weight that determines the likelihood of group events being selected at the start of the
        /// round.
        /// </summary>
        [Description("Sets the weight (chance) of group events being selected at the start of the round.")]
        public float GroupWeight { get; set; } = 0f;

        /// <summary>
        /// Gets or sets the range of event counts allowed in a group event.
        /// </summary>
        [Description("Sets the range of event count in a group event.")]
        public Int32Range GroupSize { get; set; } = new()
        {
            MinValue = 2,
            MaxValue = 3
        };

        /// <summary>
        /// Defines the collection of weights assigned to specific custom gamemodes
        /// used in the random event selection process.
        /// </summary>
        [Description("Sets the weight of custom gamemodes in random event selection.")]
        public Dictionary<string, float> Weights { get; set; } = new();
    }
}