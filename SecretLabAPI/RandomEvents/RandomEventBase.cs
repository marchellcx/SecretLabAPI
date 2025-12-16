using System.ComponentModel;

using LabExtended.API.Custom.Gamemodes;

namespace SecretLabAPI.RandomEvents
{
    /// <summary>
    /// Base class for random events.
    /// </summary>
    public abstract class RandomEventBase : CustomGamemode
    {
        /// <summary>
        /// Gets or sets the weight of the random event.
        /// </summary>
        /// <remarks>
        /// The weight determines the likelihood of the event being selected during random event processing.
        /// A higher weight increases the chance of selection relative to other events.
        /// </remarks>
        [Description("Sets the weight of the event.")]
        public virtual float Weight { get; set; } = 0f;

        /// <summary>
        /// Gets or sets the minimum number of players required for the event to be eligible for activation.
        /// </summary>
        /// <remarks>
        /// This property defines a threshold for the minimum player count needed to trigger the event.
        /// If the active player count is below this value, the event will not be considered for selection.
        /// A value of <c>null</c> indicates that there is no minimum player requirement.
        /// </remarks>
        [Description("Sets the minimumn amount of players required for this event.")]
        public virtual int? MinPlayers { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of players allowed for this random event.
        /// </summary>
        /// <remarks>
        /// This property specifies the maximum number of players that can participate in the event.
        /// If the number of players exceeds this value, the event will not be triggered.
        /// </remarks>
        [Description("Sets the maximum amount of players possible for this event.")]
        public virtual int? MaxPlayers { get; set; }
    }
}