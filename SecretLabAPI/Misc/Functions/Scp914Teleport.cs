using LabExtended.API;

using PlayerRoles;

using MapGeneration;

using LabApi.Events.Arguments.Scp914Events;
using LabApi.Events.Arguments.ServerEvents;

using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;

using LabExtended.Core;
using LabExtended.Utilities;

using Scp914;

using SecretLabAPI.Extensions;

namespace SecretLabAPI.Misc.Functions
{
    public static class Scp914Teleport
    {
        /// <summary>
        /// Gets the current configuration setting for the SCP-914 knob used in teleportation operations.
        /// </summary>
        public static Scp914KnobSetting Setting => SecretLab.Config.Scp914TeleportSetting;

        /// <summary>
        /// Gets the probability that SCP-914 will teleport an object during operation.
        /// </summary>
        public static float Chance => SecretLab.Config.Scp914TeleportChance;

        /// <summary>
        /// Gets a mapping of facility zones to their associated teleportation distances for SCP-914 operations.
        /// </summary>
        public static Dictionary<FacilityZone, float> Zones => SecretLab.Config.Scp914TeleportZones;

        /// <summary>
        /// Gets the identifier for the LCZ-914 room, if available.
        /// </summary>
        /// <remarks>This property is set internally and may be null if the LCZ-914 room is not present or
        /// has not been initialized. Use this property to reference the LCZ-914 room in scenarios where its presence is
        /// required.</remarks>
        public static RoomIdentifier? Lcz914Room { get; private set; }

        /// <summary>
        /// Determines whether any SCP team player is currently in the LCZ 914 room or in a room directly connected to
        /// it.
        /// </summary>
        /// <remarks>This method checks all players on the SCP team and considers both direct presence in
        /// the LCZ 914 room and presence in any room connected to it.</remarks>
        /// <returns>true if at least one SCP team player is in LCZ 914 or a connected room; otherwise, false.</returns>
        public static bool AnyScpIn914OrNear()
        {
            for (var x = 0; x < ExPlayer.Players.Count; x++)
            {
                var player = ExPlayer.Players[x];

                if (player?.ReferenceHub == null)
                    continue;

                if (player.Role.Team != Team.SCPs)
                    continue;

                if (player.Position.Room == null || player.Position.Room.Name != RoomName.Lcz914)
                {
                    if (Lcz914Room != null && player.Position.Room != null)
                    {
                        if (Lcz914Room.ConnectedRooms.Any(r => player.Position.Room == r))
                        {
                            return true;
                        }
                    }
                }

                return true;
            }

            return false;
        }

        private static void OnScp914UpgradedPlayer(Scp914ProcessedPlayerEventArgs args)
        {
            if (args.Player is not ExPlayer player || player.Role.IsScp)
                return;

            if (Chance <= 0f)
                return;

            if (Zones?.Count < 1) 
                return;

            if (args.KnobSetting != Setting) 
                return;

            if (Chance < 100f && !WeightUtils.GetBool(Chance)) 
                return;

            var zones = Zones.Where(p => ExRound.Duration.TotalSeconds >= p.Value).Select(p => p.Key).ToArray();

            if (zones?.Length < 1) 
                return;

            if (!AnyScpIn914OrNear()) 
                return;

            player.RandomRoomTeleport(zones);
        }

        private static void OnMapGenerated(MapGeneratedEventArgs args)
        {
            Lcz914Room = Map.Rooms.FirstOrDefault(r => r.Name == RoomName.Lcz914)?.Base;

            if (Lcz914Room == null)
                ApiLog.Error("Scp914Teleport", "Could not find Lcz914.");
        }

        internal static void Initialize()
        {
            ServerEvents.MapGenerated += OnMapGenerated;
            Scp914Events.ProcessedPlayer += OnScp914UpgradedPlayer;
        }
    }
}