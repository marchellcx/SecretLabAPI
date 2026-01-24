using LabExtended.API;
using LabExtended.Events;
using LabExtended.Utilities;

using SecretLabAPI.Extensions;

namespace SecretLabAPI.Features
{
    /// <summary>
    /// Provides a static collection of alternative user nicknames mapped by unique identifiers.
    /// </summary>
    public static class AlternativeNicks
    {
        private static bool initialized = false;

        /// <summary>
        /// Gets or sets the collection of user nicknames mapped by their unique identifiers.
        /// </summary>
        public static Dictionary<string, string> Nicks { get; set; } = new()
        {
            { "example@steam", "example" },
            { "example@discord", "example" }
        };

        /// <summary>
        /// Saves the current collection of alternative nicknames to a YAML file in the application's root directory.
        /// </summary>
        public static void SaveNicks()
        {
            FileUtils.TrySaveYamlFile(SecretLab.RootDirectory, "alternative_nicks.yml", Nicks);
        }

        private static void OnVerified(ExPlayer player)
        {
            if (Nicks.TryGetValue(player.UserId, out string? nick) && !string.IsNullOrEmpty(nick))
            {
                player.ReferenceHub.nicknameSync.Network_myNickSync = nick;
                player.SendConsoleMessage($"Updated alternative nick to: {nick}");
            }
        }

        internal static void Initialize()
        {
            Nicks = FileUtils.LoadYamlFileOrDefault(SecretLab.RootDirectory, "alternative_nicks.yml", Nicks, true);

            foreach (var player in ExPlayer.Players)
            {
                if (!player.IsValidPlayer())
                    continue;

                if (!Nicks.TryGetValue(player.UserId, out string? nick) || string.IsNullOrEmpty(nick))
                    continue;

                player.ReferenceHub.nicknameSync.Network_myNickSync = nick;
                player.SendConsoleMessage($"Updated alternative nick to: {nick}");
            }

            if (initialized)
                return;

            initialized = true;

            ExPlayerEvents.Verified += OnVerified;
        }
    }
}
