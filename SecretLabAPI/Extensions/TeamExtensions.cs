using PlayerRoles;

namespace SecretLabAPI.Extensions
{
    /// <summary>
    /// Provides extension methods and role groupings for working with teams and roles within the application.
    /// </summary>
    /// <remarks>The TeamExtensions class includes predefined arrays of role type identifiers for common team
    /// groupings, such as Chaos, Nine-Tailed Fox (NTF), and SCPs. It also provides extension methods to facilitate
    /// operations like selecting a random role for a given team. Use these members to simplify team and role management
    /// tasks, such as determining group membership or assigning roles based on team affiliation.</remarks>
    public static class TeamExtensions
    {
        /// <summary>
        /// Gets the set of role type identifiers representing all Chaos roles.
        /// </summary>
        /// <remarks>This array includes all roles classified as Chaos, such as Chaos Marauder, Chaos
        /// Repressor, Chaos Rifleman, and Chaos Conscript. Use this collection to check if a given role is part of the
        /// Chaos faction.</remarks>
        public static RoleTypeId[] ChaosRoles = [RoleTypeId.ChaosMarauder, RoleTypeId.ChaosRepressor, RoleTypeId.ChaosRifleman, RoleTypeId.ChaosConscript];

        /// <summary>
        /// Defines the set of role type identifiers that represent members of the Nine-Tailed Fox (NTF) unit.
        /// </summary>
        /// <remarks>This array includes all role types considered part of the NTF team, such as NTF
        /// Private, Sergeant, Specialist, Captain, and Facility Guard. Use this array to check if a given role belongs
        /// to the NTF group.</remarks>
        public static RoleTypeId[] NtfRoles = [RoleTypeId.NtfPrivate, RoleTypeId.NtfSergeant, RoleTypeId.NtfSpecialist, RoleTypeId.NtfCaptain, RoleTypeId.FacilityGuard];

        /// <summary>
        /// Defines the set of role type identifiers that represent SCP entities.
        /// </summary>
        /// <remarks>This array includes all role types considered SCPs within the application. The order
        /// of elements may be significant for certain operations. Duplicate entries may be present.</remarks>
        public static RoleTypeId[] ScpRoles = [RoleTypeId.Scp049, RoleTypeId.Scp096, RoleTypeId.Scp106, RoleTypeId.Scp173, RoleTypeId.Scp939, RoleTypeId.Scp3114, RoleTypeId.Scp049];

        /// <summary>
        /// Returns a random role associated with the specified team, excluding any roles provided.
        /// </summary>
        /// <param name="team">The team for which to select a random role.</param>
        /// <param name="exludedRoles">An optional array of roles to exclude from the selection. If specified, the returned role will not be any of
        /// these.</param>
        /// <returns>A randomly selected role of type <see cref="RoleTypeId"/> that belongs to the specified team and is not in
        /// the excluded roles list.</returns>
        /// <exception cref="NotImplementedException">Thrown if <paramref name="team"/> is not a recognized team value.</exception>
        public static RoleTypeId GetRandomRole(this Team team, params RoleTypeId[] exludedRoles)
        {
            switch (team)
            {
                case Team.ChaosInsurgency:
                    {
                        var role = ChaosRoles.RandomItem();

                        while (exludedRoles.Contains(role))
                            role = ChaosRoles.RandomItem();

                        return role;
                    }

                case Team.FoundationForces:
                    {
                        var role = NtfRoles.RandomItem();

                        while (exludedRoles.Contains(role))
                            role = NtfRoles.RandomItem();

                        return role;
                    }

                case Team.SCPs:
                    {
                        var role = ScpRoles.RandomItem();

                        while (exludedRoles.Contains(role))
                            role = ScpRoles.RandomItem();

                        return role;
                    }

                case Team.ClassD: return RoleTypeId.ClassD;
                case Team.Dead: return RoleTypeId.Spectator;
                case Team.OtherAlive: return RoleTypeId.Tutorial;
                case Team.Scientists: return RoleTypeId.Scientist;

                default: throw new NotImplementedException(team.ToString());
            }
        }
    }
}