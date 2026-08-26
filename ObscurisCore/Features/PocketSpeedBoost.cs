using CustomPlayerEffects;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabExtended.API;
using NiveraAPI.IO.Configs;

namespace ObscurisCore.Features;

/// <summary>
/// Provides functionality to apply a speed boost effect when a player escapes the Pocket Dimension.
/// </summary>
public static class PocketSpeedBoost
{
    /// <summary>
    /// Gets the intensity of the Movement Boost effect upon escaping a pocket dimension.
    /// </summary>
    [Config("pocketDimensionEscapeBoost", "intensity", "Sets the intensity of the Movement Boost effect upon escaping a pocket dimension.")]
    public static byte Intensity { get; set; } = 20;

    /// <summary>
    /// Gets the duration of the Movement Boost effect upon escaping a pocket dimension.
    /// </summary>
    [Config("pocketDimensionEscapeBoost", "duration", "Sets the duration of the Movement Boost effect upon escaping a pocket dimension.")]
    public static float Duration { get; set; } = 5f;
    
    private static void OnEscaped(PlayerLeftPocketDimensionEventArgs args)
    {
        if (Intensity == 0 || Duration == 0f)
            return;

        if (!args.IsSuccessful)
            return;

        if (args.Player is not ExPlayer player
            || player.Effects.IsActive<MovementBoost>())
            return;
        
        player.Effects.EnableEffect<MovementBoost>(Intensity, Duration, true);
    }

    private static void Initialize()
    {
        PlayerEvents.LeftPocketDimension += OnEscaped;
    }
}