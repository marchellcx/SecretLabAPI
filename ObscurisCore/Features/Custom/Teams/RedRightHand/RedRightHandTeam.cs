using LabExtended.API;
using LabExtended.API.Custom.Teams;

using NiveraAPI.IO.Configs;

using ObscurisCore.Features.Loadouts;

using PlayerRoles;

namespace ObscurisCore.Features.Custom.Teams.RedRightHand;

/// <summary>
/// Alpha-1 Red Right Hand team handler.
/// </summary>
public class RedRightHandTeam : CustomTeamHandler<RedRightHandWave>
{
    /// <summary>
    /// Gets the singleton instance of the team handler.
    /// </summary>
    public static RedRightHandTeam Singleton { get; private set; }

    /// <summary>
    /// Gets the CASSIE announcement.
    /// </summary>
    [Config("red-right-hand", "cassie-message", "Whether to play the CASSIE announcement for the Red Right Hand team.")]
    public static bool CassieMessage { get; set; }

    /// <inheritdoc cref="CustomTeamHandler.Name"/>
    public override string? Name { get; } = "Alpha-1 \"Red Right Hand\"";

    /// <inheritdoc cref="CustomTeamHandler.IsSpawnable"/>
    public override bool IsSpawnable(ExPlayer player)
        => player.CanBeRespawned;

    /// <inheritdoc cref="CustomTeamHandler.SelectRole"/>
    public override RoleTypeId SelectRole(ExPlayer player, Dictionary<ExPlayer, RoleTypeId> selectedRoles)
    {
        if (!selectedRoles.Any(x => x.Value == RoleTypeId.NtfCaptain))
            return RoleTypeId.NtfCaptain;

        if (!selectedRoles.Any(x => x.Value == RoleTypeId.NtfSpecialist))
            return RoleTypeId.NtfSpecialist;

        return RoleTypeId.NtfSergeant;
    }

    /// <inheritdoc cref="CustomTeamHandler.OnRegistered"/>
    public override void OnRegistered()
    {
        base.OnRegistered();

        Singleton = this;

        LoadoutManager.Ensure("Hand1", new LoadoutDefinition()
            .WithHealth(250f, 250f)
            .WithAmmo(ItemType.Ammo556x45, 120)
            .WithItems(ItemType.GunFRMG0, ItemType.GrenadeHE, ItemType.KeycardMTFCaptain, ItemType.Medkit, ItemType.Adrenaline, ItemType.Radio, ItemType.ArmorHeavy));

        LoadoutManager.Ensure("Hand2", new LoadoutDefinition()
            .WithHealth(250f, 250f)
            .WithAmmo(ItemType.Ammo556x45, 120)
            .WithAmmo(ItemType.Ammo9x19, 120)
            .WithCustomItem("SniperRifle")
            .WithItems(ItemType.GunCOM18, ItemType.GrenadeHE, ItemType.KeycardMTFCaptain, ItemType.Medkit, ItemType.Adrenaline, ItemType.Radio, ItemType.ArmorHeavy));

        LoadoutManager.Ensure("Hand3", new LoadoutDefinition()
            .WithHealth(250f, 250f)
            .WithAmmo(ItemType.Ammo556x45, 120)
            .WithItems(ItemType.GunFRMG0, ItemType.GrenadeHE, ItemType.KeycardMTFCaptain, ItemType.Medkit, ItemType.Adrenaline, ItemType.Radio, ItemType.ArmorHeavy));
    }
}