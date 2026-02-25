using LabExtended.API;
using LabExtended.Events;

using System.Text;

using UnityEngine;

using PlayerRoles;
using PlayerRoles.PlayableScps.Scp3114;

namespace SecretLabAPI.Features
{
    /// <summary>
    /// Tracks player's health and updates their custom info display accordingly.
    /// </summary>
    public static class PlayerInfoHealth
    {
        private static void Internal_RefreshingCustomInfo(ExPlayer player, StringBuilder builder)
        {
            if (!player.Role.IsAlive)
                return;

            var health = Mathf.CeilToInt(player.Health);
            var maxHealth = Mathf.CeilToInt(player.MaxHealth);

            if (player.Role.Is(RoleTypeId.Scp3114)
                && player.Subroutines.Scp3114Identity.CurIdentity != null
                && player.Subroutines.Scp3114Identity.CurIdentity.Status 
                    is Scp3114Identity.DisguiseStatus.Active or Scp3114Identity.DisguiseStatus.Equipping)
            {
                if (player.Subroutines.Scp3114Identity.CurIdentity.StolenRole.TryGetRoleTemplate<PlayerRoleBase>(out var role)
                    && role is IHealthbarRole healthbarRole)
                {
                    health = Mathf.CeilToInt(healthbarRole.MaxHealth * Mathf.CeilToInt((player.Health / player.MaxHealth) * 100f) / 100f);
                    maxHealth = Mathf.CeilToInt(healthbarRole.MaxHealth);
                }
                else
                {
                    health = Mathf.CeilToInt(Mathf.Clamp(player.Health, 0f, 100f));
                    maxHealth = Mathf.CeilToInt(Mathf.Clamp(player.MaxHealth, 0f, 100f));
                }
            }

            builder.AppendLine($"{health} HP / {maxHealth} HP");
        }

        internal static void Internal_Init()
        {
            ExPlayerEvents.RefreshingCustomInfo += Internal_RefreshingCustomInfo;
        }
    }
}