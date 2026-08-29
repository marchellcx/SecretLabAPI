using AdminToys;

using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;

using LabExtended.API;
using LabExtended.API.Toys;

using LabExtended.Core;
using LabExtended.Events;
using LabExtended.Extensions;

using NiveraAPI.IO.Configs;

using ObscurisCore.Features.Elements.Alerts;

using ProjectMER.Features;
using ProjectMER.Features.Objects;

using UnityEngine;

namespace ObscurisCore.Features.Custom.Teams.Archangels;

/// <summary>
/// Manages the spawning radio.
/// </summary>
public static class ArchangelsRadio
{
    /// <summary>
    /// Gets the number to divide the scale of the interactable toy by.
    /// </summary>
    public const float ScaleFactor = 8.5f;

    /// <summary>
    /// Gets or sets a value indicating whether the Archangels radio is enabled.
    /// </summary>
    [Config("archangels", "enabled", "Whether or not the Archangels radio is enabled")]
    public static bool Enabled { get; set; }

    /// <summary>
    /// Gets the name of the radio schematic.
    /// </summary>
    [Config("archangels", "radio-schematic-name", "The name of the radio schematic")]
    public static string SchematicName { get; set; } = "ArchangelsRadio";

    /// <summary>
    /// Gets the name of the radio position.
    /// </summary>
    [Config("archangels", "radio-position-name", "The name of the radio position")]
    public static string PositionName { get; set; } = "ArchangelsRadioSpawn";

    /// <summary>
    /// The maximum amount of players to summon once used.
    /// </summary>
    [Config("archangels", "max-players", "The maximum amount of players to summon once used")]
    public static int MaxPlayers { get; set; }

    /// <summary>
    /// The minimum amount of players required to summon.
    /// </summary>
    [Config("archangels", "min-players", "The minimum amount of players required to summon")]
    public static int MinPlayers { get; set; }

    /// <summary>
    /// Whether or not the radio was already used this round.
    /// </summary>
    public static bool WasUsed { get; private set; }

    /// <summary>
    /// Gets the spawned radio schematic.
    /// </summary>
    public static SchematicObject? RadioObject { get; private set; }

    /// <summary>
    /// Gets the spawned radio interactable toy.
    /// </summary>
    public static InteractableToy? RadioInteractable { get; private set; }

    /// <summary>
    /// Gets called once a player succesfully uses the radio.
    /// </summary>
    public static event Action<ExPlayer>? Used;

    /// <summary>
    /// Gets called once a player fails to use the radio (not enough players to spawn a wave, etc.)
    /// </summary>
    public static event Action<ExPlayer>? Failed;

    private static void OnInteracted(PlayerSearchedToyEventArgs args)
    {
        if (!Enabled)
            return;

        if (WasUsed || RadioInteractable?.Base == null)
            return;

        if (args.Interactable?.Base == null || RadioInteractable?.Base == null)
            return;

        if (args.Interactable.Base != RadioInteractable.Base)
            return;

        if (args.Player is not ExPlayer player)
            return;

        if (ArchangelsTeam.Singleton.Spawn(MinPlayers, MaxPlayers).SpawnedWave != null)
        {
            WasUsed = true;

            player.SendAlert(AlertType.Info, 10f, "Archangels", "<b><color=green>Úspěšně</color> jsi zavolal</b>\n<color=green><b>Archangels</b></color>!");

            Used?.InvokeSafe(player);
        }
        else
        {
            player.SendAlert(AlertType.Warn, 10f, "Archangels", "Aktuálně <color=red>nelze</color> zavolat tým <color=green>Archangels</color>, zkus to znova později!");

            Failed?.InvokeSafe(player);
        }
    }

    private static void OnStarted()
    {
        if (!Enabled)
            return;

        WasUsed = false;

        RadioObject = null;
        RadioInteractable = null;

        if (MapLocations.TryFindPrefixed(PositionName, out Vector3 position, out Quaternion rotation))
        {
            if (ObjectSpawner.TrySpawnSchematic(SchematicName, position, rotation, out var schematic))
            {
                RadioObject = schematic;

                RadioInteractable = new(position, rotation)
                {
                    InteractionDuration = 1f,
                    Scale = Vector3.one / ScaleFactor,
                    Shape = InvisibleInteractableToy.ColliderShape.Box,
                };
            }
            else
            {
                ApiLog.Warn("Archangels Radio", "Could not spawn the radio schematic");
            }
        }
        else
        {
            ApiLog.Warn("Archangels Radio", "Could not find the spawn point for Archangels radio");
        }
    }

    private static void Initialíze()
    {
        ExRoundEvents.Started += OnStarted;
        PlayerEvents.SearchedToy += OnInteracted;
    }
}