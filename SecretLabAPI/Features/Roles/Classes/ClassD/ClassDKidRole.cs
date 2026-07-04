using System.ComponentModel;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;

using LabExtended.Core.Configs.Objects;

using PlayerRoles;

using SecretLabAPI.Utilities;
using SecretLabAPI.Extensions;

namespace SecretLabAPI.Features.Roles.Classes.ClassD;

/// <summary>
/// Represents a custom role for a Class-D character defined as "Kid" in the game.
/// This role is a subclass of <c>SpawnableCustomRole</c>, with specific spawn conditions,
/// inventory settings, and interaction rules.
/// </summary>
public class ClassDKidRole : SpawnableCustomRole
{
    /// <inheritdoc/>
    public override string Id { get; } = "classd_kid";

    /// <inheritdoc/>
    public override string Name { get; set; } = "Kid";

    /// <inheritdoc/>
    public override bool ClearInventory { get; set; } = true;

    /// <inheritdoc/>
    public override RoleTypeId Type { get; set; } = RoleTypeId.ClassD;

    /// <inheritdoc/>
    public override List<ItemType> Items { get; set; } = new();

    /// <summary>
    /// Defines the scale of the role's player model in the game.
    /// </summary>
    /// <remarks>
    /// The scale is represented as a <see cref="YamlVector3"/> object,
    /// which includes individual scaling factors for the X, Y, and Z axes.
    /// Adjusting these values directly affects the visual size of the player model in each corresponding dimension.
    /// </remarks>
    public override YamlVector3 Scale { get; set; } = new(1f, 0.7f, 1f);

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
    /// The maximum number of uses for the role.
    /// </summary>
    [Description("Sets the maximum number of uses for the role.")]
    public int MaxUses { get; set; } = 3;

    /// <summary>
    /// Called when the custom role is registered.
    /// This method is overridden to add role-specific event handlers or initialize custom behaviors.
    /// For the <see cref="ClassDKidRole"/>, it subscribes to the <see cref="PlayerEvents.InteractingScp330"/> event
    /// to enforce custom behavior when interacting with SCP-330.
    /// </summary>
    public override void OnRegistered()
    {
        base.OnRegistered();
        
        PlayerEvents.InteractingScp330 += OnTakingCandy;
    }

    private void OnTakingCandy(PlayerInteractingScp330EventArgs args)
    {
        if (!args.Player.CastPlayer(out var player))
            return;

        if (!player.Role.IsCustom<ClassDKidRole>())
            return;
        
        args.AllowPunishment = args.Uses >= MaxUses;
    }
}