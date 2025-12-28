using LabExtended.API;

using LabExtended.Commands;
using LabExtended.Commands.Attributes;
using LabExtended.Commands.Interfaces;

using LabExtended.Events;
using LabExtended.Utilities;
using SecretLabAPI.Elements.Alerts;
using System.ComponentModel;

namespace SecretLabAPI.Misc
{
    /// <summary>
    /// Provides functionality for enabling and managing developer mode on the server, restricting access to authorized
    /// users and groups during development or maintenance periods.
    /// </summary>
    /// <remarks>Developer mode allows server administrators to temporarily limit server access to specific
    /// user groups or staff members for testing, maintenance, or development purposes. When enabled, only users who
    /// meet the configured criteria—such as being Northwood staff, belonging to an allowed group, or having remote
    /// admin access (if no groups are specified)—are permitted to join. All other users are removed from the server.
    /// The developer mode state and configuration can be accessed via the static properties. Use the Enable and Disable
    /// methods to control developer mode activation.</remarks>
    public class DeveloperMode
    {
        /// <summary>
        /// Represents a server-side command that toggles developer mode on or off.
        /// </summary>
        [Command("devmode", "Toggles developer mode on / off.")]
        public class Command : CommandBase, IServerSideCommand
        {
            [CommandOverload(null, null)]
            private void Toggle()
            {
                if (IsActive)
                {
                    Disable();

                    Ok("Disabled DEVELOPER MODE");
                }
                else
                {
                    Enable();

                    Ok("Enabled DEVELOPER MODE");
                }
            }
        }

        #region Config
        /// <summary>
        /// Gets or sets the list of user groups permitted to join the server when developer mode is enabled.
        /// </summary>
        /// <remarks>If the collection is empty, any user with Remote Admin access can join the server
        /// while developer mode is active. Otherwise, only users belonging to one of the specified groups are allowed
        /// access.</remarks>
        [Description("Sets a list of groups that can join the server while developer mode is active. If empty, anyone with Remote Admin access can join.")]
        public string[] Groups { get; set; } = [];
        #endregion

        /// <summary>
        /// Whether or not develoeper mode is currently active.
        /// </summary>
        public static bool IsActive { get; private set; }

        /// <summary>
        /// Gets the current developer mode config.
        /// </summary>
        public static DeveloperMode Config { get; private set; }

        /// <summary>
        /// Enables the server's private mode, preventing unauthorized players from joining.
        /// </summary>
        /// <remarks>When called, this method ensures that only players who meet the join criteria remain
        /// connected. Any players who do not meet the requirements are removed from the server. If the server is
        /// already in private mode, this method has no effect.</remarks>
        public static void Enable()
        {
            if (IsActive)
                return;

            ExServer.Name = "<color=red>[DEVELOPER MODE]</color> " + ExServer.Name;

            ExPlayer.Players.ToList().ForEach(player =>
            {
                if (!CanJoin(player))
                {
                    KickPlayer(player);
                }
            });

            IsActive = true;
        }

        /// <summary>
        /// Disables developer mode and restores the server name to its standard format.
        /// </summary>
        /// <remarks>If developer mode is not currently active, this method has no effect. After calling
        /// this method, the <color=red>[DEVELOPER MODE]</color> prefix is removed from the server name, and the
        /// IsActive property is set to false.</remarks>
        public static void Disable()
        {
            if (!IsActive)
                return;

            ExServer.Name = ExServer.Name.Replace("<color=red>[DEVELOPER MODE]</color> ", string.Empty);

            IsActive = false;
        }

        /// <summary>
        /// Determines whether the specified player is permitted to join based on their staff status, group membership,
        /// or remote admin access.
        /// </summary>
        /// <remarks>A player is permitted to join if they are Northwood staff, belong to an allowed
        /// permissions group, or have remote admin access when no groups are configured. If the player's ReferenceHub
        /// is null, the method returns false.</remarks>
        /// <param name="player">The player to evaluate for join eligibility. Cannot be null.</param>
        /// <returns>true if the player is allowed to join; otherwise, false.</returns>
        public static bool CanJoin(ExPlayer player)
        {
            if (player?.ReferenceHub == null)
                return false;

            if (player.IsNorthwoodStaff)
                return true;

            if (Config.Groups?.Length > 0 && !string.IsNullOrEmpty(player.PermissionsGroupName) && Config.Groups.Contains(player.PermissionsGroupName))
                return true;

            if (Config.Groups?.Length < 1 && player.RemoteAdminAccess)
                return true;

            return false;
        }

        public static void KickPlayer(ExPlayer player)
        {
            if (player?.ReferenceHub == null)
                return;

            player.Kick("[PEANUT CLUB]\nServer je aktuálně v developer módu.\nPokud jste developer, ujistěte se, že máte správná oprávnění.\n\nVíce informací na našem Discordu.");
        }

        private static void OnVerified(ExPlayer player)
        {
            if (!IsActive)
                return;

            if (!CanJoin(player))
            {
                KickPlayer(player);
            }
            else
            {
                TimingUtils.AfterSeconds(() =>
                {
                    player.SendAlert(AlertType.Info, 10f, "server management", "Na serveru je aktivní <color=orange>DEVELOPER MODE</color>!\nNezapomeňte jej vypnout jakmile skončíte!", true);
                }, 2);
            }
        }

        internal static void Initialize()
        {
            ExPlayerEvents.Verified += OnVerified;

            Config = FileUtils.LoadYamlFileOrDefault<DeveloperMode>(SecretLab.RootDirectory, "developer_mode.yml", new(), true);
        }
    }
}