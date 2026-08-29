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

namespace ObscurisCore.Features.Custom.Teams.RedRightHand;

/// <summary>
/// Manages the button used to summon a wave.
/// </summary>
public static class RedRightHandButton
{
    /// <summary>
    /// Gets whether or not the Red Right Hand button is enabled.
    /// </summary>
    [Config("red-right-hand", "enabled", "Whether or not the Red Right Hand button is enabled.")]
    public static bool Enabled { get; set; }

    /// <summary>
    /// Gets the name of the button schematic.
    /// </summary>
    [Config("red-right-hand", "button-schematic-name", "The name of the button schematic.")]
    public static string SchematicName { get; set; } = "RedRightHandButton";

    /// <summary>
    /// Gets the name of the button position.
    /// </summary>
    [Config("red-right-hand", "button-position-name", "The name of the button position.")]
    public static string PositionName { get; set; } = "RedRightHandButtonPosition";

    /// <summary>
    /// Gets the name of the button animator.
    /// </summary>
    [Config("red-right-hand", "button-animator-name", "The name of the button animator.")]
    public static string AnimatorName { get; set; } = "RedRightHandButtonAnimator";

    /// <summary>
    /// Gets the name of the button press animation.
    /// </summary>
    [Config("red-right-hand", "button-press-animation-name", "The name of the button press animation.")]
    public static string PressAnimationName { get; set; } = "RedRightHandButtonPressAnimation";

    /// <summary>
    /// Gets the name of the button press animation.
    /// </summary>
    [Config("red-right-hand", "button-idle-animation-name", "The name of the button idle animation.")]
    public static string IdleAnimationName { get; set; } = "RedRightHandButtonIdleAnimation";

    /// <summary>
    /// The maximum amount of players to spawn.
    /// </summary>
    [Config("red-right-hand", "button-max-players", "The maximum amount of players to spawn.")]
    public static int MaxPlayers { get; set; } 

    /// <summary>
    /// The minimum amount of players to spawn.
    /// </summary>
    [Config("red-right-hand", "button-min-players", "The minimum amount of players to spawn.")]
    public static int MinPlayers { get; set; }

    /// <summary>
    /// Whether or not the button was used this round.
    /// </summary>
    public static bool WasUsed { get; private set; }

    /// <summary>
    /// Gets the spawned button schematic.
    /// </summary>
    public static SchematicObject? ButtonObject { get; private set; }

    /// <summary>
    /// Gets the spawned button interactable toy.
    /// </summary>
    public static InteractableToy? ButtonInteractable { get; private set; }

    /// <summary>
    /// Gets called once the button is succesfully used.
    /// </summary>
    public static event Action<ExPlayer>? Used;

    /// <summary>
    /// Gets called once the button is used resulting in a fail (missing O5 keycard, not enough players to spawn etc.).
    /// </summary>
    public static event Action<ExPlayer>? Failed;

    private static void OnInteracted(PlayerSearchedToyEventArgs args)
    {
        if (!Enabled)
            return;

        if (WasUsed || ButtonInteractable?.Base == null)
            return;

        if (args.Interactable?.Base == null || ButtonInteractable?.Base == null)
            return;

        if (args.Interactable.Base != ButtonInteractable.Base)
            return;

        if (args.Player is not ExPlayer player)
            return;

        if (ButtonObject != null)
        {
            try
            {
                ButtonObject.AnimationController.Play(PressAnimationName, AnimatorName);
            }
            catch
            {
                ApiLog.Warn("Red Right Hand Button", "Could not play the button press animation!");
            }
        }

        if (!player.Inventory.HasItem(ItemType.KeycardO5))
        {
            player.SendAlert(AlertType.Warn, 10f, "Red Right Hand", "Pro zavolání týmu <color=red>Red Right Hand</color> je třeba mít <b>O5 kartu</b>!");

            Failed?.InvokeSafe(player);
            return;
        }

        if (RedRightHandTeam.Singleton.Spawn(MinPlayers, MaxPlayers).SpawnedWave != null)
        {
            WasUsed = true;

            player.SendAlert(AlertType.Info, 10f, "Red Right Hand", "<b><color=green>Úspěšně</color> jsi zavolal</b>\n<color=red><b>Red Right Hand</b></color>!");

            Used?.InvokeSafe(player);
        }
        else
        {
            player.SendAlert(AlertType.Warn, 10f, "Red Right Hand", "Aktuálně <color=red>nelze</color> zavolat tým <color=red>Red Right Hand</color>, zkus to znova později!");

            Failed?.InvokeSafe(player);
        }
    }

    private static void OnStarted()
    {
        if (!Enabled)
            return;

        WasUsed = false;
        ButtonObject = null;

        if (MapLocations.TryFindPrefixed(PositionName, out Vector3 position, out Quaternion rotation))
        {
            if (ObjectSpawner.TrySpawnSchematic(SchematicName, position, rotation, out var schematic))
            {
                ButtonObject = schematic;

                try
                {
                    ButtonObject?.AnimationController.Play(IdleAnimationName, AnimatorName);
                }
                catch
                {
                    ApiLog.Warn("Red Right Hand Button", "Could not play the button idle animation!");
                }

                ButtonInteractable = new(position, rotation) { Scale = Vector3.one / 8.5f };
                ButtonInteractable.Shape = InvisibleInteractableToy.ColliderShape.Box;
                ButtonInteractable.InteractionDuration = 1f;
            }
            else
            {
                ApiLog.Warn("Red Right Hand Button", "The Red Right Hand button schematic could not be spawned!");
            }
        }
        else
        {
            ApiLog.Warn("Red Right Hand Button", "Could not find the spawn point for Red Right Hand button schematic");
        }
    }

    private static void Initialize()
    {
        ExRoundEvents.Started += OnStarted;
        PlayerEvents.SearchedToy += OnInteracted;
    }
}