using PlayerRoles;

using SecretLabAPI.Utilities;

namespace SecretLabAPI.Features.Roles.Classes.Scientists;

/// <summary>
/// The zone manager custom role.
/// </summary>
public class ScientistZoneManagerRole : SpawnableCustomRole
{
    /// <inheritdoc/>
    public override string Id { get; } = "zoneManager";

    /// <inheritdoc/>
    public override string Name { get; set; } = "Zone Manager";

    /// <inheritdoc/>
    public override bool ClearInventory { get; set; } = true;

    /// <inheritdoc/>
    public override RoleTypeId Type { get; set; } = RoleTypeId.Scientist;

    /// <inheritdoc/>
    public override List<ItemType> Items { get; set; } = new()
    {
        ItemType.Medkit,
        ItemType.Medkit,
        ItemType.ArmorLight,
        ItemType.KeycardZoneManager
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
            OverallChance = 5,
            MaxSpawnCount = 1
        },

        new()
        {
            MinPlayers = 7,
            MaxPlayers = 11,
            OverallChance = 10,
            MaxSpawnCount = 2
        },

        new()
        {
            MinPlayers = 12,
            MaxPlayers = 18,
            OverallChance = 15,
            MaxSpawnCount = 3
        },

        new()
        {
            MinPlayers = 19,
            OverallChance = 30,
            MaxPlayers = -1,
            MaxSpawnCount = 4
        }
    };
}