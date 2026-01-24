namespace SecretLabAPI.Features.Roles.ChaosSpy
{
    /// <summary>
    /// Contains data of a chaos spy.
    /// </summary>
    public class ChaosSpyData
    {
        /// <summary>
        /// Whether or not the Chaos disguise is currently in cooldown.
        /// </summary>
        public bool IsInCooldown { get; internal set; }

        /// <summary>
        /// Gets the time at which the cooldown ends (relative to <see cref="UnityEngine.Time.realtimeSinceStartup"/>).
        /// </summary>
        public float CooldownEndTime { get; internal set;  }
    }
}