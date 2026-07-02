namespace SecretLabAPI.Features.Roles.Zombies;

/// <summary>
/// The zombie sprinter custom role.
/// </summary>
public class ZombieSprinter : ZombieRole
{
    /// <summary>
    /// Gets the ID of the role.
    /// </summary>
    public override string Id { get; } = "zombie_sprinter";
    
    /// <summary>
    /// Gets or sets the name of the role.
    /// </summary>
    public override string Name { get; set; } = "Zombie Sprinter";

    /// <summary>
    /// Gets or sets the spawn chance of the role.
    /// </summary>
    public override float SpawnChance { get; set; } = 25f;

    /// <summary>
    /// Gets or sets the damage dealt by the role.
    /// </summary>
    public override float Damage { get; set; } = 25f;

    /// <summary>
    /// Gets or sets the speed of the role.
    /// </summary>
    public override float Speed { get; set; } = 1.2f;

    /// <summary>
    /// Gets or sets the size of the role.
    /// </summary>
    public override float Size { get; set; } = 0.9f;
    
    /// <summary>
    /// Gets or sets the max health of the role.
    /// </summary>
    public override float MaxHealth { get; set; } = 250f;

    /// <summary>
    /// Gets or sets the spawn health of the role.
    /// </summary>
    public override float SpawnHealth { get; set; } = 250f;
    
    /// <summary>
    /// The spawn message of the role.
    /// </summary>
    public override string Message { get; } =
        $"";
}