using LabExtended.Events;
using LabExtended.Events.Player.Snake;

using ObscurisCore.Extensions;

namespace ObscurisCore.Features;

/// <summary>
/// Makes the player playing the Snake minigame explode upon death.
/// </summary>
public static class SnakeExplosion
{
    private static void OnSnakeGameOver(PlayerSnakeGameOverEventArgs args)
    {
        args.Player.Explode(1, ItemType.GrenadeHE, "Game Over", true, true, 10f);
    }

    private static void Initialize()
    {
        ExPlayerEvents.SnakeGameOver += OnSnakeGameOver;
    }
}