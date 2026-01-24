namespace SecretLabAPI.Features.Actions.Enums
{
    /// <summary>
    /// Specifies the result of an action, indicating whether it succeeded or failed and whether subsequent processing
    /// should continue or stop.
    /// </summary>
    [Flags]
    public enum ActionResultFlags
    {
        /// <summary>
        /// No flags are set.
        /// </summary>
        None = 0,

        /// <summary>
        /// Indicates a succesfull execution.
        /// </summary>
        Success = 1,

        /// <summary>
        /// Indicates that the context should be disposed.
        /// </summary>
        Dispose = 2,

        /// <summary>
        /// Indicates that the execution should stop.
        /// </summary>
        Stop = 4,

        /// <summary>
        /// Represents a status that indicates both a successful operation and that disposal has occurred.
        /// </summary>
        SuccessDispose = Success | Dispose,

        /// <summary>
        /// Represents a combination of the Stop and Dispose actions, indicating that both operations should be
        /// performed together.
        /// </summary>
        StopDispose = Stop | Dispose,

        /// <summary>
        /// Indicates that the action completed successfully and further processing should stop.
        /// Combines both <see cref="Success"/> and <see cref="Stop"/> flags.
        /// </summary>
        SuccessStop = Success | Stop,
    }
}