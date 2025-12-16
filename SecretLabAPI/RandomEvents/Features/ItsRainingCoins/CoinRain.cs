using System.Diagnostics;

using LabExtended.API;
using LabExtended.Utilities.Update;

using SecretLabAPI.Extensions;

using UnityEngine;

namespace SecretLabAPI.RandomEvents.Features.ItsRainingCoins
{
    public class CoinRain
    {
        /// <summary>
        /// The player targeted by this instance.
        /// </summary>
        public ExPlayer Player;

        /// <summary>
        /// The delay timer.
        /// </summary>
        public Stopwatch Watch;

        /// <summary>
        /// The source event.
        /// </summary>
        public ItsRainingCoinsEvent Event;

        /// <summary>
        /// Starts the rain.
        /// </summary>
        public void Start()
        {
            Watch = new();
            Watch.Restart();

            PlayerUpdateHelper.OnLateUpdate += Update;
        }

        /// <summary>
        /// Stops the rain.
        /// </summary>
        public void Stop()
        {
            PlayerUpdateHelper.OnLateUpdate -= Update;
            
            if (Watch != null)
            {
                Watch.Stop();
                Watch.Reset();
            }

            Watch = null!;
            Player = null!;
            Event = null!;
        }

        private void Update()
        {
            if (Watch is null || !Watch.IsRunning)
                return;

            if (Event is null || Player?.ReferenceHub == null)
                return;

            if (!Player.Role.IsAlive)
                return;

            if (Event.CurrentCount < 1)
                return;

            if (Watch.ElapsedMilliseconds < Event.CurrentDelay)
                return;
            
            Watch.Restart();

            for (var x = 0; x < Event.CurrentCount; x++)
                ExMap.SpawnItem(ItemType.Coin, Player.PositionAdjustY(0.5f), Vector3.one, Player.Rotation);
        }
    }
}