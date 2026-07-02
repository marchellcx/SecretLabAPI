namespace SecretLabAPI.Features.Roles.Zombies;

/// <summary>
/// The zombie juggernaut custom role.
/// </summary>
public class ZombieJuggernaut : ZombieRole
{
    /// <summary>
    /// Gets the ID of the role.
    /// </summary>
    public override string Id { get; } = "zombie_juggernaut";
    
    /// <summary>
    /// Gets or sets the name of the role.
    /// </summary>
    public override string Name { get; set; } = "Zombie Juggernaut";

    /// <summary>
    /// Gets or sets the spawn chance of the role.
    /// </summary>
    public override float SpawnChance { get; set; } = 5f;

    /// <summary>
    /// Gets or sets the damage dealt by the role.
    /// </summary>
    public override float Damage { get; set; } = 65f;

    /// <summary>
    /// Gets or sets the speed of the role.
    /// </summary>
    public override float Speed { get; set; } = 0.7f;

    /// <summary>
    /// Gets or sets the size of the role.
    /// </summary>
    public override float Size { get; set; } = 1.15f;
    
    /// <summary>
    /// Gets or sets the max health of the role.
    /// </summary>
    public override float MaxHealth { get; set; } = 750f;

    /// <summary>
    /// Gets or sets the spawn health of the role.
    /// </summary>
    public override float SpawnHealth { get; set; } = 750f;

    /// <summary>
    /// The spawn message of the role.
    /// </summary>
    public override string Message { get; } =
        $"";
}