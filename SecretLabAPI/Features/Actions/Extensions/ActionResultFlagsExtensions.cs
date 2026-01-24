using SecretLabAPI.Features.Actions.Enums;

namespace SecretLabAPI.Features.Actions.Extensions
{
    /// <summary>
    /// Provides extension methods for evaluating and interpreting bitwise flags in ActionResultFlags values.
    /// </summary>
    public static class ActionResultFlagsExtensions
    {
        /// <summary>
        /// Determines whether one or more bitwise flags are set in the specified ActionResultFlags value.
        /// </summary>
        /// <remarks>This method provides a faster alternative to Enum.HasFlag by avoiding boxing and is
        /// suitable for performance-critical scenarios. Both parameters should be valid ActionResultFlags
        /// values.</remarks>
        /// <param name="flags">The ActionResultFlags value to evaluate for the presence of the specified flag or flags.</param>
        /// <param name="flag">The ActionResultFlags flag or combination of flags to check for within the flags value.</param>
        /// <returns>true if all bits in flag are set in flags; otherwise, false.</returns>
        public static bool HasFlagFast(this ActionResultFlags flags, ActionResultFlags flag)
        {
            return (flags & flag) == flag;
        }

        /// <summary>
        /// Determines whether the specified flags indicate a successful result.
        /// </summary>
        /// <param name="flags">The set of <see cref="ActionResultFlags"/> to evaluate for a success indicator.</param>
        /// <returns>true if the <paramref name="flags"/> include the Success flag; otherwise, false.</returns>
        public static bool IsSuccess(this ActionResultFlags flags)
        {
            return flags.HasFlagFast(ActionResultFlags.Success);
        }

        /// <summary>
        /// Determines whether the specified flags indicate that the associated resource should be disposed.
        /// </summary>
        /// <param name="flags">The set of <see cref="ActionResultFlags"/> values to evaluate for the dispose condition.</param>
        /// <returns>true if the <see cref="ActionResultFlags.Dispose"/> flag is set; otherwise, false.</returns>
        public static bool ShouldDispose(this ActionResultFlags flags)
        {
            return flags.HasFlagFast(ActionResultFlags.Dispose);
        }

        /// <summary>
        /// Determines whether the specified flags indicate that processing should stop.
        /// </summary>
        /// <param name="flags">The set of <see cref="ActionResultFlags"/> to evaluate for the <see cref="ActionResultFlags.Stop"/>
        /// condition.</param>
        /// <returns>true if the <see cref="ActionResultFlags.Stop"/> flag is set; otherwise, false.</returns>
        public static bool ShouldStop(this ActionResultFlags flags)
        {
            return flags.HasFlagFast(ActionResultFlags.Stop);
        }
    }
}