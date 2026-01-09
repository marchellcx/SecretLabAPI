using System.ComponentModel;

using LabExtended.API;

using SecretLabAPI.Elements.Alerts;
using SecretLabAPI.RandomEvents;

using SecretLabAPI.Utilities;
using SecretLabAPI.Utilities.Configs;

using YamlDotNet.Serialization;

namespace SecretLabAPI.Gamemodes
{
    /// <summary>
    /// An event where it literally just rains coins.
    /// </summary>
    public class ItsRainingCoinsEvent : RandomEventBase
    {
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
        [YamlIgnore]
        public int CurrentDelay { get; private set; }

        /// <summary>
        /// Gets or sets the current amount of coins dropped per tick.
        /// </summary>
        [YamlIgnore]
        public int CurrentCount { get; private set; }
        
        /// <inheritdoc />
        public override string Id { get; } = "ItsRainingCoins";

        /// <inheritdoc />
        public override bool CanActivateMidRound { get; set; } = true;

        /// <inheritdoc />
        public override void OnEnabled()
        {
            base.OnEnabled();

            CurrentDelay = RainDelay.GetRandom();
            CurrentCount = RainCount.GetRandom();
            
            foreach (var player in ExPlayer.Players)
            {
                ItemRain.StartItemRain(player, ItemType.Coin, CurrentCount, CurrentDelay);
                
                ShowHint(player, false);
            }
        }

        /// <inheritdoc />
        public override void OnDisabled()
        {
            base.OnDisabled();

            CurrentDelay = int.MaxValue;
            CurrentCount = 0;
           
            foreach (var player in ExPlayer.Players)
                ItemRain.StopItemRainIf(player, ItemType.Coin);
        }

        /// <inheritdoc />
        public override void OnPlayerJoined(ExPlayer player)
        {
            base.OnPlayerJoined(player);

            ItemRain.StartItemRain(player, ItemType.Coin, CurrentCount, CurrentDelay);

            ShowHint(player, true);
        }

        private void ShowHint(ExPlayer player, bool isJoined)
        {
            if (isJoined)
            {
                player.SendAlert(AlertType.Info, 15f, "Náhodné Eventy",
                    $"<b>Na serveru aktuálně probíhá event <color=red>Život na žida</color></b>\n" +
                    $"<b>Každých <color=yellow>{CurrentDelay / 1000} sekund</color> se spawne nad každým živím hráčem <color=yellow>{CurrentCount}</color> coinů.</b>");
            }
            else
            {
                player.SendAlert(AlertType.Info, 15f, "Náhodné Eventy",
                    $"<b>Začal event <color=red>Život na žida</color></b>\n" +
                    $"<b>Každých <color=yellow>{CurrentDelay / 1000} sekund</color> se spawne nad každým živím hráčem <color=yellow>{CurrentCount}</color> coinů!</b>");
            }
        }
    }
}