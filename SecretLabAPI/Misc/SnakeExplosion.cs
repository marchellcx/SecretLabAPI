using LabExtended.Events;
using LabExtended.Events.Player.Snake;

using SecretLabAPI.Utilities;

namespace SecretLabAPI.Misc
{
    /// <summary>
    /// Makes the player playing the Snake minigame explode upon death.
    /// </summary>
    public static class SnakeExplosion
    {
        private static void Internal_SnakeGameOver(PlayerSnakeGameOverEventArgs args)
        {
            ExplosionEffects.Explode(args.Player, 1, ItemType.GrenadeHE, "Game Over", true, true);
        }

        internal static void Internal_Init()
        {
            ExPlayerEvents.SnakeGameOver += Internal_SnakeGameOver;
        }
    }
}