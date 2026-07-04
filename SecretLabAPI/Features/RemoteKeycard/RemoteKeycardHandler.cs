using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using Interactables.Interobjects.DoorButtons;

using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;

using LabApi.Features.Wrappers;

using LabExtended.Events;
using LabExtended.Extensions;

using LabExtended.Events.Firearms;

using MapGeneration;
using MapGeneration.Distributors;
using SecretLabAPI.Extensions;

using UnityEngine;

using ElevatorDoor = Interactables.Interobjects.ElevatorDoor;

namespace SecretLabAPI.Features.RemoteKeycard;

/// <summary>
/// Allows opening doors without taking out your keycard.
/// </summary>
public static class RemoteKeycardHandler
{
    private static LayerMask mask = LayerMask.GetMask("InteractableNoPlayerCollision");
    
    private static void OnInteractingGenerator(PlayerInteractingGeneratorEventArgs args)
    {
        if (args.Generator.IsUnlocked || args.ColliderId != Scp079Generator.GeneratorColliderId.Door)
            return;
        
        if (!args.Player.CastPlayer(out var player))
            return;

        if (player.Inventory.HasKeycardPermission(args.Generator.RequiredPermissions))
        {
            args.IsAllowed = false;
            args.Generator.IsUnlocked = true;
        }
    }

    private static void OnInteractingDoor(PlayerInteractingDoorEventArgs args)
    {
        if (args.CanOpen)
            return;

        if (!args.Player.CastPlayer(out var player))
            return;

        if (player.Inventory.HasKeycardPermission(args.Door.Base.PermissionsPolicy.RequiredPermissions))
            args.CanOpen = true;
    }

    private static void OnInteractingLocker(PlayerInteractingLockerEventArgs args)
    {
        if (args.CanOpen)
            return;

        if (!args.Player.CastPlayer(out var player))
            return;

        if (player.Inventory.HasKeycardPermission(args.Chamber.RequiredPermissions))
            args.CanOpen = true;
    }

    private static void OnShot(FirearmRayCastEventArgs args)
    {
        if (args.IsAllowed && Physics.Raycast(args.Ray, out var hit, args.MaxDistance, mask))
        {
            if (hit.collider.gameObject.TryFindComponent<ButtonVariant>(out var button)
                && button.ParentDoor != null
                && button.ParentDoor.AllowInteracting(args.Player.ReferenceHub, 0))
            {
                button.ParentDoor.ServerInteract(args.Player.ReferenceHub, 0);
            }
            else if (hit.collider.gameObject.TryFindComponent<ElevatorPanel>(out var panel)
                     && panel.AssignedChamber != null
                     && panel.AssignedChamber.IsReady
                     && !DoorLockUtils.GetMode(panel.AssignedChamber.ActiveLocksAllDoors).HasFlagFast(DoorLockMode.FullLock)
                     && ElevatorDoor.AllElevatorDoors.TryGetValue(panel.AssignedChamber.AssignedGroup, out var doors)
                     && (!Decontamination.IsDecontaminating || !doors.Any(d => d.IsInZone(FacilityZone.LightContainment))) 
                     && !Warhead.IsDetonated)

            {
                panel.AssignedChamber.ServerSetDestination(panel.AssignedChamber.NextLevel, true);
            }
        }
    }
    
    private static void Initialize()
    {
        ExFirearmEvents.RayCast += OnShot;
        
        PlayerEvents.InteractingDoor += OnInteractingDoor;
        PlayerEvents.InteractingLocker += OnInteractingLocker;
        PlayerEvents.InteractingGenerator += OnInteractingGenerator;
    }
}