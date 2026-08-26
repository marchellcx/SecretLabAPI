using LabExtended.API;

using LabExtended.API.Hints;
using LabExtended.API.Hints.Elements.Personal;

using LabExtended.Core;
using LabExtended.Events;

using MEC;

using NiveraAPI.IO.Configs;

namespace ObscurisCore.Features.Hud;

/// <summary>
/// Manages the HUD elements.
/// </summary>
public static class HudManager
{
    /// <summary>
    /// List of elements to be disabled in the HUD.
    /// </summary>
    [Config("hud-manager", "disabled-elements", "List of elements to be disabled in the HUD.")]
    public static List<string> DisabledElements { get; set; } = new();
    
    /// <summary>
    /// List of elements to be added to the HUD.
    /// </summary>
    public static List<Type> Elements { get; } = new()
    {
        
    };

    private static void OnPlayerJoined(ExPlayer player)
    {
        Timing.CallDelayed(1.5f, () =>
        {
            if (player?.ReferenceHub == null) // could have disconnected in those 2 secs
                return; 
            
            foreach (var type in Elements)
            {
                try
                {
                    ApiLog.Debug("HudManager", $"Adding element &1{type.Name}&r to {player.ToLogString()} ..");
                    
                    if (DisabledElements.Contains(type.Name))
                    {
                        continue;
                    }

                    if (Activator.CreateInstance(type) is not PersonalHintElement personalHintElement)
                    {
                        ApiLog.Warn("HudManager", $"Failed to create &1{type.Name}&r");
                    }
                    else
                    {
                        player.AddHintElement(personalHintElement);
                    }
                }
                catch (Exception ex)
                {
                    ApiLog.Error(ex);
                }
            }
        });
    }

    private static void Initialize()
    {
        if (Elements.Count > 0)
        {
            ExPlayerEvents.Verified += OnPlayerJoined;
        }
    }
}