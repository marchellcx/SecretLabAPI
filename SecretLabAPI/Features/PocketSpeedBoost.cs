using CustomPlayerEffects;

using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;

using LabExtended.API;

namespace SecretLabAPI.Features;

/// <summary>
/// Provides functionality to apply a speed boost effect when a player escapes the Pocket Dimension.
/// </summary>
public static class PocketSpeedBoost
{
    /// <summary>
    /// Gets the intensity of the Movement Boost effect upon escaping a pocket dimension.
    /// </summary>
    public static byte Intensity => SecretLab.Config.PocketSpeedBoostIntensity;
    
    /// <summary>
    /// Gets the duration of the Movement Boost effect upon escaping a pocket dimension.
    /// </summary>
    public static float Duration => SecretLab.Config.PocketSpeedBoostDuration;
    
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

    internal static void Initialize()
    {
        PlayerEvents.LeftPocketDimension += OnEscaped;
    }
}