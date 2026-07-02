using CustomPlayerEffects;

using InventorySystem.Items.Usables;

using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;

using LabExtended.API;

using NiveraAPI.IO.Configs;

namespace SecretLabAPI.Features.Misc;

/// <summary>
/// Disables all effects when the player uses the SCP-500.
/// </summary>
public static class Scp500ClearAllEffects
{
    /// <summary>
    /// Gets or sets the list of effects that SCP-500 will not disable.
    /// </summary>
    [Config("scp500ClearAllEffects", "ignoreEffects", "List of effects that SCP-500 will not disable.")]
    public static List<string> IgnoreEffects { get; set; } = new()
    {
        nameof(PocketCorroding),
        nameof(Corroding)
    };
    
    private static void OnUsedItem(PlayerUsedItemEventArgs args)
    {
        if (args.UsableItem?.Base == null
            || args.UsableItem.Base is not Scp500
            || args.Player is not ExPlayer player)
            return;

        foreach (var effect in player.ActiveEffects.ToArray())
        {
            if (IgnoreEffects.Contains(effect.GetType().Name))
                continue;
            
            effect.ServerDisable();
        }
    }

    private static void Initialize()
    {
        PlayerEvents.UsedItem += OnUsedItem;
    }
}