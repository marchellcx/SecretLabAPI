using HarmonyLib;

namespace SecretLabAPI.Patches.Webhooks
{
    /// <summary>
    /// Fixes exception spam by not disposing the HTTP client, lmao.
    /// </summary>
    public static class WebhooksDisablePrefix
    {
        [HarmonyPatch("Webhooks.Discord.DiscordClient", "Dispose")]
        private static bool Prefix() => false;
    }
}