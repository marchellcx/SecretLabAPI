using LabExtended.API;
using LabExtended.API.Custom.Teams;

using LabExtended.Utilities;

using ObscurisCore.Features.Loadouts;
using ObscurisCore.Features.Elements.Alerts;

using PlayerRoles;

namespace ObscurisCore.Features.Custom.Teams.Archangels;

/// <summary>
/// An instance of Zeta-3 Archangels wave.
/// </summary>
public class ArchangelsWave : CustomTeamInstance<ArchangelsTeam>
{
    /// <summary>
    /// The CASSIE announcement for the Archangels team.
    /// </summary>
    public const string CassieMessage =
        "Attention security personnel . Unknown chaos insurgency operative team spotted at gate A . Find safe shelter and wait for ninetailedfox backu";

    /// <inheritdoc cref="CustomTeamInstance.OnSpawned"/>
    public override void OnSpawned()
    {
        base.OnSpawned();

        if (ArchangelsTeam.CassieMessage)
        {
            LabApi.Features.Wrappers.Cassie.Message(CassieMessage, false, true, false);
        }
    }

    /// <inheritdoc cref="CustomTeamInstance.SpawnPlayer"/>
    public override void SpawnPlayer(ExPlayer player, RoleTypeId role)
    {
        player.Role.Set(role, RoleChangeReason.Respawn, RoleSpawnFlags.UseSpawnpoint);

        TimingUtils.AfterSeconds(() =>
        {
            var loadout = string.Empty;

            switch (player.Role.Type)
            {
                // Archangels 1
                case RoleTypeId.ChaosRepressor:
                    loadout = "Archangels1";
                    break;

                // Archangels2
                case RoleTypeId.ChaosRifleman:
                    loadout = "Archangels2";
                    break;

                // Archangels 3
                case RoleTypeId.ChaosMarauder:
                    loadout = "Archangels3";
                    break;
            }

            if (!string.IsNullOrEmpty(loadout))
            {
                LoadoutManager.TryApply(player, loadout);
            }

            player.SendAlert(AlertType.Info, 10f, "Archangels",
                $"<b>Jsi členem týmu</b>\n" +
                $"<b><color=green><size=30>Archangels</size></color></b>\n" +
                $"<b>Máš stejný objektiv jako normální <color=green>Chaos Insurgent</color>.</b>");
        }, 0.2f);
    }
}