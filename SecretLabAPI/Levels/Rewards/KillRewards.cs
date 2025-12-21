using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;

using LabExtended.API;

namespace SecretLabAPI.Levels.Rewards
{
    /// <summary>
    /// A static class that manages the reward system for player kills and deaths.
    /// </summary>
    public static class KillRewards
    {
        /// <summary>
        /// Provides access to the <see cref="LevelRewards"/> instance responsible for configuring
        /// the experience rewards and penalties for various player actions such as kills and deaths.
        /// </summary>
        public static LevelRewards Rewards => LevelRewards.Rewards;
        
        private static void OnDeath(PlayerDeathEventArgs args)
        {
            if (args.Player is not ExPlayer target || target?.ReferenceHub == null) return;

            if (args.Attacker is ExPlayer attacker)
            {
                if (target.IsSCP && attacker.IsHuman)
                {
                    var scpExp = Rewards.KilledScpExp.GetRandom();
                    var humanExp = Rewards.DiedToHumanXp.GetRandom();

                    if (scpExp > 0) attacker.AddExperience("Killed an SCP", scpExp);
                    if (humanExp > 0) target.SubtractExperience("Died to a human", humanExp);
                }
                else if (target.IsHuman && attacker.IsSCP)
                {
                    var scpExp = Rewards.KilledHumanExp.GetRandom();
                    var humanExp = Rewards.DiedToScpXp.GetRandom();

                    if (scpExp > 0) attacker.AddExperience("Killed a human", scpExp);
                    if (humanExp > 0) target.SubtractExperience("Died to an SCP", humanExp);
                }
                else if (target.IsHuman && attacker.IsHuman)
                {
                    var attackerExp = Rewards.KilledHumanExp.GetRandom();
                    var targetExp = Rewards.DiedToHumanXp.GetRandom();

                    if (attackerExp > 0) attacker.AddExperience("Killed a human", attackerExp);
                    if (targetExp > 0) target.SubtractExperience("Died to a human", targetExp);
                }
            }
            else
            {
                var exp = Rewards.DiedBySuicideXp.GetRandom();
                if (exp > 0) target.SubtractExperience("Died to an unknown reason", exp);
            }
        }
        
        internal static void Initialize()
        {
            PlayerEvents.Death += OnDeath;
        }
    }
}