using SecretLabAPI.Extensions;

using SecretLabAPI.Actions.API;
using SecretLabAPI.Actions.Enums;
using SecretLabAPI.Actions.Attributes;

using PlayerRoles;

using LabExtended.API;
using LabExtended.Extensions;

namespace SecretLabAPI.Misc.Actions
{
    /// <summary>
    /// Provides static methods for evaluating and performing team-switching actions between Foundation Forces and Chaos
    /// Insurgency players.
    /// </summary>
    /// <remarks>This class contains utility actions related to switching player teams in scenarios where such
    /// operations are required. All methods are static and intended to be used within the context of action execution
    /// workflows. Facility Guards are specifically excluded from certain operations, as noted in individual method
    /// documentation.</remarks>
    public static class MiscActions
    {
        /// <summary>
        /// Determines whether the SwitchSides action can be executed based on the presence of eligible players on both
        /// the Foundation Forces and Chaos Insurgency teams.
        /// </summary>
        /// <remarks>The action can only be executed if there is at least one Foundation Forces player
        /// (excluding Facility Guards) and at least one Chaos Insurgency player present. The result of the check is
        /// stored in the context's memory for use by subsequent actions.</remarks>
        /// <param name="context">A reference to the current action context. The method updates the context's memory to indicate whether the
        /// action can be performed.</param>
        /// <returns>An ActionResultFlags value indicating the result of the check. Always returns SuccessDispose after updating
        /// the context.</returns>
        [Action("CanSwitchSides", "Checks if the SwitchSides action can be executed based on the presence of players in both teams.")]
        public static ActionResultFlags CanSwitchSides(ref ActionContext context)
        {
            var mtfPlayers = ExPlayer.Players.Where(p => p.Team == Team.FoundationForces && p.Role.Type != RoleTypeId.FacilityGuard);
            var ciPlayers = ExPlayer.Players.Where(p => p.Team == Team.ChaosInsurgency);

            if (mtfPlayers.Count() == 0 || ciPlayers.Count() == 0)
            {
                context.SetMemory(false);
                return ActionResultFlags.SuccessDispose;
            }

            context.SetMemory(true);
            return ActionResultFlags.SuccessDispose;
        }

        /// <summary>
        /// Switches all players on the Foundation Forces team (excluding Facility Guards) to the Chaos Insurgency team,
        /// and all Chaos Insurgency players to the Foundation Forces team.
        /// </summary>
        /// <remarks>Facility Guards are not affected by this operation and remain on their current team.
        /// The method only performs the switch if there is at least one eligible player on each team.</remarks>
        /// <param name="context">A reference to the current action context. Provides information about the action execution environment and
        /// may be modified by the method.</param>
        /// <returns>An ActionResultFlags value indicating the outcome of the operation. Returns SuccessDispose if the switch was
        /// successful; otherwise, returns StopDispose if there are no eligible players to switch.</returns>
        [Action("SwitchSides", "Switches all Foundation Forces players (excluding Facility Guards) to Chaos Insurgency and vice versa.")]
        public static ActionResultFlags SwitchSides(ref ActionContext context)
        {
            var mtfPlayers = ExPlayer.Players.Where(p => p.Team == Team.FoundationForces && p.Role.Type != RoleTypeId.FacilityGuard);
            var ciPlayers = ExPlayer.Players.Where(p => p.Team == Team.ChaosInsurgency);

            if (mtfPlayers.Count() == 0 || ciPlayers.Count() == 0)
                return ActionResultFlags.StopDispose;

            mtfPlayers.ForEach(p => p.Role.Set(Team.ChaosInsurgency.GetRandomRole(), RoleChangeReason.Respawn, RoleSpawnFlags.None));
            ciPlayers.ForEach(p => p.Role.Set(Team.FoundationForces.GetRandomRole(RoleTypeId.FacilityGuard), RoleChangeReason.Respawn, RoleSpawnFlags.None));

            return ActionResultFlags.SuccessDispose;
        }
    }
}
