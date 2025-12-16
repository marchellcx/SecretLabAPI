using BasicOverlays.Overlays;

using HarmonyLib;

using LabExtended.API.Hints;
using LabExtended.Core;

using SecretLabAPI.Utilities.Configs;

namespace SecretLabAPI.Patches.Overlays
{
    /// <summary>
    /// Provides functionality to override the server name overlay in the Peanut Club overlay system.
    /// </summary>
    public static class BasicOverlaysServerNameOverridePatch
    {
        /// <summary>
        /// Gets the overlay options used to display the server name, or null if no overlay is configured.
        /// </summary>
        public static OverlayOptions? ServerNameOverlay { get; internal set; }

        [HarmonyPatch(typeof(PeanutClubOverlay), nameof(PeanutClubOverlay.OnUpdate))]
        private static bool UpdatePrefix(PeanutClubOverlay __instance)
        {
            if (ServerNameOverlay is null)
                return true;

            var field = AccessTools.Field(typeof(PeanutClubOverlay), "LocalData");
            var value = field.GetValue(__instance) as IEnumerable<HintData>;

            if (value is null)
            {
                ApiLog.Warn("SecretLabAPI", "PeanutClubOverlay.LocalData is null");
                return true;
            }

            var data = value.ElementAtOrDefault(1);

            if (data is null)
            {
                ApiLog.Warn("SecretLabAPI", "PeanutClubOverlay.LocalData[1] is null");
                return true;
            }

            if (!ServerNameOverlay.IsEnabled)
                data.Content = string.Empty;

            data.VerticalOffset = ServerNameOverlay.VerticalOffset;

            ServerNameOverlay = null;
            return true;
        }
    }
}
