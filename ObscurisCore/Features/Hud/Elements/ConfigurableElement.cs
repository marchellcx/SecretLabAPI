using LabExtended.API.Enums;

using LabExtended.API.Settings.Menus;
using LabExtended.API.Settings.Entries;

using LabExtended.API.Hints.Elements.Personal;
using LabExtended.API.Settings;
using LabExtended.API.Settings.Entries.Dropdown;

using UserSettings.ServerSpecific;

namespace ObscurisCore.Features.Hud.Elements;

/// <summary>
/// Represents a configurable element within the HUD system, providing settings and customization options.
/// </summary>
public abstract class ConfigurableElement : PersonalHintElement
{
    /// <summary>
    /// Represents a menu for configuring a configurable element.
    /// </summary>
    public class ConfigurableElementMenu : SettingsMenu
    {
        /// <summary>
        /// The element to configure.
        /// </summary>
        public ConfigurableElement Element { get; }

        /// <summary>
        /// The header of the menu.
        /// </summary>
        public override string Header { get; }

        /// <summary>
        /// The custom ID of the menu.
        /// </summary>
        public override string CustomId { get; }
        
        /// <summary>
        /// The slider for the offset.
        /// </summary>
        public SettingsSlider OffsetSlider { get; private set; }
        
        /// <summary>
        /// The dropdown for the alignment.
        /// </summary>
        public SettingsDropdown AlignmentDropdown { get; private set; }

        /// <summary>
        /// Creates a new menu for the specified element.
        /// </summary>
        public ConfigurableElementMenu(ConfigurableElement element)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            Element = element;

            Header = $"HUD | {element.Name}";
            CustomId = $"hud_config_{element.Name}";
        }

        /// <summary>
        /// Builds the menu for the configurable element by adding the required settings entries.
        /// </summary>
        /// <param name="settings">The list of settings entries to be populated with controls for the menu.</param>
        public override void BuildMenu(List<SettingsEntry> settings)
        {
            settings.Add(OffsetSlider = SettingsSlider.Create($"{CustomId}_offset", "Offset", -15f, 15f, 0f, true));
            settings.Add(AlignmentDropdown = SettingsDropdown.Create($"{CustomId}_alignment", "Alignment", 0, SSDropdownSetting.DropdownEntryType.Scrollable, BuildAlignmentDropdown));
        }

        /// <summary>
        /// Handles the logic executed when the slider value is changed.
        /// </summary>
        /// <param name="slider">The slider whose value has been modified.</param>
        public override void OnSliderMoved(SettingsSlider slider)
        {
            base.OnSliderMoved(slider);

            if (slider != OffsetSlider)
                return;

            if (Player?.ReferenceHub == null)
                return;
            
            Element.offset = slider.Value;
        }

        /// <summary>
        /// Handles the logic executed when a dropdown option is selected.
        /// </summary>
        /// <param name="dropdown">The dropdown menu where an option was selected.</param>
        /// <param name="option">The selected option from the dropdown.</param>
        public override void OnDropdownSelected(SettingsDropdown dropdown, SettingsDropdownOption option)
        {
            base.OnDropdownSelected(dropdown, option);
            
            if (dropdown != AlignmentDropdown)
                return;
            
            if (Player?.ReferenceHub == null)
                return;

            if (option is not SettingsDropdownOption<HintAlign> alignOption)
                return;
            
            Element.align = alignOption.CastData;
        }

        private void BuildAlignmentDropdown(SettingsDropdown dropdown)
        {
            foreach (var option in EnumUtils<HintAlign>.Values)
            {
                dropdown.AddOption(option, option.ToString());
            }
        }
    }

    private ConfigurableElementMenu menu;

    internal int spacing;
    internal float offset;
    internal HintAlign align;

    /// <summary>
    /// The name of the element.
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// The default vertical offset of the element.
    /// </summary>
    public abstract float DefaultOffset { get; }
    
    /// <summary>
    /// The default pixel spacing of the element.
    /// </summary>
    public abstract int DefaultPixelSpacing { get; }

    /// <summary>
    /// The default alignment of the element.
    /// </summary>
    public abstract HintAlign DefaultAlignment { get; }

    /// <summary>
    /// The alignment of the element.
    /// </summary>
    public override HintAlign Alignment => align;

    /// <summary>
    /// The pixel spacing of the element.
    /// </summary>
    public override int PixelSpacing => spacing;

    /// <summary>
    /// The vertical offset of the element.
    /// </summary>
    public override float VerticalOffset => offset;

    /// <summary>
    /// Executes the logic required when the configurable element is enabled.
    /// </summary>
    public override void OnEnabled()
    {
        base.OnEnabled();

        offset = DefaultOffset;
        align = DefaultAlignment;
        spacing = DefaultPixelSpacing;
        
        menu = new(this);

        Player.AddMenu(menu);
    }

    /// <summary>
    /// Executes the logic required when the configurable element is disabled.
    /// </summary>
    public override void OnDisabled()
    {
        if (menu != null && Player?.ReferenceHub != null)
            menu.HideMenu();
    }
}