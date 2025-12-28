using System.Diagnostics;

using LabExtended.API;

using SecretLabAPI.Extensions;

using UnityEngine;

namespace SecretLabAPI.Utilities
{
    /// <summary>
    /// Provides functionality to spawn items repeatedly for a specified player, simulating an "item rain" effect in the
    /// game environment.
    /// </summary>
    /// <remarks>Attach this component to a player to enable periodic spawning of items at their location. Use
    /// the static methods to start or stop the effect for a given player. The effect can be customized by setting the
    /// item type, spawn count, and delay between drops. If the player does not have a valid reference hub, the
    /// component will be automatically removed.</remarks>
    public class ItemRain : MonoBehaviour
    {
        private Stopwatch watch = new();

        /// <summary>
        /// The player targeted by this instance.
        /// </summary>
        public ExPlayer Player { get; private set; }

        /// <summary>
        /// Gets or sets the amount of items to drop.
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Gets or sets the delay between each drop in milliseconds.
        /// </summary>
        public int Delay { get; set; }

        /// <summary>
        /// Gets or sets the item type to drop.
        /// </summary>
        public ItemType Item { get; set; }

        /// <summary>
        /// Gets or sets the scale of the dropped item.
        /// </summary>
        public Vector3 Scale { get; set; } = Vector3.one;

        /// <summary>
        /// Whether or not the rain is in progress.
        /// </summary>
        public bool IsRunning => watch.IsRunning;

        /// <summary>
        /// Enables the item rain.
        /// </summary>
        public void Enable()
        {
            watch.Restart();
        }

        /// <summary>
        /// Disables the item rain.
        /// </summary>
        public void Disable()
        {
            watch.Stop();
            watch.Reset();
        }

        void Start()
        {
            Player = ExPlayer.Get(gameObject)!;

            if (Player?.ReferenceHub == null)
            {
                Destroy(this);
                return;
            }
        }

        private void Update()
        {
            if (!IsRunning)
                return;

            if (Player?.ReferenceHub == null)
                return;

            if (!Player.Role.IsAlive)
                return;

            if (Count < 1 || Item == ItemType.None)
                return;

            if (Delay > 0 && watch.ElapsedMilliseconds < Delay)
                return;

            for (var x = 0; x < Count; x++)
                ExMap.SpawnItem(ItemType.Coin, Player.PositionAdjustY(0.5f), Vector3.one, Player.Rotation);
        }

        void OnDestroy()
        {
            Disable();
        }

        /// <summary>
        /// Stops the active item rain effect for the specified player if it matches the given item type.
        /// </summary>
        /// <remarks>If the player does not have an active item rain effect for the specified item type,
        /// no action is taken and the method returns <see langword="false"/>.</remarks>
        /// <param name="player">The player whose item rain effect should be checked and potentially stopped. Must not be null and must have
        /// a valid ReferenceHub.</param>
        /// <param name="type">The item type to match against the player's current item rain effect.</param>
        /// <param name="destroyRain">Indicates whether the item rain component should be destroyed after disabling it. If <see langword="true"/>,
        /// the component is destroyed; otherwise, it is only disabled. The default is <see langword="true"/>.</param>
        /// <returns>Returns <see langword="true"/> if an item rain effect matching the specified type was found and stopped;
        /// otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="player"/> is null or does not have a valid ReferenceHub.</exception>
        public static bool StopItemRainIf(ExPlayer player, ItemType type, bool destroyRain = true)
        {
            if (player?.ReferenceHub == null)
                throw new ArgumentException("Player is not valid.", nameof(player));

            if (player.TryGetComponent<ItemRain>(out var itemRain) && itemRain.Item == type)
            {
                itemRain.Disable();

                if (destroyRain)
                    Destroy(itemRain);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Stops the item rain effect for the specified player.
        /// </summary>
        /// <param name="player">The player for whom the item rain effect should be stopped. Must not be null and must have a valid reference
        /// hub.</param>
        /// <param name="destroyRain">Indicates whether the item rain component should be destroyed after disabling it. If <see langword="true"/>,
        /// the component is destroyed; otherwise, it is only disabled. The default value is <see langword="true"/>.</param>
        /// <returns>Returns <see langword="true"/> if the item rain effect was active and has been stopped; otherwise, <see
        /// langword="false"/>.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="player"/> is null or does not have a valid reference hub.</exception>
        public static bool StopItemRain(ExPlayer player, bool destroyRain = true)
        {
            if (player?.ReferenceHub == null)
                throw new ArgumentException("Player is not valid.", nameof(player));

            if (player.TryGetComponent<ItemRain>(out var itemRain))
            {
                itemRain.Disable();

                if (destroyRain)
                    Destroy(itemRain);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Starts an item rain effect for the specified player, causing items of the given type to spawn repeatedly at
        /// set intervals.
        /// </summary>
        /// <remarks>If the player already has an active ItemRain component, its parameters will be
        /// updated and the effect restarted. Otherwise, a new ItemRain component will be added to the player.</remarks>
        /// <param name="player">The player for whom the item rain effect will be started. Must be a valid player with an initialized
        /// reference hub.</param>
        /// <param name="type">The type of item to spawn during the item rain effect.</param>
        /// <param name="amount">The total number of items to spawn over the duration of the effect. Must be a non-negative integer.</param>
        /// <param name="delay">The delay, in milliseconds, between each item spawn. Must be a non-negative integer.</param>
        /// <returns>An ItemRain instance representing the active item rain effect for the specified player.</returns>
        /// <exception cref="ArgumentException">Thrown if the specified player is null or does not have a valid reference hub.</exception>
        public static ItemRain StartItemRain(ExPlayer player, ItemType type, int amount = 1, int delay = 0, Vector3? scale = null)
        {
            if (player?.ReferenceHub == null)
                throw new ArgumentException("Player is not valid.", nameof(player));

            if (player.TryGetComponent<ItemRain>(out var itemRain))
            {
                itemRain.Item = type;
                itemRain.Count = amount;
                itemRain.Delay = delay;
                itemRain.Scale = scale ?? Vector3.one;

                itemRain.Enable();
                return itemRain;
            }
            else
            {
                itemRain = player.GameObject!.AddComponent<ItemRain>();

                itemRain.Item = type;
                itemRain.Count = amount;
                itemRain.Delay = delay;
                itemRain.Scale = scale ?? Vector3.one;

                itemRain.Enable();
                return itemRain;
            }
        }
    }
}