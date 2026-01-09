using LabExtended.API.Settings;

using LabExtended.API.Settings.Menus;
using LabExtended.API.Settings.Entries;

using UnityEngine;

using LabExtended.Core;

namespace SecretLabAPI.Misc.Grabbing
{
    /// <summary>
    /// Represents a settings menu that provides controls for manipulating grabbed items, including rotation, throwing,
    /// and release actions.
    /// </summary>
    /// <remarks>The GrabMenu exposes UI elements such as sliders, buttons, and key bindings to allow users to
    /// interact with items they have grabbed. It extends the SettingsMenu to provide specialized controls for item
    /// manipulation within the settings interface.</remarks>
    public class GrabMenu : SettingsMenu
    {
        /// <inheritdoc/>
        public override string CustomId { get; } = "secretlabapi.grabmenu";

        /// <inheritdoc/>
        public override string Header { get; } = "<color=yellow> ✋ | Nastavení Grabu</color>";

        /// <summary>
        /// Gets the slider control used to adjust the distance setting.
        /// </summary>
        public SettingsSlider DistanceSlider { get; private set; }

        /// <summary>
        /// Gets the slider control used to adjust the rotation angle.
        /// </summary>
        public SettingsSlider RotateSlider { get; private set; }

        /// <summary>
        /// Gets the key binding used to trigger the grab action.
        /// </summary>
        public SettingsKeyBind GrabKeybind { get; private set; }

        /// <summary>
        /// Gets the key binding used to trigger the release-in-place action.
        /// </summary>
        public SettingsKeyBind ReleaseInPlaceKeybind { get; private set; }

        /// <summary>
        /// Gets the key binding used to trigger the release-to-position action.
        /// </summary>
        public SettingsKeyBind ReleaseToPositionKeybind { get; private set; }

        /// <inheritdoc/>
        public override void BuildMenu(List<SettingsEntry> settings)
        {
            settings.WithEntries(new SettingsEntry[]
            {
                GrabKeybind = new("secretlabapi.grabmenu.grabkeybind", "🔒 | Sebrání předmětu", KeyCode.L, true, false, "Tlačítko pro sebrání předmětu."),
                DistanceSlider = new("secretlabapi.grabmenu.distanceslider", "📏 | Vzdálenost", 1f, 30f, 5f, true, "0.##", "{0}m", "Určuje jak daleko je držený předmět."),
                
                RotateSlider = new("secretlabapi.grabmenu.rotateslider", "📐 | Úhel", -360f, 360f, 0f, true, "0.##", "{0}°", 
                    "Tento slider otočí aktuálně držený předmět o určitý úhel.") { ShouldSyncDrag = false },
                
                ReleaseInPlaceKeybind = new("secretlabapi.grabmenu.releaseinplacekeybind", "<color=red>❌ | Zrušení grabu</color> <i>(zůstane na aktuální pozici)</i>", 
                    KeyCode.X, true, false, "Klávesa pro zrušení grabu (předmět zůstane na aktuální pozici)."),
                
                ReleaseToPositionKeybind = new("secretlabapi.grabmenu.releasetopositionkeybind", "<color=red>❌ | Zrušení grabu</color> <i>(vrátí se na původní pozici)</i>",
                    KeyCode.B, true, false, "Klávesa pro zrušení grabu (předmět se vrátí na původní pozici).")
            });
        }

        public override void OnKeyBindPressed(SettingsKeyBind keyBind)
        {
            base.OnKeyBindPressed(keyBind);

            if (Player?.ReferenceHub == null)
            {
                ApiLog.Warn("GrabMenu", "Player or Player.ReferenceHub is null in OnKeyBindPressed.");
                return;
            }

            if (keyBind?.Base != null
                && GrabKeybind?.Base != null
                && keyBind == GrabKeybind)
            {
                if (Player.TryGetComponent<GrabHandler>(out var grabHandler))
                {
                    grabHandler.Player = Player;
                    grabHandler.Menu = this;

                    grabHandler.OnGrabKeybind();
                }
                else
                {
                    grabHandler = Player.GameObject.AddComponent<GrabHandler>();

                    if (grabHandler != null)
                    {
                        grabHandler.Player = Player;
                        grabHandler.Menu = this;

                        grabHandler.Init();
                        grabHandler.OnGrabKeybind();
                    }
                    else
                    {
                        ApiLog.Warn("GrabHandler", $"Could not add GrabHandler to {Player.ToLogString()}");
                    }
                }
            }
        }
    }
}