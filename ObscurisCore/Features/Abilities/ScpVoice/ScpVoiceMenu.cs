using LabExtended.API.Settings;
using LabExtended.API.Settings.Menus;
using LabExtended.API.Settings.Entries;

using LabExtended.Core;

using NiveraAPI.IO.Configs;

using UnityEngine;

namespace ObscurisCore.Features.Abilities.ScpVoice;

/// <summary>
/// Represents the settings menu for the SCP Voice feature within the application.
/// This class extends the base `SettingsMenu` functionality to allow users to configure
/// and customize the behavior of the SCP Voice ability, such as key bindings and menu text.
/// </summary>
public class ScpVoiceMenu : SettingsMenu
{
    /// <summary>
    /// The header text for the SCP Voice menu.
    /// </summary>
    [Config("scp-voice", "menu-header", "The header text for the SCP Voice menu.")]
    public static string HeaderCfg { get; set; } = "SCP Voice";

    /// <summary>
    /// The key used to toggle the SCP Voice ability.   
    /// </summary>
    [Config("scp-voice", "toggle-key", "The key used to toggle the SCP Voice ability.")]
    public static KeyCode KeyBindKey { get; set; } = KeyCode.LeftAlt;

    /// <summary>
    /// The text displayed next to the keybind toggle. 
    /// </summary>
    [Config("scp-voice", "toggle-text", "The label for the keybind button.")]
    public static string KeyBindText { get; set; } = "SCP Voice Toggle";

    /// <summary>
    /// The instructional hint text displayed for the key binding used to toggle the SCP Voice feature.
    /// </summary>
    [Config("scp-voice", "toggle-hint", "The instructional hint text displayed for the key binding used to toggle the SCP Voice feature.")]
    public static string KeyBindHint { get; set; } = "SCP Voice Toggle";
    
    /// <summary>
    /// The unique identifier for the SCP Voice menu.
    /// </summary>
    public override string CustomId { get; } = "scp_voice_menu";

    /// <summary>
    /// The header text for the SCP Voice menu.
    /// </summary>
    public override string Header => HeaderCfg;
    
    /// <summary>
    /// The SCP Voice ability instance.
    /// </summary>
    public ScpVoiceAbility Ability { get; internal set; }

    /// <summary>
    /// Constructs the menu for the SCP Voice feature. This method is responsible for creating
    /// and adding the necessary settings entries to the provided settings list, enabling
    /// customization of the SCP Voice behavior.
    /// </summary>
    /// <param name="settings">The list of settings entries to be populated with this menu's options.</param>
    public override void BuildMenu(List<SettingsEntry> settings)
    {
        settings.WithEntry(SettingsKeyBind.Create("scp_voice_menu_toggle", KeyBindText, KeyBindKey, true, false, KeyBindHint));
    }

    /// <summary>
    /// Handles the event when a key bind associated with the menu is pressed. This method checks
    /// the custom ID of the key bind and attempts to trigger the SCP Voice ability if the key bind
    /// corresponds to the toggle action for the SCP Voice menu.
    /// </summary>
    /// <param name="keyBind">The key bind entry that was pressed, containing details such as its
    /// custom ID and other metadata.</param>
    public override void OnKeyBindPressed(SettingsKeyBind keyBind)
    {
        base.OnKeyBindPressed(keyBind);

        if (!keyBind.IsPressed)
            return;

        if (keyBind.CustomId != "scp_voice_menu_toggle")
        {
            ApiLog.Warn("Received keybind press for SCP Voice menu, but custom ID does not match expected value.");
            return;
        }

        if (Player?.ReferenceHub == null)
        {
            ApiLog.Warn("Received keybind press for SCP Voice menu, but player is not connected.");
            return;
        }

        if (Ability == null)
        {
            ApiLog.Warn("Received keybind press for SCP Voice menu, but no SCP Voice ability is active.");
            return;
        }

        if (!Ability.IsEnabled)
            return;
        
        ApiLog.Debug("Received keybind press for SCP Voice menu, triggering ability.");

        var result = Ability.TryUse(false);
        
        ApiLog.Debug($"Result: {result}");
    }
}