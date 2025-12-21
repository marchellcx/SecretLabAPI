using LabExtended.Core;
using LabExtended.Extensions;

using MapGeneration;

using SecretLabAPI.Actions.API;
using SecretLabAPI.Actions.Enums;
using SecretLabAPI.Actions.Attributes;

namespace SecretLabAPI.Actions.Functions.Getters
{
    /// <summary>
    /// Contains static methods for retrieving and manipulating room-related data within the SecretLabAPI environment.
    /// </summary>
    public static class MapGetters
    {
        /// <summary>
        /// Finds a random room within specific facility zones and saves it as a RoomIdentifier object in memory.
        /// </summary>
        /// <param name="context">
        /// The action context containing necessary information to execute the operation.
        /// Expected parameters include:
        /// - Zones: An array of FacilityZone values that specify the zones to search for rooms in. If no zones are provided, all available rooms will be considered.
        /// </param>
        /// <returns>
        /// An ActionResultFlags value indicating the outcome of the operation.
        /// Returns SuccessDispose if a room is successfully found and saved in memory.
        /// Returns StopDispose if no valid room is found.
        /// </returns>
        [Action("GetRandomRoom", "Finds a random room within specific facility zones.")]
        [ActionParameter("Zones", "The list of zones to search for rooms in.")]
        public static ActionResultFlags GetRandomRoom(ref ActionContext context)
        {
            context.EnsureCompiled((_, p) => p.EnsureCompiled<FacilityZone[]>(SecretLabAPI.Extensions.StringExtensions.TryParseEnumArray, []));
            
            var zones = context.GetValue<FacilityZone[]>(0);
            var rooms = zones.Length > 0
                ? RoomIdentifier.AllRoomIdentifiers.Where(r => r != null && zones.Contains(r.Zone))
                : RoomIdentifier.AllRoomIdentifiers.Where(r => r != null);

            var room = rooms.GetRandomItem();

            if (room != null)
            {
                context.SetMemory(room);
                return ActionResultFlags.SuccessDispose;
            }

            ApiLog.Warn("Actions :: GetRandomRoom", $"Failed to find a random room in zones: {string.Join(",", zones)}");
            return ActionResultFlags.StopDispose;
        }
        
        /// <summary>
        /// Retrieves a specific room and saves it as a RoomIdentifier object based on input parameters.
        /// </summary>
        /// <param name="context">
        /// The action context containing necessary information to evaluate the parameters and execute the action.
        /// Expected parameters include:
        /// - Room: The name of the room, represented as a RoomName enum value.
        /// - Zone: The zone the room should be in, represented as a FacilityZone enum value. Use None to not specify a zone.
        /// - Shape: The shape of the room, represented as a RoomShape enum value. Use Undefined to not specify a shape.
        /// </param>
        /// <returns>
        /// An ActionResultFlags value indicating the result of the operation.
        /// Returns SuccessDispose if the room is found and stored in memory.
        /// Returns StopDispose if the room cannot be found.
        /// </returns>
        [Action("GetRoom", "Gets a specific room (saves as a RoomIdentifier object).")]
        [ActionParameter("Room", "The name of the room (from the RoomName enum).")]
        [ActionParameter("Zone", "The zone the room should be in (from the FacilityZone enum, use None to not specify).")]
        [ActionParameter("Shape", "The shape of the room (from the RoomShape enum, use Undefined to not specify).")]
        public static ActionResultFlags GetRoom(ref ActionContext context)
        {
            context.EnsureCompiled((i, p) =>
            {
                return i switch
                {
                    0 => p.EnsureCompiled(Enum.TryParse, RoomName.Unnamed),
                    1 => p.EnsureCompiled(Enum.TryParse, FacilityZone.None),
                    2 => p.EnsureCompiled(Enum.TryParse, RoomShape.Undefined),
                    
                    _ => false
                };
            });
            
            var room = context.GetValue<RoomName>(0);
            var zone = context.GetValue<FacilityZone>(1);
            var shape = context.GetValue<RoomShape>(2);
            
            if (RoomUtils.TryFindRoom(room,
                    zone is FacilityZone.None ? null : zone,
                    shape is RoomShape.Undefined ? null : shape, out var result))
            {
                context.SetMemory(result);
                return ActionResultFlags.SuccessDispose;
            }

            ApiLog.Warn("Actions :: GetRoom", $"Could not find room! (Name={room}; Zone={zone}; Shape={shape})");
            return ActionResultFlags.StopDispose;
        }
    }
}