using HarmonyLib;

using Webhooks.Discord;

namespace SecretLabAPI.Patches.Webhooks
{
    /// <summary>
    /// Fixes exception spam by not disposing the HTTP client, lmao.
    /// </summary>
    public static class WebhooksDisablePrefix
    {
        [HarmonyPatch(typeof(DiscordClient), nameof(DiscordClient.Dispose))]
        private static bool Prefix() => false;
    }
}