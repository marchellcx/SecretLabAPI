using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Arguments.ServerEvents;

using LabApi.Events.Handlers;

using LabExtended.API;
using LabExtended.Events;

using MapGeneration.Distributors;

using PlayerRoles;

namespace SecretLabAPI.Levels.Rewards
{
    /// <summary>
    /// Manages experience gains related to player map interactions.
    /// </summary>
    public static class MapRewards
    {
        private static Dictionary<Scp079Generator, ExPlayer> generatorActivators = new();

        /// <summary>
        /// Provides access to the configuration of experience rewards associated with various player actions on the map.
        /// </summary>
        public static LevelRewards Rewards => LevelRewards.Rewards;

        private static void OnGeneratorDeactivated(PlayerDeactivatedGeneratorEventArgs args)
        {
            if (args.Generator?.Base != null) generatorActivators.Remove(args.Generator.Base);
            if (args.Player is not ExPlayer player || player?.ReferenceHub == null || !player.Role.IsScp) return;
            if (!ExPlayer.Players.Any(p => p.Role.Is(RoleTypeId.Scp079))) return;

            var exp = Rewards.ScpDeactivatedGeneratorXp.GetRandom();
            if (exp > 0) player.AddExperience("Deactivated a generator", exp);
        }
        
        private static void OnGeneratorFinished(GeneratorActivatedEventArgs args)
        {
            if (args.Generator?.Base == null) return;
            if (!generatorActivators.TryGetValue(args.Generator.Base, out var activator) || activator?.ReferenceHub == null) return;
            if (!ExPlayer.Players.Any(p => p.Role.Is(RoleTypeId.Scp079))) return;

            var exp = Rewards.ActivatedGeneratorXp.GetRandom();
            if (exp > 0) activator.AddExperience("Activated a generator", exp);

            generatorActivators.Remove(args.Generator.Base);

            var scpExp = Rewards.ScpActivatedGeneratorXp.GetRandom();

            if (scpExp > 0)
            {
                ExPlayer.Players.ForEach(p =>
                {
                    if (!p.Role.IsScp) return;
                    p.SubtractExperience("Let a generator activate", scpExp);
                });
            }
        }

        private static void OnInserting(PlayerActivatedGeneratorEventArgs args)
        {
            if (args.Generator?.Base == null) return;
            if (args.Player is not ExPlayer player || player?.ReferenceHub == null) return;
            if (!ExPlayer.Players.Any(p => p.Role.Is(RoleTypeId.Scp079))) return;

            generatorActivators[args.Generator.Base] = player;
        }
        
        internal static void Initialize()
        {
            ServerEvents.GeneratorActivated += OnGeneratorFinished;
            
            PlayerEvents.ActivatedGenerator += OnInserting;
            PlayerEvents.DeactivatedGenerator += OnGeneratorDeactivated;

            ExRoundEvents.Restarting += generatorActivators.Clear;
        }
    }
}