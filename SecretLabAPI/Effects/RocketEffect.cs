using LabExtended.API.Custom.Effects;

using System.ComponentModel;

using UnityEngine;

namespace SecretLabAPI.Effects
{
    /// <summary>
    /// Represents a custom ticking effect that simulates a "rocket" effect on a player.
    /// Gradually elevates the player's vertical position over time with a consistent step value
    /// and applies disintegration upon completion or when the effect is removed.
    /// </summary>
    public class RocketEffect : CustomDurationEffect
    {
        /// <summary>
        /// Gets or sets the increase in Y axis per frame.
        /// </summary>
        [Description("The increase in Y axis per frame.")]
        public float Step { get; set; } = 15f;

        /// <inheritdoc />
        public override void RemoveEffects()
        {
            base.RemoveEffects();

            if (Player?.ReferenceHub == null || !Player.IsAlive)
                return;

            Player.Disintegrate(Vector3.up, true);
        }

        /// <inheritdoc />
        public override void Tick()
        {
            base.Tick();

            if (Player?.ReferenceHub == null || !Player.IsAlive)
            {
                Disable();
                return;
            }
            
            var pos = Player.Position.Position;

            pos.y += Step;

            Player.Position.Position = pos;
        }
    }
}