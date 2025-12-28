using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;

using LabExtended.API;
using LabExtended.Utilities;

using SecretLabAPI.Elements.Alerts;
using SecretLabAPI.RandomEvents;

namespace SecretLabAPI.Gamemodes
{
    /// <summary>
    /// Represents a random event that simulates a blackout by disabling lights and providing players with light sources
    /// if needed.
    /// </summary>
    /// <remarks>During the BlackoutEvent, all map lights are flickered off for the duration of the event.
    /// Players who do not already possess a flashlight or lantern are given one at random. Informational alerts are
    /// sent to all players when the event starts and to new players who join while the event is active. The event is
    /// reversed when disabled, restoring normal lighting conditions.</remarks>
    public class BlackoutEvent : RandomEventBase
    {
        /// <inheritdoc/>
        public override string Id { get; } = "Blackout";

        /// <inheritdoc/>
        public override bool CanActivateMidRound { get; set; } = true;

        /// <inheritdoc/>
        public override void OnEnabled()
        {
            base.OnEnabled();

            PlayerEvents.ChangedRole += OnChangedRole;

            ExMap.FlickerLights(float.MaxValue);

            foreach (var player in ExPlayer.Players)
            {
                if (!player.Inventory.HasItem(ItemType.Flashlight)
                    && !player.Inventory.HasItem(ItemType.Lantern))
                {
                    if (WeightUtils.GetBool(30))
                    {
                        player.Inventory.AddOrSpawnItem(ItemType.Lantern);
                    }
                    else
                    {
                        player.Inventory.AddOrSpawnItem(ItemType.Flashlight);
                    }
                }

                player.SendAlert(AlertType.Info, 10f, "Blackout Event", "<b>Vypadá to že někdo nezaplatil účet za elektřinu ..</b>", true);
            }
        }

        /// <inheritdoc/>
        public override void OnDisabled()
        {
            base.OnDisabled();

            PlayerEvents.ChangedRole -= OnChangedRole;

            ExMap.FlickerLights(1f);
        }

        /// <inheritdoc/>
        public override void OnPlayerJoined(ExPlayer player)
        {
            base.OnPlayerJoined(player);

            player.SendAlert(AlertType.Info, 10f, "Blackout Event", "<b>Na serveru právě probíhá event <color=red>Blackout</color>!</b>", true);
        }

        private void OnChangedRole(PlayerChangedRoleEventArgs args)
        {
            if (args.Player is not ExPlayer player)
                return;

            TimingUtils.AfterSeconds(() =>
            {
                if (!player.Inventory.HasItem(ItemType.Flashlight)
                    && !player.Inventory.HasItem(ItemType.Lantern))
                {
                    if (WeightUtils.GetBool(30))
                    {
                        player.Inventory.AddOrSpawnItem(ItemType.Lantern);
                    }
                    else
                    {
                        player.Inventory.AddOrSpawnItem(ItemType.Flashlight);
                    }
                }
            }, 1);
        }
    }
}
