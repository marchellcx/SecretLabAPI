using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;

using LabExtended.API;

using MapGeneration;

using PlayerRoles;

using SecretLabAPI.Extensions;
using SecretLabAPI.Features.Elements.Alerts;

using UnityEngine;

namespace SecretLabAPI.Features;

/// <summary>
/// Provides functionality to handle events related to zombie deaths in the game.
/// </summary>
public static class ZombieDeathAlert
{
    private static void OnDied(PlayerDeathEventArgs args)
    {
        if (args.Player is not ExPlayer player
            || args.OldRole != RoleTypeId.Scp0492)
            return;
        
        var scpPlayers = ExPlayer.Players.Where(p => p.Role.Is(RoleTypeId.Scp049));
        
        var deathReason = args.DamageHandler.TranslateDeathReason(args.Attacker != null 
            ? $"<color=orange>{args.Attacker.Nickname}</color>" 
            : null);

        var deathRoom = args.OldPosition.TryGetRoom(out var room) ? room : null;
        var deathTesla = deathRoom != null && TeslaGate.AllGates.Any(
            g => g != null && g.Room != null && g.Room == deathRoom);

        foreach (var scp in scpPlayers)
        {
            if (scp?.ReferenceHub == null)
                continue;
            
            scp.SendAlert(AlertType.Warn, 15f, "Smrt SCP-049-2", 
                $"<b><color=red>SCP-049-2</color> <color=yellow>{player.Nickname}</color> byl</b>\n" +
                $"<b>{deathReason}</b>" +
                $"{(deathRoom != null 
                    ? $"\n<b>v místnosti <color=yellow>{deathRoom.Name}</color> {(deathTesla ? "s Tesla bránou" : "bez Tesla brány")}</b>\n" 
                    : "\n")} <b>ve vzdálenosti <color=red>{Mathf.CeilToInt(Vector3.Distance(args.OldPosition, scp.Position))}</color> metrů</b>.");
        }
    }
    
    internal static void Initialize()
    {
        PlayerEvents.Death += OnDied;
    }
}