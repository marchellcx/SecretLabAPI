using PlayerRoles;

using SecretLabAPI.Utilities;

namespace SecretLabAPI.Features.Roles.Classes.Scientists;

/// <summary>
/// Represents the MedicRole, a custom spawnable role derived from the SpawnableCustomRole base class.
/// The MedicRole is designed to provide healing utility within the game and inherits core
/// spawning functionality and role-related properties.
/// </summary>
public class ScientistMedicRole : SpawnableCustomRole
{
    /// <inheritdoc/>
    public override string Id { get; } = "medic";

    /// <inheritdoc/>
    public override string Name { get; set; } = "Medic";

    /// <inheritdoc/>
    public override bool ClearInventory { get; set; } = true;

    /// <inheritdoc/>
    public override RoleTypeId Type { get; set; } = RoleTypeId.Scientist;

    /// <inheritdoc/>
    public override List<ItemType> Items { get; set; } = new()
    {
        ItemType.Medkit,
        ItemType.Medkit,
        ItemType.Medkit,
        ItemType.KeycardScientist
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