using HarmonyLib;

using InventorySystem.Items.Usables;

using LabExtended.API.Custom.Items;

using SecretLabAPI.Features.Items;

namespace SecretLabAPI.Patches.NoColaEffect;

/// <summary>
/// Prevents the SCP-207 and Anti-SCP-207 from activating.
/// </summary>
public static class ColaEffectPatch
{
    [HarmonyPatch(typeof(Consumable), nameof(Consumable.ActivateEffects))]
    private static bool Prefix(Consumable __instance)
    {
        if (__instance is not AntiScp207 and not Scp207)
            return true;

        return !CustomItem.IsCustomItem<NoEffectCola>(__instance.ItemSerial, out _);
    }
}