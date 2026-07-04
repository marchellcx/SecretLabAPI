using PlayerRoles;

using SecretLabAPI.Utilities;

namespace SecretLabAPI.Features.Roles.Classes.ClassD;

/// <summary>
/// Represents a custom spawnable role named "Addicted" that inherits from <see cref="SpawnableCustomRole"/>.
/// This role is assigned to Class-D personnel and includes unique properties and spawn conditions.
/// </summary>
public class ClassDAddictedRole : SpawnableCustomRole
{
    /// <inheritdoc/>
    public override string Id { get; } = "classd_addicted";

    /// <inheritdoc/>
    public override string Name { get; set; } = "Addicted";

    /// <inheritdoc/>
    public override bool ClearInventory { get; set; } = true;

    /// <inheritdoc/>
    public override RoleTypeId Type { get; set; } = RoleTypeId.ClassD;

    /// <inheritdoc/>
    public override List<ItemType> Items { get; set; } = new()
    {
        ItemType.SCP500
    };

    /// <summary>
    /// Janitor spawn conditions.
    /// </summary>
    public override List<SpawnRange> Conditions { get; set; } = new()
    {
        new()
        {
            MinPlayers = 1,
            MaxPlayers = 6,
            OverallChance = 20,
            MaxSpawnCount = 1
        },

        new()
        {
            MinPlayers = 7,
            MaxPlayers = 11,
            OverallChance = 50,
            MaxSpawnCount = 1
        },

        new()
        {
            MinPlayers = 12,
            MaxPlayers = 18,
            OverallChance = 80,
            MaxSpawnCount = 1
        },

        new()
        {
            MinPlayers = 19,
            OverallChance = 100,
            MaxPlayers = -1,
            MaxSpawnCount = 1
        }
    };
}