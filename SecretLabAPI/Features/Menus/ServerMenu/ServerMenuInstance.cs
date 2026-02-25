using LabExtended.API.Settings.Entries;
using LabExtended.API.Settings.Entries.Buttons;

using LabExtended.API.Settings.Menus;

using LabExtended.Extensions;

using SecretLabAPI.Features.Elements.Alerts;

namespace SecretLabAPI.Features.Menus.ServerMenu
{
    /// <summary>
    /// Server management menu.
    /// </summary>
    public class ServerMenuInstance : SettingsMenu
    {
        private HashSet<string> adminEvents = new();

        /// <inheritdoc/>
        public override string Header { get; } = "Server List";

        /// <inheritdoc/>
        public override string CustomId { get; } = "secretlabapi.servermenu";

        /// <inheritdoc/>
        public override int Priority { get; } = 1;

        /// <summary>
        /// Gets a dictionary of created join buttons for each server, keyed by server alias.
        /// </summary>
        public Dictionary<string, SettingsButton> JoinButtons { get; } = new();

        /// <summary>
        /// Gets a dictionary of created admin buttons, keyed by server alias.
        /// </summary>
        public Dictionary<string, SettingsTwoButtons> AdminButtons { get; } = new();

        /// <inheritdoc/>
        public override void BuildMenu(List<SettingsEntry> settings)
        {
            foreach (var server in ServerMenuManager.ServersByAlias)
            {
                var joinButton = CreateJoinButton(server.Value);
                var adminButton = CreateAdminButtons(server.Value);

                adminButton.IsHidden = !Player.IsOnlineAndVerified || server.Value.Permission == null 
                    || !Player.HasPermission(server.Value.Permission);

                JoinButtons.Add(server.Key, joinButton);
                AdminButtons.Add(server.Key, adminButton);

                settings.Add(joinButton);
                settings.Add(adminButton);
            }
        }

        /// <summary>
        /// Synchronizes the menu display to reflect the current running status of all servers managed by the
        /// ServerMenuManager.
        /// </summary>
        public void SyncMenu()
        {
            var changed = false;

            foreach (var server in ServerMenuManager.ServersByAlias)
            {
                if (JoinButtons.TryGetValue(server.Key, out var joinButton))
                {
                    if (server.Value.isRunning && !joinButton.Base.ButtonText.EndsWith("Připojit"))
                    {
                        joinButton.Base.Label = $"<color=blue>🌐</color> | <color=green>{server.Value.Alias}</color>";
                        joinButton.Base.ButtonText = "🚀 | Připojit";

                        changed = true;
                    }
                    else if (!server.Value.isRunning && !joinButton.Base.ButtonText.EndsWith("Offline"))
                    {
                        joinButton.Base.Label = $"<color=blue>🌐</color> | <color=red>{server.Value.Alias}</color>";
                        joinButton.Base.ButtonText = "🛑 | Offline";

                        changed = true;
                    }
                }
            }

            if (!changed)
                return;

            SyncEntries();
        }

        /// <inheritdoc/>
        public override void OnButtonTriggered(SettingsButton button)
        {
            base.OnButtonTriggered(button);

            if (!JoinButtons.TryGetKey(button, out var alias))
            {
                Player.SendAlert(AlertType.Warn, 5f, "Server Menu", "<color=red>Neplatné tlačítko připojení k serveru!</color>");
                return;
            }

            if (!ServerMenuManager.ServersByAlias.TryGetValue(alias, out var server))
            {
                Player.SendAlert(AlertType.Warn, 5f, "Server Menu", "<color=red>Server nenalezen!</color>");
                return;
            }

            if (server.isRunning)
            {
                Player.SendAlert(AlertType.Info, 5f, "Server Menu", $"Přeposílám tě k serveru <color=green>{server.Alias}</color>...");
                Player.RedirectToServer(server.Port);

                return;
            }
            else
            {
                Player.SendAlert(AlertType.Warn, 5f, "Server Menu", $"Server <color=red>{server.Alias}</color> je momentálně offline!");
                return;
            }
        }

        /// <inheritdoc/>
        public override void OnButtonSwitched(SettingsTwoButtons button)
        {
            base.OnButtonSwitched(button);

            return;

            if (!AdminButtons.TryGetKey(button, out var alias))
            {
                Player.SendAlert(AlertType.Warn, 5f, "Server Menu", "Neplatné tlačítko ovládání serveru!");
                return;
            }

            if (adminEvents.Add(alias))
                return;

            if (!ServerMenuManager.ServersByAlias.TryGetValue(alias, out var server))
            {
                Player.SendAlert(AlertType.Warn, 5f, "Server Menu", "Server nenalezen!");
                return;
            }

            if (!Player.HasPermission(server.Permission))
            {
                Player.SendAlert(AlertType.Warn, 5f, "Server Menu", "Nemáte oprávnění k ovládání tohoto serveru!");
                return;
            }

            if (button.IsAButtonActive)
            {
                if (ServerMenuManager.StartServer(server))
                {
                    Player.SendAlert(AlertType.Info, 5f, "Server Menu", "Server spuštěn!");
                }
                else
                {
                    Player.SendAlert(AlertType.Warn, 5f, "Server Menu", "Nepodařilo se spustit server!");
                }
            }
            else
            {
                if (ServerMenuManager.StopServer(server))
                {
                    Player.SendAlert(AlertType.Info, 5f, "Server Menu", "Server vypnut!");

                }
                else
                {
                    Player.SendAlert(AlertType.Warn, 5f, "Server Menu", "Nepodařilo se vypnout server!");
                }
            }
        }

        internal void CheckPermissions()
        {
            foreach (var server in ServerMenuManager.ServersByAlias)
            {
                if (!AdminButtons.TryGetValue(server.Key, out var adminButton))
                    continue;

                adminButton.IsHidden = server.Value.Permission == null || !Player.HasPermission(server.Value.Permission);
            }
        }

        private SettingsButton CreateJoinButton(ServerMenuInfo server)
        {
            if (server.isRunning)
            {
                return new(
                    $"secretlabapi.servermenu.joinbutton.{server.Alias}",
                    $"<color=blue>🌐</color> | <color=green>{server.Alias}</color>",
                    $"🚀 | Připojit",
                    server.Description);
            }
            else
            {
                return new(
                    $"secretlabapi.servermenu.joinbutton.{server.Alias}",
                    $"<color=blue>🌐</color> | <color=red>{server.Alias}</color>",
                    $"🛑 | Offline",
                    server.Description);
            }
        }

        private SettingsTwoButtons CreateAdminButtons(ServerMenuInfo server)
        {
            return SettingsTwoButtons.Create(
                $"secretlabapi.servermenu.adminbutton.{server.Alias}",
                "🛠️ | Ovládání serveru",
                "<color=green>Zapnout</color>",
                "<color=red>Vypnout</color>",
                true,
                server.Description);
        }
    }
}