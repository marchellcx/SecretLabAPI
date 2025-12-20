using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;

using LabExtended.API;
using LabExtended.Events;
using LabExtended.Utilities;

using SecretLabAPI.Actions.API;
using SecretLabAPI.Actions.Enums;
using SecretLabAPI.Actions.Attributes;

namespace SecretLabAPI.Actions
{
    /// <summary>
    /// Provides action table execution for coins.
    /// </summary>
    public static class CoinActions
    {
        /// <summary>
        /// A list of player user IDs that will be ignored when flipping a coin.
        /// </summary>
        public static HashSet<string> PausedPlayers { get; } = new();

        /// <summary>
        /// Pauses the execution of action tables during a coin flip for the specified player.
        /// </summary>
        /// <param name="context">The action context, containing player information and other parameters.</param>
        /// <returns>
        /// Returns <see cref="ActionResultFlags.SuccessDispose"/> if the operation is successfully executed,
        /// indicating that the action was completed and resources should be cleaned up.
        /// </returns>
        [Action("PauseFlipping", "Pauses the execution of action tables upon coin flip for a certain player.")]
        [ActionParameter("Delay", "The amount of seconds to wait for before resuming coin flips for this player (will not be resumed if set to 0).")]
        public static ActionResultFlags PauseFlipping(ref ActionContext context)
        {
            context.EnsureCompiled((_, p) => p.EnsureCompiled(int.TryParse, 0));
            
            var delay = context.GetValue<int>(0);

            if (context.Player != null)
            {
                if (PausedPlayers.Add(context.Player.UserId) && delay > 0)
                {
                    var player = context.Player;
                    
                    TimingUtils.AfterSeconds(() => PausedPlayers.Remove(player.UserId), delay);
                }
            }

            return ActionResultFlags.SuccessDispose;
        }

        /// <summary>
        /// Resumes the execution of action tables during a coin flip for the specified player.
        /// </summary>
        /// <param name="context">The action context containing player information and relevant parameters.</param>
        /// <returns>
        /// Returns <see cref="ActionResultFlags.SuccessDispose"/> if the operation is successfully completed,
        /// indicating that the player's pause state has been cleared and the action is finalized.
        /// </returns>
        [Action("ResumeFlipping", "Resumes the execution of action tables upon coin flip for a certain player.")]
        public static ActionResultFlags ResumeFlipping(ref ActionContext context)
        {
            if (context.Player != null)
                PausedPlayers.Remove(context.Player.UserId);

            return ActionResultFlags.SuccessDispose;
        }
        
        private static void OnFlipped(PlayerFlippedCoinEventArgs args)
        {
            if (args.Player is not ExPlayer player)
                return;

            if (PausedPlayers.Contains(player.UserId))
                return;
            
            var prefix = args.IsTails
                ? "CoinTails_"
                : "CoinHead_";

            ActionManager.Table.SelectAndExecuteTable(player, str => str.StartsWith(prefix));
        }

        private static void OnRestart()
        {
            PausedPlayers.Clear();
        }

        private static void OnLeave(ExPlayer player)
        {
            PausedPlayers.Remove(player.UserId);
        }

        internal static void Initialize()
        {
            PlayerEvents.FlippedCoin += OnFlipped;

            ExPlayerEvents.Left += OnLeave;
            ExRoundEvents.Restarting += OnRestart;
        }
    }
}