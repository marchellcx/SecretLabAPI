using PlayerRoles;

using SecretLabAPI.Utilities;

namespace SecretLabAPI.Features.Roles.Classes.Guards;

/// <summary>
/// The guard commander custom role.
/// </summary>
public class GuardCommanderRole : SpawnableCustomRole
{
    /// <inheritdoc/>
    public override string Id { get; } = "guard_commander";

    /// <inheritdoc/>
    public override string Name { get; set; } = "Commander";

    /// <inheritdoc/>
    public override bool ClearInventory { get; set; } = true;

    /// <inheritdoc/>
    public override RoleTypeId Type { get; set; } = RoleTypeId.FacilityGuard;

    /// <inheritdoc/>
    public override List<ItemType> Items { get; set; } = new()
    {
        ItemType.GunE11SR,
        ItemType.KeycardMTFPrivate,
        ItemType.Medkit,
        ItemType.Adrenaline,
        ItemType.GrenadeFlash,
        ItemType.Radio,
        ItemType.ArmorCombat
    };

    /// <inheritdoc/>
    public override Dictionary<ItemType, ushort> Ammo { get; set; } = new()
    {
        { ItemType.Ammo556x45, 120 }
    };

    /// <summary>
    /// Guard Commander spawn conditions.
    /// </summary>
    public override List<SpawnRange> Conditions { get; set; } = new()
    {
        new()
        {
            MinPlayers = 3,
            MaxPlayers = 5,
            OverallChance = 20,
            MaxSpawnCount = 1
        },

        new()
        {
            MinPlayers = 6,
            MaxPlayers = 12,
            OverallChance = 40,
            MaxSpawnCount = 1
        },

        new()
        {
            MinPlayers = 13,
            MaxPlayers = 16,
            OverallChance = 60,
            MaxSpawnCount = 1
        },

        new()
        {
            MinPlayers = 17,
            MaxPlayers = 26,
            OverallChance = 80,
            MaxSpawnCount = 1
        },

        new()
        {
            MinPlayers = 27,
            MaxPlayers = -1,
            OverallChance = 100,
            MaxSpawnCount = 1
        }
    };
}