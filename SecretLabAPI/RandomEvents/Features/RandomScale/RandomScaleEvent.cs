using System.ComponentModel;

using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;

using LabExtended.API;
using LabExtended.Utilities;

using LabExtended.Core.Configs.Objects;

using PlayerRoles;

using SecretLabAPI.Elements.Alerts;

using UnityEngine;

namespace SecretLabAPI.RandomEvents.Features.RandomScale
{
    public class RandomScaleEvent : RandomEventBase
    {
        /// <inheritdoc />
        public override string Id { get; } = "RandomScale";

        /// <summary>
        /// Gets or sets a dictionary defining the possible scales to apply along with their respective probabilities.
        /// </summary>
        /// <remarks>
        /// The key represents the probability in percentage, and the value defines the corresponding scale as a vector.
        /// </remarks>
        [Description("Sets the list of possible scales to apply.")]
        public Dictionary<float, YamlVector3> Values { get; set; } = new()
        {
            { 50f, new(2f, 1f, 2f) },
            { 25f, new(1f, 2f, 1f) },
            { 20f, new(1f, 1f, 2f) },
            { 5f, new(2f, 1f, 1f) }
        };

        /// <inheritdoc />
        public override void OnEnabled()
        {
            base.OnEnabled();
            
            PlayerEvents.ChangedRole += OnChangedRole;
            
            ExPlayer.Players.ForEach(p =>
            {
                p.SendAlert(AlertType.Info, 10f,
                    $"<b>Začal event <color=red>Náhodná velikost</color>!</b>\n" +
                    $"<b>Po každém respawnu dostanete náhodný scale.</b>", true);

                if (!p.Role.IsAlive)
                    return;

                var scale = SelectScale();

                if (scale == Vector3.one)
                    return;

                p.Scale = scale;
            });
        }

        /// <inheritdoc />
        public override void OnDisabled()
        {
            base.OnDisabled();
            
            PlayerEvents.ChangedRole -= OnChangedRole;
        }

        /// <inheritdoc />
        public override void OnPlayerJoined(ExPlayer player)
        {
            base.OnPlayerJoined(player);
            
            player.SendAlert(AlertType.Info, 10f, 
                $"<b>Na serveru právě probíhá event <color=red>Náhodná velikost</color></b>\n" +
                $"<b>Po každém respawnu dostanete náhodný scale.</b>", true);
        }

        private void OnChangedRole(PlayerChangedRoleEventArgs args)
        {
            if (!args.NewRole.RoleTypeId.IsAlive() || args.NewRole.RoleTypeId == RoleTypeId.Tutorial)
                return;

            if (args.Player is not ExPlayer player)
                return;

            var scale = SelectScale();

            if (scale == Vector3.zero)
                return;

            TimingUtils.AfterSeconds(() => player.Scale = scale, 1f);
        }

        private Vector3 SelectScale()
        {
            var random = Values.GetRandomWeighted(p => p.Key);

            if (random.Value != null)
                return random.Value.Vector;

            return Vector3.zero;
        }
    }
}