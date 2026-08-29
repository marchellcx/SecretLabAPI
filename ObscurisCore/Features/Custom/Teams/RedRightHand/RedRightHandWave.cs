using LabExtended.API;
using LabExtended.API.Custom.Teams;

using LabExtended.Extensions;
using LabExtended.Utilities;

using NorthwoodLib.Pools;

using ObscurisCore.Features.Loadouts;
using ObscurisCore.Features.Elements.Alerts;

using PlayerRoles;

namespace ObscurisCore.Features.Custom.Teams.RedRightHand;

/// <summary>
/// An instance of a Red Right Hand team.
/// </summary>
public class RedRightHandWave : CustomTeamInstance<RedRightHandTeam>
{
    /// <inheritdoc cref="CustomTeamInstance.OnSpawned"/>
    public override void OnSpawned()
    {
        base.OnSpawned();

        if (RedRightHandTeam.CassieMessage)
        {
            LabApi.Features.Wrappers.Cassie.Message(StringBuilderPool.Shared.BuildString(x =>
            {
                var scpsLeft = ExPlayer.Players.Count(x => x.Role.IsScpButNotZombie);

                x.Append("Mtfunit Red Right Hand under the order of the O5 hasentered allremaining ");

                if (scpsLeft == 0)
                {
                    x.Append("noscpsleft");
                    return;
                }

                x.Append("awaitingrecontainment ");
                x.Append(scpsLeft);
                x.Append(scpsLeft == 1 ? " scpsubject" : " scpsubjects");
            }), false, true, false);
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
                case RoleTypeId.NtfCaptain:
                    loadout = "Hand1";
                    break;

                case RoleTypeId.NtfSpecialist:
                    loadout = "Hand2";
                    break;

                case RoleTypeId.NtfSergeant:
                    loadout = "Hand3";
                    break;
            }

            if (!string.IsNullOrEmpty(loadout))
            {
                LoadoutManager.TryApply(player, loadout);
            }

            player.SendAlert(AlertType.Info, 10f, "Red Right Hand",
                $"<b>Jsi členem týmu</b>\n" +
                $"<b><color=blue><size=30>Red Right Hand</size></color></b>\n" +
                $"<b>Máš stejný objektiv jako normální <color=blue>Nine-Tailed Fox</color>.</b>");
        }, 0.2f);
    }
}