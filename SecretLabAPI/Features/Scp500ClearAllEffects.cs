using InventorySystem.Items.Usables;

using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;

using LabExtended.API;

namespace SecretLabAPI.Features;

/// <summary>
/// Disables all effects when the player uses the SCP-500.
/// </summary>
public static class Scp500ClearAllEffects
{
    private static void OnUsedItem(PlayerUsedItemEventArgs args)
    {
        if (args.UsableItem?.Base == null
            || args.UsableItem.Base is not Scp500
            || args.Player is not ExPlayer player)
            return;
        
        player.Effects.DisableAllEffects();
    }

    internal static void Initialize()
    {
        PlayerEvents.UsedItem += OnUsedItem;
    }
}