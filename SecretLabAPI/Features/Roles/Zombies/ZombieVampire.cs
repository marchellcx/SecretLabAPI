using System.ComponentModel;

using LabApi.Events.Arguments.PlayerEvents;

using LabExtended.Core;
using LabExtended.Core.Configs.Objects;

namespace SecretLabAPI.Features.Roles.Zombies;

/// <summary>
/// The zombie vampire custom role.
/// </summary>
public class ZombieVampire : ZombieRole
{
    /// <summary>
    /// Gets the ID of the role.
    /// </summary>
    public override string Id { get; } = "zombie_vampire";
    
    /// <summary>
    /// Gets or sets the name of the role.
    /// </summary>
    public override string Name { get; set; } = "Zombie Vampire";

    /// <summary>
    /// Gets or sets the spawn chance of the role.
    /// </summary>
    public override float SpawnChance { get; set; } = 20f;

    /// <summary>
    /// Gets or sets the damage dealt by the role.
    /// </summary>
    public override float Damage { get; set; } = 20f;

    /// <summary>
    /// Gets or sets the speed of the role.
    /// </summary>
    public override float Speed { get; set; } = 1.05f;

    /// <summary>
    /// Gets or sets the size of the role.
    /// </summary>
    public override float Size { get; set; } = 0.9f;

    /// <summary>
    /// Gets or sets the max health of the role.
    /// </summary>
    public override float MaxHealth { get; set; } = 450f;

    /// <summary>
    /// Gets or sets the spawn health of the role.
    /// </summary>
    public override float SpawnHealth { get; set; } = 450f;
    
    /// <summary>
    /// The spawn message of the role.
    /// </summary>
    public override string Message =>
        $"Jsi <color=red>Zombie Vampire</color>!\n" +
        $"Za každý hit získáš <color=yellow>{HealthPerHit} HP</color> zpět\n" +
        $"Za zabití hráče získáš <color=yellow>{HealthPerKill} HP</color> zpět";

    /// <summary>
    /// Gets or sets whether or not health should be allowed to overflow.
    /// </summary>
    [Description("Whether or not health should be allowed to overflow.")]
    public bool HealthOverflow { get; set; } = true;

    /// <summary>
    /// The amount of health restored per hit.
    /// </summary>
    [Description("The amount of health restored per hit.")]
    public float HealthPerHit { get; set; } = 10f;

    /// <summary>
    /// The amount of health restored per kill.
    /// </summary>
    [Description("The amount of health restored per kill.")]
    public float HealthPerKill { get; set; } = 50f;

    /// <summary>
    /// Handles the logic when the Zombie Vampire attacks a player.
    /// </summary>
    /// <param name="args">The event arguments containing details of the attack (e.g., attacker, damage).</param>
    /// <param name="data">A reference to additional custom data for the event, which can be modified.</param>
    public override void OnAttacked(PlayerHurtEventArgs args, ref object? data)
    {
        base.OnAttacked(args, ref data);
        
        ApiLog.Debug($"OnAttacked");

        if (HealthPerHit < 1f)
        {
            ApiLog.Debug($"HealthPerHit is less than 1f");
            return;       
        }

        if (!HealthOverflow && (args.Player.Health + HealthPerHit) > args.Player.MaxHealth)
        {
            ApiLog.Debug($"Health overflow protection triggered");
            return;
        }
        
        if (args.Player.Health + HealthPerHit > args.Player.MaxHealth)
        {
            args.Player.MaxHealth = args.Player.Health + HealthPerHit;
            
            ApiLog.Debug($"Max health updated to {args.Player.MaxHealth}");
        }
        
        args.Player.Health += HealthPerHit;
        
        ApiLog.Debug($"Health updated to {args.Player.Health}");
    }

    /// <summary>
    /// Handles the logic when the Zombie Vampire kills a player.
    /// </summary>
    /// <param name="args">The event arguments containing details of the death, including the victim and cause of death.</param>
    /// <param name="data">A reference to additional custom data for the event, which can be modified.</param>
    public override void OnKilled(PlayerDeathEventArgs args, ref object? data)
    {
        base.OnKilled(args, ref data);
        
        ApiLog.Debug($"OnKilled");
        
        if (HealthPerKill < 1f)
        {
            ApiLog.Debug($"HealthPerKill is less than 1f");
            return;
        }

        if (!HealthOverflow && (args.Player.Health + HealthPerKill) > args.Player.MaxHealth)
        {
            ApiLog.Debug($"Health overflow protection triggered");
            return;      
        }
        
        if (args.Player.Health + HealthPerKill > args.Player.MaxHealth)
        {
            args.Player.MaxHealth = args.Player.Health + HealthPerKill;
            
            ApiLog.Debug($"Max health updated to {args.Player.MaxHealth}");
        }
        
        args.Player.Health += HealthPerKill;
        
        ApiLog.Debug($"Health updated to {args.Player.Health}");
    }
}