using LabExtended.API;

using PlayerRoles;

using SecretLabAPI.Utilities;
using SecretLabAPI.Extensions;

namespace SecretLabAPI.Features.Roles.Classes.ClassD;

/// <summary>
/// Represents a specialized role for Class-D personnel known as "Sprinter."
/// </summary>
/// <remarks>
/// This role enhances the mobility of Class-D personnel by increasing their movement speed,
/// and is governed by specific spawn conditions. The role is designed for dynamic gameplay
/// scenarios where Class-D can leverage their enhanced speed.
/// </remarks>
public class ClassDSprinterRole : SpawnableCustomRole
{
    /// <inheritdoc/>
    public override string Id { get; } = "classd_sprinter";

    /// <inheritdoc/>
    public override string Name { get; set; } = "Sprinter";

    /// <inheritdoc/>
    public override bool ClearInventory { get; set; } = true;

    /// <inheritdoc/>
    public override RoleTypeId Type { get; set; } = RoleTypeId.ClassD;

    /// <inheritdoc/>
    public override List<ItemType> Items { get; set; } = new();

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
    /// Executes actions when the Janitor role is spawned.
    /// </summary>
    /// <param name="player">The player assigned to the Janitor role.</param>
    /// <param name="data">Optional data that may influence the spawn behavior.</param>
    public override void OnSpawned(ExPlayer player, ref object? data)
    {
        base.OnSpawned(player, ref data);

        player.ChangeSpeedByMultiplier(1.2f);
    }
}