namespace SecretLabAPI.Features.Roles.Zombies;

/// <summary>
/// The regular zombie custom role.
/// </summary>
public class ZombieRegular : ZombieRole
{
    /// <summary>
    /// Gets the ID of the role.
    /// </summary>
    public override string Id { get; } = "zombie_regular";
    
    /// <summary>
    /// Gets or sets the name of the role.
    /// </summary>
    public override string Name { get; set; } = "Regular Zombie";

    /// <summary>
    /// Gets or sets the spawn chance of the role.
    /// </summary>
    public override float SpawnChance { get; set; } = 80f;

    /// <summary>
    /// Gets or sets the damage dealt by the role.
    /// </summary>
    public override float Damage { get; set; } = 40f;

    /// <summary>
    /// Gets or sets the speed of the role.
    /// </summary>
    public override float Speed { get; set; } = 1f;

    /// <summary>
    /// Gets or sets the size of the role.
    /// </summary>
    public override float Size { get; set; } = 1f;

    /// <summary>
    /// Gets or sets the max health of the role.
    /// </summary>
    public override float MaxHealth { get; set; } = 500f;

    /// <summary>
    /// Gets or sets the spawn health of the role.
    /// </summary>
    public override float SpawnHealth { get; set; } = 500f;
    
    /// <summary>
    /// The spawn message of the role.
    /// </summary>
    public override string Message { get; } =
        $"";
}