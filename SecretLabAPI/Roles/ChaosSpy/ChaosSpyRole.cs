using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Arguments.ServerEvents;

using LabApi.Events.Handlers;

using LabExtended.API;
using LabExtended.API.Custom.Roles;

using LabExtended.Extensions;
using LabExtended.Utilities;

using PlayerRoles;

using SecretLabAPI.Elements.Alerts;

using System.ComponentModel;

using UnityEngine;

namespace SecretLabAPI.Roles.ChaosSpy
{
    /// <summary>
    /// Implementation of the Chaos Insurgency spy role.
    /// </summary>
    public class ChaosSpyRole : CustomRole
    {
        /// <summary>
        /// Gets the global config loaded role instance.
        /// </summary>
        public static ChaosSpyRole Instance { get; private set; }

        /// <inheritdoc/>
        public override string Id { get; } = "chaos_spy";

        /// <inheritdoc/>
        public override string Name { get; set; } = "Chaos Spy";

        /// <inheritdoc/>
        public override RoleTypeId Type { get; set; } = RoleTypeId.NtfPrivate;

        /// <summary>
        /// Gets or sets the number of seconds the ability remains on cooldown after the spy shoots someone.
        /// </summary>
        [Description("Sets the amount of seconds the ability will be in cooldown after the spy shoots someone.")]
        public int DisguiseCoooldown { get; set; } = 20;

        /// <summary>
        /// Gets or sets the minimum size of an NTF wave required for infiltration.
        /// </summary>
        [Description("Sets the minimum size of an NTF wave required for infiltration.")]
        public int MinWaveSize { get; set; } = 0;

        /// <summary>
        /// Gets or sets the chance that a squad infiltration attempt will succeed.
        /// </summary>
        [Description("Sets the chance of a squad infiltration.")]
        public float SpyChance { get; set; } = 10f;

        /// <inheritdoc/>
        public override void OnRegistered()
        {
            base.OnRegistered();
            ServerEvents.WaveRespawned += OnWaveSpawned;
        }

        /// <inheritdoc/>
        public override void OnUnregistered()
        {
            base.OnUnregistered();
            ServerEvents.WaveRespawned -= OnWaveSpawned;
        }

        /// <inheritdoc/>
        public override RoleTypeId GetAppearance(ExPlayer player, ExPlayer receiver, RoleTypeId appearance, ref object? data)
        {
            if (player == receiver)
                return RoleTypeId.ChaosRifleman;

            if (data is ChaosSpyData chaosSpyData && chaosSpyData.IsInCooldown)
                return RoleTypeId.NtfPrivate;

            if (receiver.Role.IsChaosOrClassD || receiver.IsInOverwatch)
                return RoleTypeId.ChaosRifleman;

            return RoleTypeId.NtfPrivate;
        }

        /// <inheritdoc/>
        public override void Update(ExPlayer player, ref object? data)
        {
            if (data is not ChaosSpyData chaosSpyData)
                return;

            if (!chaosSpyData.IsInCooldown)
                return;

            if (Time.realtimeSinceStartup >= chaosSpyData.CooldownEndTime)
            {
                chaosSpyData.IsInCooldown = false;

                player.SendAlert(AlertType.Info, 5f, "Chaos Spy",
                    "<b>Tvůj převlek je znova <color=green>aktivní</color>!</b>", true);
            }
        }

        /// <inheritdoc/>
        public override void OnAdded(ExPlayer player, ref object? data)
        {
            base.OnAdded(player, ref data);

            data = new ChaosSpyData();
        }

        /// <inheritdoc/>
        public override void OnSpawned(ExPlayer player, ref object? data)
        {
            base.OnSpawned(player, ref data);

            player.SendAlert(AlertType.Info, 10f, "Chaos Spy", $"" +
                                                              $"<b>Jsi <color=green>Chaos Spy</color>!</b>\n" +
                                                              $"<b>Dodržuj stejná pravidla jako normální <color=green>Chaos Insurgent</color>!</b>\n" +
                                                              $"<b>Budeš odhalen jakmile střelíš do enemy týmu (<color=yellow>Scientist</color>, <color=blue>MTF</color>)</b>");

            ExPlayer.Players.ForEach(ply =>
            {
                if (ply.Role.Faction != Faction.FoundationEnemy)
                    return;

                if (ply == player)
                    return;

                ply.SendAlert(AlertType.Info, 10f, "Chaos Spy",
                    $"<b>Hráč <color=red>{player.Nickname}</color> se spawnul jako <color=green>Chaos Spy</color>!</b>", true);
            });
        }

        /// <inheritdoc/>
        public override void OnAttacked(PlayerHurtEventArgs args, ref object? data)
        {
            base.OnAttacked(args, ref data);

            if (args.Attacker is not ExPlayer attacker)
                return;

            if (args.Player is not ExPlayer target)
                return;

            if (attacker == target)
                return;

            if (target.Role.Faction != Faction.FoundationStaff)
                return;

            if (data is not ChaosSpyData chaosSpyData)
                data = chaosSpyData = new();

            if (chaosSpyData.IsInCooldown)
                return;

            chaosSpyData.CooldownEndTime = Time.realtimeSinceStartup + DisguiseCoooldown;
            chaosSpyData.IsInCooldown = true;

            attacker.SendAlert(AlertType.Info, 10f, "Chaos Spy",
                $"<b>Tvůj převlek byl odhalen! Znovu aktivní bude za <color=yellow>{DisguiseCoooldown} sekund</color>!</b>", true);
        }

        /// <inheritdoc/>
        public override void OnKilled(PlayerDeathEventArgs args, ref object? data)
        {
            base.OnKilled(args, ref data);

            if (args.Attacker is not ExPlayer attacker)
                return;

            if (args.Player is not ExPlayer target)
                return;

            target.SendAlert(AlertType.Info, 10f, "Chaos Spy",
                $"<b>Zabil tě hráč <color=red>{attacker.Nickname}</color> jako <color=green>Chaos Spy</color>!</b>", true);
        }

        private void OnWaveSpawned(WaveRespawnedEventArgs args)
        {
            if (args.Wave.Faction is Faction.FoundationStaff)
            {
                var validPlayers = args.Players.Where(x => x is ExPlayer && x.ReferenceHub != null && x.Role is RoleTypeId.NtfPrivate);
                var validCount = validPlayers.Count();

                if (validCount < 1)
                    return;

                if (MinWaveSize > 0 && validCount < MinWaveSize)
                    return;

                if (SpyChance <= 0f)
                    return;

                if (SpyChance < 100f && !WeightUtils.GetBool(SpyChance))
                    return;

                var target = validPlayers.GetRandomItem();

                if (target?.ReferenceHub == null)
                    return;

                Give((ExPlayer)target);
            }
            else if (args.Wave.Faction is Faction.FoundationEnemy)
            {
                ExPlayer.Players.ForEach(ply =>
                {
                    if (!ply.Role.IsAlive)
                        return;

                    if (!ply.Role.IsCustom<ChaosSpyRole>())
                        return;

                    args.Players.ForEach(x =>
                    {
                        ((ExPlayer)x).SendAlert(AlertType.Info, 10f, "Chaos Spy", 
                            $"<b>Hráč <color=red>{ply.Nickname}</color> je <color=green>Chaos Spy</color>!</b>", true);
                    });
                });
            }
        }

        internal static void Initialize()
        {
            Instance = SecretLab.LoadConfig(false, "chaos_spy", () => new ChaosSpyRole());
            Instance.Register();
        }
    }
}