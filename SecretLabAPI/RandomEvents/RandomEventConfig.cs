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
        /// Defines the collection of weights assigned to specific custom gamemodes
        /// used in the random event selection process.
        /// </summary>
        [Description("Sets the weight of custom gamemodes in random event selection.")]
        public Dictionary<string, float> Weights { get; set; } = new();
    }
}