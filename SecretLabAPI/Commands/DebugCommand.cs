using Interactables.Interobjects.DoorUtils;

using LabApi.Features.Wrappers;

using LabExtended.API;
using LabExtended.Extensions;

using LabExtended.Commands;
using LabExtended.Commands.Attributes;
using LabExtended.Commands.Interfaces;

using System.Text;

using PlayerRoles;
using PlayerRoles.PlayableScps.Scp079;
using MEC;
using PlayerStatsSystem;

namespace SecretLabAPI.Commands
{
    /// <summary>
    /// Represents a debug command used for testing and diagnostic purposes on the server side.
    /// </summary>
    [Command("debug", "Debug command for testing purposes")]
    public class DebugCommand : CommandBase, IServerSideCommand
    {
        /// <summary>
        /// Gets the collection of available actions, each mapped by a unique string key.
        /// </summary>
        public static Dictionary<string, Action<ExPlayer, string[], StringBuilder>> Actions { get; } = new()
        {
            { "gen_engage", Engage },
            { "gen_overcharge", Overcharge },

            { "door_debug", Door },
            { "door_locks", DoorLocks }
        };

        [CommandOverload("Show debug information for a given key", null)]
        private void Show(
            [CommandParameter("Key", "The key of the action.")] string key,
            [CommandParameter("Arguments", "The list of arguments for the action (separated by ;).")] string? arguments = null)
        {
            if (!Actions.TryGetValue(key, out var action))
            {
                Fail($"No action found for key '{key}'");
                return;
            }

            Ok(x =>
            {
                x.AppendLine();
                x.AppendLine($"Output of action '{key}'");

                try
                {
                    action(Sender, (arguments ?? string.Empty).Split(';'), x);
                }
                catch (Exception ex)
                {
                    x.AppendLine($"An error occurred while executing the action for key '{key}': {ex.Message}");
                    return;
                }
            });
        }

        private static void Overcharge(ExPlayer player, string[] args, StringBuilder builder)
        {
            Engage(player, args, builder);

            if (!ExPlayer.AllPlayers.TryGetFirst(x => x.Role.Is(RoleTypeId.Scp079), out var scp079))
            {
                builder.AppendLine("No SCP-079 player found.");
                return;
            }

            builder.AppendLine($"Found SCP-079 player: {scp079.Nickname}");

            var recontainer = UnityEngine.Object.FindFirstObjectByType<Scp079Recontainer>();

            if (recontainer == null)
            {
                builder.AppendLine("SCP-079 Recontainer not found.");
                return;
            }

            recontainer.Recontain(true);

            builder.AppendLine("SCP-079 Overcharge triggered.");

            Timing.CallDelayed(3f, () =>
            {
                if (scp079?.ReferenceHub != null && scp079.Role.Is(RoleTypeId.Scp079))
                {
                    scp079.ReferenceHub.playerStats.KillPlayer(new RecontainmentDamageHandler(player.Footprint));
                }
            });
        }

        private static void Engage(ExPlayer player, string[] args, StringBuilder builder)
        {
            var generators = Map.Generators.Where(x => x?.Base != null && !x.Engaged).ToList();

            foreach (var generator in generators)
                generator.Engaged = true;

            builder.AppendLine($"Engaged {generators.Count} generators.");
        }

        private static void Door(ExPlayer _, string[] args, StringBuilder builder)
        {
            if (args.Length < 1 || !int.TryParse(args[0], out var doorId))
            {
                builder.AppendLine("Invalid or missing door ID.");
                return;
            }

            var door = Map.Doors.FirstOrDefault(x => x?.Base != null && x.Base.netId == doorId)?.Base;

            if (door == null)
            {
                builder.AppendLine($"Door with ID {doorId} not found.");
                return;
            }

            builder.AppendLine(
                $"DoorID: {door.DoorId}\n" +
                $"DoorName: {door.DoorName}\n" +
                $"GameObjectName: {door.gameObject.name}\n" +
                $"NetworkID: {door.netId}\n" +
                $"InstanceID: {door.GetInstanceID()}\n" +
                $"Locks: {door.ActiveLocks} ({(DoorLockReason)door.ActiveLocks}\n" +
                $"LockMode: {DoorLockUtils.GetMode((DoorLockReason)door.ActiveLocks)}\n" +
                $"TargetState: {door.TargetState}\n" +
                $"GetExactState: {door.GetExactState()}\n" +
                $"IsConsideredOpen: {door.IsConsideredOpen()}\n" +
                $"RoomsAlreadyRegistered: {door.RoomsAlreadyRegistered}\n" +
                $"DeniedCooldown: {door.DeniedCooldown}\n" +
                $"IsMoving: {door.IsMoving}\n" +
                $"IsVisibleThrough: {door.IsVisibleThrough}\n" +
                $"Permissions: {door.PermissionsPolicy.RequireAll} ({door.PermissionsPolicy.RequiredPermissions})\n" +
                $"_prevLock: {door._prevLock}\n" +
                $"_prevState: {door._prevState}\n" +
                $"Rooms: {(door.Rooms == null ? "(null)" : string.Join(",", door.Rooms.Select(x => $"{x.Name} ({x.Zone}; {x.Shape})")))}");
        }

        private static void DoorLocks(ExPlayer _, string[] args, StringBuilder builder)
        {
            var doors = Map.Doors.Where(x => x?.Base != null && x.Base.ActiveLocks != 0).ToList();

            if (doors.Count == 0)
            {
                builder.AppendLine("No doors with active locks found.");
                return;
            }

            builder.AppendLine($"Found {doors.Count} doors with active locks:");

            foreach (var door in doors)
            {
                builder.AppendLine($"[{door.Base.GetType().Name} / {door.Base.netId}] Active Locks: {(DoorLockReason)door.Base.ActiveLocks} " +
                    $"(Mode: {DoorLockUtils.GetMode((DoorLockReason)door.Base.ActiveLocks)})");
            }
        }
    }
}