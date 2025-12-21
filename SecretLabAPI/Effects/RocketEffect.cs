using LabExtended.API.Custom.Effects;

using UnityEngine;

namespace SecretLabAPI.Effects
{
    /// <summary>
    /// Represents a custom ticking effect that simulates a "rocket" effect on a player.
    /// Gradually elevates the player's vertical position over time with a consistent step value
    /// and applies disintegration upon completion or when the effect is removed.
    /// </summary>
    public class RocketEffect : CustomTickingEffect
    {
        private float delay;
        private float time;
        
        /// <summary>
        /// Gets or sets the increase in Y axis per frame.
        /// </summary>
        public float Step { get; set; } = 15f;

        /// <summary>
        /// Gets or sets the duration of the rocket before being killed (in seconds).
        /// </summary>
        public float Duration { get; set; } = 30f;

        /// <summary>
        /// Gets or sets the per-frame delay.
        /// </summary>
        public new float Delay
        {
            get => delay;
            set => delay = value;
        }

        /// <inheritdoc />
        public override void ApplyEffects()
        {
            base.ApplyEffects();
            time = delay;
        }

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

            if (delay > 0f)
            {
                if (time > 0f)
                {
                    time -= Time.deltaTime;
                    return;
                }

                time = delay;
            }

            if (Player?.ReferenceHub == null || !Player.IsAlive)
            {
                RemoveEffects();
                return;
            }

            if (Duration <= 0f)
            {
                RemoveEffects();
                return;
            }
            
            var pos = Player.Position.Position;

            pos.y += Step;

            Player.Position.Position = pos;
        }
    }
}