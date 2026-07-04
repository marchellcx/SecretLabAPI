using LabExtended.API;

using PlayerRoles;

using SecretLabAPI.Utilities;

namespace SecretLabAPI.Features.Roles.Classes.Guards;

/// <summary>
/// Represents the "Guard Captain" role in the game, which is a spawnable custom role.
/// </summary>
public class GuardCaptainRole : SpawnableCustomRole
{
    /// <inheritdoc/>
    public override string Id { get; } = "guard_captain";

    /// <inheritdoc/>
    public override string Name { get; set; } = "Captain";

    /// <inheritdoc/>
    public override bool ClearInventory { get; set; } = true;

    /// <inheritdoc/>
    public override RoleTypeId Type { get; set; } = RoleTypeId.FacilityGuard;

    /// <inheritdoc/>
    public override List<ItemType> Items { get; set; } = new()
    {
        ItemType.KeycardGuard,
        ItemType.GunCrossvec,
        ItemType.ArmorCombat,
        ItemType.GrenadeFlash,
        ItemType.Radio
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

    /// <summary>
    /// Executes actions when the role is spawned.
    /// </summary>
    /// <param name="player">The player assigned to the role.</param>
    /// <param name="data">Optional data that may influence the spawn behavior.</param>
    public override void OnSpawned(ExPlayer player, ref object? data)
    {
        base.OnSpawned(player, ref data);
    }
}