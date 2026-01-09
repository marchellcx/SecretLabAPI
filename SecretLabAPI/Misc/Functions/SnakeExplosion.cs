using LabExtended.Events;
using LabExtended.Events.Player.Snake;

using SecretLabAPI.Extensions;

namespace SecretLabAPI.Misc.Functions
{
    /// <summary>
    /// Makes the player playing the Snake minigame explode upon death.
    /// </summary>
    public static class SnakeExplosion
    {
        private static void Internal_SnakeGameOver(PlayerSnakeGameOverEventArgs args)
        {
            args.Player.Explode(1, ItemType.GrenadeHE, "Game Over", true, true, 10f);
        }

        internal static void Internal_Init()
        {
            ExPlayerEvents.SnakeGameOver += Internal_SnakeGameOver;
        }
    }
}