using LabExtended.API.Settings;
using LabExtended.API.Settings.Menus;

using LabExtended.API.Settings.Entries;
using LabExtended.API.Settings.Entries.Buttons;

using LabExtended.Extensions;

namespace SecretLabAPI.Features.Data
{
    /// <summary>
    /// Provides a settings menu for data collection entries.
    /// </summary>
    public class DataCollectionMenu : SettingsMenu
    {
        /// <inheritdoc/>
        public override string CustomId { get; } = "DataCollection";

        /// <inheritdoc/>
        public override string Header { get; } = "<color=yellow>💬</color>| <b>Sběr dat</b></color>";

        /// <summary>
        /// Gets a list of all generated buttons.
        /// </summary>
        public Dictionary<string, SettingsTwoButtons> Buttons { get; } = new();

        /// <inheritdoc/>
        public override void BuildMenu(List<SettingsEntry> settings)
        {
            settings.WithEntry(new SettingsTextArea("DataCollection.MenuDescription",
                "V této sekci můžete nastavit funkce, kterým povolíte sběr dat." +
                "\nTuto sekci vidíte kvůli povolené možnosti \"Do Not Track\"." +
                "\nPro popis jednotlivých funkcí stačí najet kurzorem myši na ikonu otazníku u dané funkce.", string.Empty));

            foreach (var entry in DataCollection.Entries)
            {
                var button = new SettingsTwoButtons(entry.Id, entry.Name, 

                    "<color=green>Povoleno</color>",
                    "<color=red>Zakázáno</color>", 
                    
                    true, entry.Description);

                if (button?.Base != null)
                {
                    Buttons[entry.Id] = button;

                    settings.Add(button);
                }
            }
        }

        /// <inheritdoc/>
        public override void OnButtonSwitched(SettingsTwoButtons button)
        {
            base.OnButtonSwitched(button);

            if (!DataCollection.Entries.TryGetFirst(x => x.Id == button.CustomId, out var entry))
                return;

            DataCollection.OnToggled(Player, entry, button.IsAButtonActive);
        }
    }
}