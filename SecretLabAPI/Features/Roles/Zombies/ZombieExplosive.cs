using System.ComponentModel;

using LabApi.Events.Arguments.PlayerEvents;

using LabExtended.API;
using LabExtended.Core;

using Utils;

namespace SecretLabAPI.Features.Roles.Zombies;

/// <summary>
/// The explosive zombie custom role.
/// </summary>
public class ZombieExplosive : ZombieRole
{
    /// <summary>
    /// Gets the ID of the role.
    /// </summary>
    public override string Id { get; } = "zombie_explosive";
    
    /// <summary>
    /// Gets or sets the name of the role.
    /// </summary>
    public override string Name { get; set; } = "Explosive Zombie";

    /// <summary>
    /// Gets or sets the spawn chance of the role.
    /// </summary>
    public override float SpawnChance { get; set; } = 40f;

    /// <summary>
    /// Gets or sets the damage dealt by the role.
    /// </summary>
    public override float Damage { get; set; } = 0f;

    /// <summary>
    /// Gets or sets the speed of the role.
    /// </summary>
    public override float Speed { get; set; } = 1f;

    /// <summary>
    /// Gets or sets the size of the role.
    /// </summary>
    public override float Size { get; set; } = 0.9f;

    /// <summary>
    /// Gets or sets the amount of grenades the zombie will drop.
    /// </summary>
    [Description("The amount of grenades the zombie will drop.")]
    public int GrenadeAmount { get; set; } = 2;

    /// <summary>
    /// The spawn message of the role.
    /// </summary>
    public override string Message { get; } =
        $"Jsi <color=red>Explosive Zombie</color>!\n" +
        $"Po smrti spawneš se na tvé pozici spawnou granáty které ihned explodují.";

    /// <summary>
    /// Called when the zombie dies. Executes specific behavior for the Explosive Zombie role,
    /// such as triggering explosions at the zombie's death location based on the configured number of grenades.
    /// </summary>
    /// <param name="args">The event arguments providing details about the player's death, including position and cause.</param>
    /// <param name="data">Additional data that can be passed or modified during the death process.</param>
    public override void OnDied(PlayerDeathEventArgs args, ref object? data)
    {
        base.OnDied(args, ref data);
        
        ApiLog.Debug($"OnDied");

        if (args.Player is not ExPlayer player)
        {
            ApiLog.Debug($"Player is not an ExPlayer");
            return;       
        }
        
        ApiLog.Debug($"Spawning {GrenadeAmount} grenades at {args.OldPosition}");
        
        for (var x = 0; x < GrenadeAmount; x++)
            ExplosionUtils.ServerExplode(args.OldPosition, player.Footprint, ExplosionType.Grenade);
    }
}