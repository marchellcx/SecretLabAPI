using AdminToys;

using Interactables;

using InventorySystem.Items.Pickups;

using LabExtended.API;
using LabExtended.API.Settings;
using LabExtended.API.Settings.Entries;

using LabExtended.Core;
using LabExtended.Extensions;
using LabExtended.Utilities;

using SecretLabAPI.Elements.Alerts;
using SecretLabAPI.Rays;

using UnityEngine;

namespace SecretLabAPI.Misc.Grabbing
{
    /// <summary>
    /// Provides functionality for grabbing and releasing supported objects, such as players, doors, toys, and item
    /// pickups, enabling interactive manipulation within the game environment.
    /// </summary>
    /// <remarks>A GrabHandler instance is associated with a specific player and menu. It supports grabbing
    /// only certain object types: ExPlayer, DoorVariant, AdminToyBase, and ItemPickupBase. Attempting to interact with
    /// unsupported types will result in an exception. The handler manages the state of the currently grabbed object and
    /// ensures proper release and synchronization with the associated menu. This class is intended to be used as a
    /// component on a GameObject representing a player.</remarks>
    public class GrabHandler : MonoBehaviour
    {
        /// <summary>
        /// Specifies the combined layer mask used for grab interactions.
        /// </summary>
        public static LayerMask GrabLayerMask = InteractionCoordinator.RaycastMask.Mask | PhysicsUtils.VisibleMask;

        /// <summary>
        /// Releases any object currently held by the specified player, if applicable.
        /// </summary>
        /// <param name="player">The player whose held object should be released. Cannot be null and must have a valid ReferenceHub.</param>
        /// <exception cref="ArgumentNullException">Thrown if the specified player is null or does not have a valid ReferenceHub.</exception>
        public static void ReleaseObject(ExPlayer player, bool releaseToOriginal)
        {
            if (player?.ReferenceHub == null)
                throw new ArgumentNullException(nameof(player));

            if (!player.TryGetComponent<GrabHandler>(out var handler))
                return;

            handler.Release(releaseToOriginal);
        }

        /// <summary>
        /// Attempts to make the specified player grab the given object.
        /// </summary>
        /// <param name="player">The player who will attempt to grab the object. Cannot be null and must have a valid ReferenceHub.</param>
        /// <param name="obj">The object to be grabbed by the player. Cannot be null.</param>
        /// <returns>true if the object was successfully grabbed; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if player is null, player.ReferenceHub is null, or obj is null.</exception>
        public static bool GrabObject(ExPlayer player, object obj)
        {
            if (player?.ReferenceHub == null)
                throw new ArgumentNullException(nameof(player));

            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            if (!player.TryGetComponent<GrabHandler>(out var handler))
                handler = player.GameObject!.AddComponent<GrabHandler>();

            return handler.Grab(obj);
        }

        private ExPlayer? targetPlayer;
        private AdminToyBase? targetToy;
        private ItemPickupBase? targetPickup;

        private float? appliedAngle;
        private bool hasStarted;

        private Vector3? originalPosition;
        private Quaternion? originalRotation;

        /// <summary>
        /// The settings menu associated with this GrabHandler.
        /// </summary>
        public GrabMenu Menu { get; internal set; }

        /// <summary>
        /// The player who is using this GrabHandler.
        /// </summary>
        public ExPlayer Player { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether any object is currently being grabbed.
        /// </summary>
        public bool AnyGrabbed => targetPlayer?.ReferenceHub != null
            || targetToy != null
            || targetPickup != null;

        /// <summary>
        /// Attempts to grab the specified object, enabling interaction with supported types.
        /// </summary>
        /// <remarks>Supported object types include ExPlayer, DoorVariant, AdminToyBase, and
        /// ItemPickupBase. Attempting to grab an object of any other type will result in an exception.</remarks>
        /// <param name="obj">The object to grab. Must be an instance of a supported type such as ExPlayer, DoorVariant, AdminToyBase, or
        /// ItemPickupBase.</param>
        /// <returns>true if the object was successfully grabbed; otherwise, false if the object is already grabbed.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="obj"/> is null.</exception>
        /// <exception cref="Exception">Thrown if <paramref name="obj"/> is not a supported type.</exception>
        public bool Grab(object obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            if (!AnyGrabbed)
            {
                if (obj is ExPlayer player)
                {
                    if (player?.ReferenceHub == null)
                        return false;

                    if (player == Player)
                        return false;

                    GrabPlayer(player);
                    return true;
                }

                if (obj is AdminToyBase toy)
                {
                    GrabToy(toy);
                    return true;
                }

                if (obj is ItemPickupBase pickup)
                {
                    GrabPickup(pickup);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Releases the currently grabbed object/player.
        /// </summary>
        public void Release(bool releaseToOriginal)
        {
            if (AnyGrabbed)
            {
                if (targetPlayer?.ReferenceHub != null)
                    ReleasePlayer(releaseToOriginal);
                else if (targetPickup != null)
                    ReleasePickup(releaseToOriginal);
                else if (targetToy != null)
                    ReleaseToy(releaseToOriginal);

                targetPlayer = null;
                targetPickup = null;
                targetToy = null;

                originalRotation = null;
                originalPosition = null;

                appliedAngle = null;
            }
        }

        internal void Init()
        {
            if (!hasStarted)
            {
                if (Player?.ReferenceHub == null)
                {
                    if (!ExPlayer.TryGet(gameObject, out var owner) || owner?.ReferenceHub == null)
                    {
                        ApiLog.Warn("GrabHandler", "Could not get owner from gameObject");

                        Destroy(this);
                        return;
                    }

                    Player = owner;
                }

                if (Menu == null)
                    Menu = Player.GetMenu<GrabMenu>()!;

                SyncMenu();

                appliedAngle = null;

                originalPosition = null;
                originalRotation = null;

                Player.SendConsoleMessage($"[GRAB] Component started", "green");

                hasStarted = true;
            }
        }

        void Start()
            => Init();

        void OnDestroy()
        {
            Release(true);

            Menu = null!;
            Player = null!;

            originalPosition = null;
            originalRotation = null;

            hasStarted = false;

            ApiLog.Debug("GrabHandler", "Destroyed");
        }

        private void Update()
        {
            if (Player?.ReferenceHub == null)
            {
                Destroy(this);
                return;
            }

            if (!Player.Role.IsAlive)
            {
                if (AnyGrabbed)
                    Release(true);

                return;
            }

            if (targetPlayer?.ReferenceHub != null)
            {
                if (!targetPlayer.Role.IsAlive)
                {
                    Release(false);
                    return;
                }

                UpdatePlayer();
            }
            else if (targetPickup != null)
            {
                UpdatePickup();
            }
            else if (targetToy != null)
            {
                UpdateToy();
            }
        }

        private void SyncMenu()
        {
            if (Menu == null)
                return;

            Menu.RotateSlider.OnMoved = OnAngleSlider;

            Menu.ReleaseInPlaceKeybind.OnPressed = ReleaseInPlaceKeybind;
            Menu.ReleaseToPositionKeybind.OnPressed = ReleaseToOriginalKeybind;
        }

        #region Grab
        private void GrabToy(AdminToyBase adminToyBase)
        {
            originalPosition = adminToyBase.Position;
            originalRotation = adminToyBase.Rotation;

            adminToyBase.NetworkIsStatic = false;

            targetToy = adminToyBase;

            Player.SendAlert(AlertType.Info, 5f, "Grab System", $"<b>Grabbed: <color=red>Admin Toy</color> {adminToyBase.CommandName}</b>");
        }

        private void GrabPlayer(ExPlayer player)
        {
            originalPosition = player.Position;
            originalRotation = player.Rotation;

            targetPlayer = player;

            player.IsGodModeEnabled = true;

            Player.SendAlert(AlertType.Info, 5f, "Grab System", $"<b>Grabbed: <color=red>Player</color> {player.Nickname}</b>");
        }

        private void GrabPickup(ItemPickupBase itemPickupBase)
        {
            originalPosition = itemPickupBase.Position;
            originalRotation = itemPickupBase.Rotation;

            itemPickupBase.LockPickup();
            itemPickupBase.FreezePickup();

            targetPickup = itemPickupBase;

            Player.SendAlert(AlertType.Info, 5f, "Grab System", $"<b>Grabbed: <color=red>Item</color> {itemPickupBase.Info.ItemId}</b>");
        }
        #endregion

        #region Update
        private void UpdateToy()
        {
            targetToy.NetworkPosition = GetGrabPosition();
            targetToy.NetworkRotation = GetGrabRotation();
        }

        private void UpdatePlayer()
        {
            targetPlayer.Position.Position = GetGrabPosition();
            targetPlayer.Rotation.Rotation = GetGrabRotation();
        }

        private void UpdatePickup()
        {
            targetPickup.Position = GetGrabPosition();
            targetPickup.Rotation = GetGrabRotation();
        }
        #endregion

        #region Release
        private void ReleaseToy(bool releaseToOriginal)
        {
            if (releaseToOriginal && originalPosition != null && originalRotation != null)
            {
                targetToy.NetworkPosition = originalPosition.Value;
                targetToy.NetworkRotation = originalRotation.Value;
            }
        }

        private void ReleasePlayer(bool releaseToOriginal)
        {
            if (targetPlayer?.ReferenceHub != null)
            {
                if (releaseToOriginal && originalPosition != null && originalRotation != null && targetPlayer.Role.IsAlive)
                {
                    targetPlayer.Position.Position = originalPosition.Value;
                    targetPlayer.Rotation.Rotation = originalRotation.Value;
                }

                TimingUtils.AfterFrames((() => targetPlayer.IsGodModeEnabled = false), 1);
            }
        }

        private void ReleasePickup(bool releaseToOriginal)
        {
            if (releaseToOriginal && originalPosition != null && originalRotation != null)
            {
                targetPickup.Position = originalPosition.Value;
                targetPickup.Rotation = originalRotation.Value;
            }

            targetPickup.UnlockPickup();
            targetPickup.UnfreezePickup();
        }
        #endregion

        private Vector3 GetGrabPosition()
        {
            var cameraPosition = Player.Rotation.CameraPosition;
            var cameraForward = Player.Rotation.CameraForward;

            if (Menu.DistanceSlider.Value != 0f)
                cameraForward *= Menu.DistanceSlider.Value;

            return cameraPosition + cameraForward;
        }

        private Quaternion GetGrabRotation()
        {
            var cameraForward = Player.Rotation.CameraForward;
            var cameraUp = Player.CameraTransform.up;
            
            var lookAtPlayer = Quaternion.LookRotation(cameraForward, cameraUp);
            
            if (appliedAngle != null)
                lookAtPlayer *= Quaternion.Euler(appliedAngle.Value, 0f, 0f);
            
            return lookAtPlayer;
        }

        internal void OnGrabKeybind()
        {
            try
            {
                if (Player?.ReferenceHub == null)
                {
                    ApiLog.Warn("GrabHandler", "Player is null");

                    Destroy(this);
                    return;
                }

                if (!Player.Role.IsAlive)
                {
                    Player.SendConsoleMessage($"[GRAB] Not alive!", "red");
                    return;
                }

                if (!Player.TryCast(50f, 0.2f, GrabLayerMask, out var hit))
                {
                    Player.SendConsoleMessage($"[GRAB] Invalid raycast (no hit)", "red");
                    return;
                }

                if (hit.collider == null)
                {
                    Player.SendConsoleMessage($"[GRAB] Invalid raycast (null collider)", "red");
                    return;
                }

                Player.SendConsoleMessage($"[GRAB] Ray: ColliderName={hit.collider.name}; GoName={hit.collider.gameObject.name}; " +
                    $"Distance={hit.distance}; Point={hit.point.ToPreciseString()}", "red");

                if (hit.collider.gameObject.TryFindComponent<AdminToyBase>(out var adminToy))
                {
                    Grab(adminToy);

                    Player.SendConsoleMessage($"[GRAB] Found AdminToyBase: {adminToy.CommandName}", "red");
                }
                else if (hit.collider.gameObject.TryFindComponent<ItemPickupBase>(out var itemPickupBase))
                {
                    Grab(itemPickupBase);

                    Player.SendConsoleMessage($"[GRAB] Found ItemPickupBase: {itemPickupBase.Info.ItemId}", "red");
                }
                else if (hit.collider.gameObject.TryFindComponent<ReferenceHub>(out var referenceHub))
                {
                    if (ExPlayer.TryGet(referenceHub, out var targetPlayer))
                    {
                        Grab(targetPlayer);

                        Player.SendConsoleMessage($"[GRAB] Found player: {targetPlayer.Nickname}", "red");
                    }
                    else
                    {
                        Player.SendConsoleMessage($"[GRAB] Invalid raycast (ReferenceHub but not ExPlayer)", "red");
                    }
                }
                else if (hit.collider.gameObject.TryFindComponent<IDestructible>(out var destructible))
                {
                    if (ExPlayer.TryGet(destructible.NetworkId, out var targetPlayer) && targetPlayer != Player)
                    {
                        Grab(targetPlayer);
                    }
                    else
                    {
                        Player.SendConsoleMessage($"[GRAB] Invalid raycast (IDestructible but not ExPlayer)", "red");
                    }
                }
                else
                {
                    Player.SendConsoleMessage($"[GRAB] Invalid raycast (no component)", "red");
                }
            }
            catch (Exception ex)
            {
                ApiLog.Error("GrabHandler", ex);
            }
        }

        private void OnAngleSlider(SettingsSlider slider)
        {
            appliedAngle = slider.Value;

            if (Player?.ReferenceHub != null)
                Player.SendConsoleMessage($"[GRAB] Angle is now {appliedAngle?.ToString() ?? "(null)"}", "red");
        }

        private void ReleaseInPlaceKeybind(SettingsKeyBind bind)
        {
            if (!bind.IsPressed)
                return;

            if (Player?.ReferenceHub != null)
                Player.SendConsoleMessage($"[GRAB] Releasing in place", "red");

            Release(false);
        }

        private void ReleaseToOriginalKeybind(SettingsKeyBind bind)
        {
            if (!bind.IsPressed)
                return;

            if (Player?.ReferenceHub != null)
                Player.SendConsoleMessage($"[GRAB] Releasing to original", "red");

            Release(true);
        }

        internal static void Initialize()
        {
            SettingsManager.AddBuilder(new SettingsBuilder("GrabMenuBuilder")
                .WithMenu(() => new GrabMenu())
                .WithPredicate(p => p.RemoteAdminAccess));
        }
    }
}