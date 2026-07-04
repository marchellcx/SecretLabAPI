using PlayerRoles;

using SecretLabAPI.Utilities;

namespace SecretLabAPI.Features.Roles.Classes.ClassD;

/// <summary>
/// Represents a custom spawnable role named "Sneaky," part of the Class D team.
/// This role is designed to be spawned conditionally at round start with specific attributes and items.
/// </summary>
public class ClassDSneakyRole : SpawnableCustomRole
{
    /// <inheritdoc/>
    public override string Id { get; } = "classd_sneaky";

    /// <inheritdoc/>
    public override string Name { get; set; } = "Sneaky";

    /// <inheritdoc/>
    public override bool ClearInventory { get; set; } = true;

    /// <inheritdoc/>
    public override RoleTypeId Type { get; set; } = RoleTypeId.ClassD;

    /// <inheritdoc/>
    public override List<ItemType> Items { get; set; } = new()
    {
        ItemType.KeycardJanitor
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