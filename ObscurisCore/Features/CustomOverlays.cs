using LabExtended.API.Hints;

using NiveraAPI.IO.Configs;
using NiveraAPI.Utilities;

using ObscurisCore.Features.Elements;
using ObscurisCore.Patches;
using ObscurisCore.Utilities.Configs;

namespace ObscurisCore.Features;

/// <summary>
/// Provides functionality for configuring and managing custom overlays in the application.
/// Implements the <see cref="IInvokeOnLoad"/> interface to initialize overlays when loaded.
/// </summary>
public static class CustomOverlays
{
    /// <summary>
    /// Gets or sets a dictionary of static overlays, where the key represents
    /// the overlay identifier and the value represents the associated overlay content.
    /// </summary>
    [Config("overlays", "list", "List of static overlays.")]
    public static Dictionary<string, OverlayOptions> StaticOverlays { get; set; } = new()
    {
        { "example", new() }
    };
    
    private static void Initialize()
    {
        foreach (var pair in StaticOverlays)
        {
            if (string.IsNullOrEmpty(pair.Key)
                || string.IsNullOrEmpty(pair.Value?.OverlayString?.Value))
                continue;
            
            if (pair.Key == "ServerName")
            {
                BasicOverlaysServerNameOverridePatch.ServerNameOverlay = pair.Value;
            }
            else
            {
                new StringOverlay(pair.Value) { CustomId = pair.Key }.AddHintElement();
            }
        }
    }
}