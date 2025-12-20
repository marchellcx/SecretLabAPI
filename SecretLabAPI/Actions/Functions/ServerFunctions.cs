using LabExtended.API;

using SecretLabAPI.Actions.API;
using SecretLabAPI.Actions.Enums;
using SecretLabAPI.Actions.Attributes;

namespace SecretLabAPI.Actions.Functions
{
    /// <summary>
    /// Actions related to server management.
    /// </summary>
    public static class ServerFunctions
    {
        /// <summary>
        /// Bans one or more players from the server using the specified reason and duration.
        /// </summary>
        /// <remarks>The ban reason and duration are retrieved from the context parameters. All selected
        /// players in the context will be banned for the specified duration. Ensure that the context is properly
        /// compiled with valid parameters before calling this method.</remarks>
        /// <param name="context">The action context containing player selection and ban parameters. Must include a reason and a duration (in
        /// seconds) for the ban.</param>
        /// <returns>An ActionResultFlags value indicating the result of the ban operation. Returns SuccessDispose if the action
        /// completes successfully.</returns>
        [Action("Ban", "Bans players from the server.")]
        [ActionParameter("Reason", "Reason for the ban.")]
        [ActionParameter("Duration", "Duration of the ban (in seconds).")]
        public static ActionResultFlags Ban(ref ActionContext context)
        {
            context.EnsureCompiled((index, parameter) =>
            {
                return index switch
                {
                    0 => parameter.EnsureCompiled("No reason specified."),
                    1 => parameter.EnsureCompiled<long>(long.TryParse, 0),

                    _ => false
                };
            });

            var reason = context.GetValue(0);
            var duration = context.GetValue<long>(1);

            context.Player.Ban(reason, duration);
            return ActionResultFlags.SuccessDispose;
        }

        /// <summary>
        /// Kicks one or more players from the server using the specified action context.
        /// </summary>
        /// <remarks>The reason for the kick is retrieved from the context using the key "KickReason". All
        /// players specified in the context are kicked with the same reason.</remarks>
        /// <param name="context">A reference to the action context containing information about the players to kick and the reason for the
        /// kick. Must be compiled before use.</param>
        /// <returns>An ActionResultFlags value indicating the result of the operation. Returns SuccessDispose if the players
        /// were kicked successfully.</returns>
        [Action("Kick", "Kicks a player from the server.")]
        [ActionParameter("Reason", "The reason for the kick")]
        public static ActionResultFlags Kick(ref ActionContext context)
        {
            context.EnsureCompiled((index, parameter) =>
            {
                return index switch
                {
                    0 => parameter.EnsureCompiled("No reason specified."),
                    _ => false,
                };
            });

            var reason = context.GetValue(0);

            context.Player.Kick(reason);
            return ActionResultFlags.SuccessDispose;
        }

        /// <summary>
        /// Executes a specified server command.
        /// </summary>
        /// <remarks>This method retrieves and executes a command string specified in the action context. Ensure that the command
        /// parameter is properly compiled and valid before invoking this method.</remarks>
        /// <param name="context">The action context containing the command to execute. The context must include a properly compiled command string.</param>
        /// <returns>An ActionResultFlags value indicating the result of the command execution. Typically returns SuccessDispose if the command is successfully executed.</returns>
        [Action("Command", "Executes a command.")]
        [ActionParameter("Command", "The command to execute.")]
        public static ActionResultFlags Command(ref ActionContext context)
        {
            context.EnsureCompiled((_, p) => p.EnsureCompiled(string.Empty));
            
            ExServer.ExecuteCommand(context.GetValue(0));
            return ActionResultFlags.SuccessDispose;
        }

        /// <summary>
        /// Logs a specified message to the server console.
        /// </summary>
        /// <remarks>
        /// This method processes the provided action context to retrieve the message to be logged.
        /// The message is appended to the server console log. Ensure the context parameter
        /// is properly compiled with a valid message before invoking this method.
        /// </remarks>
        /// <param name="context">The action context containing the parameters for the log operation.
        /// The first parameter should include the message to log.</param>
        /// <returns>An ActionResultFlags value indicating the result of the logging operation.
        /// Returns SuccessDispose if logging is successful.</returns>
        [Action("Log", "Logs a message to the server console.")]
        [ActionParameter("Message", "The message to log into the console.")]
        public static ActionResultFlags Log(ref ActionContext context)
        {
            context.EnsureCompiled((_, p) => p.EnsureCompiled(string.Empty));
            
            ServerConsole.AddLog(context.GetValue(0));
            return ActionResultFlags.SuccessDispose;
        }
    }
}