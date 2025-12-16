using System.ComponentModel;

using LabExtended.API;

using SecretLabAPI.Elements.Alerts;
using SecretLabAPI.Utilities.Configs;

namespace SecretLabAPI.RandomEvents.Features.ItsRainingCoins
{
    /// <summary>
    /// An event where it literally just rains coins.
    /// </summary>
    public class ItsRainingCoinsEvent : RandomEventBase
    {
        private Dictionary<ExPlayer, CoinRain> rains = new();

        /// <summary>
        /// Gets or sets the delay, in milliseconds, between each tick during the raining coins event.
        /// </summary>
        [Description("Sets the delay between each tick.")]
        public Int32Range RainDelay { get; set; } = new()
        {
            MinValue = 3000,
            MaxValue = 10000
        };

        /// <summary>
        /// Gets or sets the number of coins that will drop per tick during the event.
        /// </summary>
        [Description("Sets the amount of coins that will drop per tick.")]
        public Int32Range RainCount { get; set; } = new()
        {
            MinValue = 1,
            MaxValue = 2
        };
        
        /// <summary>
        /// Gets or sets the current rain delay.
        /// </summary>
        public int CurrentDelay { get; private set; }
        
        /// <summary>
        /// Gets or sets the current amount of coins dropped per tick.
        /// </summary>
        public int CurrentCount { get; private set; }
        
        /// <inheritdoc />
        public override string Id { get; } = "ItsRainingCoins";

        /// <inheritdoc />
        public override void OnEnabled()
        {
            base.OnEnabled();

            CurrentDelay = RainDelay.GetRandom();
            CurrentCount = RainCount.GetRandom();
            
            foreach (var player in ExPlayer.Players)
            {
                var rain = new CoinRain();

                rain.Event = this;
                rain.Player = player;
                
                rain.Start();

                rains[player] = rain;
                
                ShowHint(player, false);
            }
        }

        /// <inheritdoc />
        public override void OnDisabled()
        {
            base.OnDisabled();

            CurrentDelay = int.MaxValue;
            CurrentCount = 0;

            foreach (var pair in rains)
                pair.Value.Stop();
            
            rains.Clear();
        }

        /// <inheritdoc />
        public override void OnPlayerLeft(ExPlayer player)
        {
            base.OnPlayerLeft(player);
            
            if (rains.TryGetValue(player, out var rain))
                rain.Stop();

            rains.Remove(player);
        }

        /// <inheritdoc />
        public override void OnPlayerJoined(ExPlayer player)
        {
            base.OnPlayerJoined(player);
            
            var rain = new CoinRain();

            rain.Event = this;
            rain.Player = player;
                
            rain.Start();

            rains[player] = rain;
            
            ShowHint(player, true);
        }

        private void ShowHint(ExPlayer player, bool isJoined)
        {
            if (isJoined)
            {
                player.SendAlert(AlertType.Info, 15f,
                    $"<b>Na serveru aktuálně probíhá event <color=red>Život na žida</color></b>\n" +
                    $"<b>Každých <color=yellow>{CurrentDelay / 1000} sekund</color> se spawne nad každým živím hráčem <color=yellow>{CurrentCount}</color> coinů.</b>",
                    true);
            }
            else
            {
                player.SendAlert(AlertType.Info, 15f,
                    $"<b>Začal event <color=red>Život na žida</color></b>\n" +
                    $"<b>Každých <color=yellow>{CurrentDelay / 1000} sekund</color> se spawne nad každým živím hráčem <color=yellow>{CurrentCount}</color> coinů!</b>",
                    true);
            }
        }
    }
}