using LabExtended.API;

using MapGeneration;

using PlayerRoles.PlayableScps.Scp106;

using UnityEngine;

namespace SecretLabAPI.Extensions
{
    /// <summary>
    /// Extensions for room utils.
    /// </summary>
    public static class RoomExtensions
    {
        public const float MaxSurfaceY = 302f;
        
        /// <summary>
        /// Finds a safe position within a specified room for the given player.
        /// </summary>
        /// <param name="room">The room identifier in which to find a safe position.</param>
        /// <param name="player">The player for whom the safe position is being found.</param>
        /// <returns>A <c>Vector3</c> representing the safe position within the room.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <c>room</c> parameter is null.</exception>
        /// <exception cref="Exception">Thrown when no safe positions can be found for the given room.</exception>
        public static Vector3 GetSafePosition(this RoomIdentifier room, ExPlayer player)
        {
            if (room == null)
                throw new ArgumentNullException(nameof(room));

            var poses = SafeLocationFinder.GetLocations(
                r => r != null && r.Link.Rooms?.Length > 0 && r.Link.Rooms.Contains(room),
                d => d != null && d.Rooms?.Length > 0 && d.Rooms.Contains(room));

            if (poses?.Count < 1)
                throw new($"Could not get safe poses for room '{room}'");

            var pose = Scp106PocketExitFinder.GetRandomPose(poses.ToArray());
                
            var range = room.Zone is FacilityZone.Surface
                ? 11f
                : 45f;
            
            var position = SafeLocationFinder.GetSafePositionForPose(pose, range, player.Role.MovementModule!.CharController);

            if (room.Zone is FacilityZone.Surface) position.y = Mathf.Min(MaxSurfaceY, position.y);
            return position;
        }
    }
}