using System.ComponentModel;

using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Arguments.Scp049Events;

using LabApi.Events.Handlers;

using LabExtended.API;
using LabExtended.API.Custom.Roles;

using LabExtended.Core;
using LabExtended.Utilities;
using MEC;
using NiveraAPI.IO.Configs;
using PlayerRoles;
using PlayerStatsSystem;

using SecretLabAPI.Extensions;
using SecretLabAPI.Features.Elements.Alerts;
using SecretLabAPI.Features.Misc;
using UnityEngine;

using YamlDotNet.Serialization;

namespace SecretLabAPI.Features.Roles.Zombies;

/// <summary>
/// Represents a base class for zombie roles.
/// </summary>
public abstract class ZombieRole : CustomRole
{
    [Config("zombies", "tank", "Configuration for Zombie Tank")] private static ZombieTank zombieTank = new();
    [Config("zombies", "regular", "Configuration for Zombie Regular")] private static ZombieRegular zombieRegular = new();
    [Config("zombies", "vampire", "Configuration for Zombie Vampire")] private static ZombieVampire zombieVampire = new();
    [Config("zombies", "sprinter", "Configuration for Zombie Sprinter")] private static ZombieSprinter zombieSprinter = new();
    [Config("zombies", "explosive", "Configuration for Zombie Explosive")] private static ZombieExplosive zombieExplosive = new();
    [Config("zombies", "juggernaut", "Configuration for Zombie Juggernaut")] private static ZombieJuggernaut zombieJuggernaut = new();
    
    /// <summary>
    /// Gets the list of all registered zombie roles.
    /// </summary>
    public static List<ZombieRole> Roles { get; } = new();
    
    /// <summary>
    /// Gets or sets the spawn chance of the role.
    /// </summary>
    [Description("Sets the spawn chance of the role.")]
    public abstract float SpawnChance { get; set; }
    
    /// <summary>
    /// Gets or sets the damage dealt by the role.
    /// </summary>
    [Description("Sets the damage dealt by the role.")]
    public abstract float Damage { get; set; }
    
    /// <summary>
    /// Gets or sets the speed of the role.
    /// </summary>
    [Description("Sets the speed of the role.")]
    public abstract float Speed { get; set; }
    
    /// <summary>
    /// Gets or sets the size of the role.
    /// </summary>
    [Description("Sets the size of the role.")]
    public abstract float Size { get; set; }
    
    /// <summary>
    /// The spawn message of the role.
    /// </summary>
    [YamlIgnore]
    public abstract string Message { get; }
    
    /// <summary>
    /// Gets or sets the type of the role.
    /// </summary>
    public override RoleTypeId Type { get; set; } = RoleTypeId.Scp0492;

    /// <summary>
    /// Handles the attacking behavior of the zombie role. This method modifies the damage dealt during an attack
    /// based on the specified damage value of the role.
    /// </summary>
    /// <param name="args">The event arguments containing details of the player's attack.</param>
    /// <param name="data">Additional data that can be passed and modified during the attack process.</param>
    public override void OnAttacking(PlayerHurtingEventArgs args, ref object? data)
    {
        base.OnAttacking(args, ref data);

        ApiLog.Debug($"[{Id}] OnAttacking {args.DamageHandler?.GetType()?.Name ?? "null"} with Damage: {Damage}");
        
        if (Damage > 0f)
        {
            ApiLog.Debug($"[{Id}] Applying damage override: {Damage}");
            
            if (args.DamageHandler is StandardDamageHandler handler)
            {
                handler.Damage = Damage;
                
                ApiLog.Debug($"[{Id}] Damage override applied: {Damage}={handler.Damage}");
            }
            else
            {
                ApiLog.Warn($"[{Id}] Damage override could not be applied: {args.DamageHandler?.GetType()?.Name ?? "null"}");
            }
        }
    }

    /// <summary>
    /// Executes logic when the zombie role spawns. Adjusts the player's movement speed and applies any necessary effects
    /// based on the specified speed value of the role.
    /// </summary>
    /// <param name="player">The player instance associated with the spawned zombie role.</param>
    /// <param name="data">Additional data that can be passed and possibly modified during the spawning process.</param>
    public override void OnSpawned(ExPlayer player, ref object? data)
    {
        base.OnSpawned(player, ref data);
        
        ApiLog.Debug($"[{Id}] OnSpawned: Speed={Speed} Size={Size}");

        player.ChangeSpeedByMultiplier(Speed);

        if (Size is < 1f or > 1f)
            player.SetFakeScale(Vector3.one * Size, true, true, true);
        
        if (!string.IsNullOrEmpty(Message))
            player.SendAlert(AlertType.Info, 10f, "Zombie Role", Message, true);
    }

    /// <summary>
    /// Handles cleanup operations when the zombie role is removed from a player.
    /// This includes resetting the player's speed and removing any applied fake scale visual effects.
    /// </summary>
    /// <param name="player">The player instance from whom the role is being removed.</param>
    /// <param name="data">Additional data that can be passed and potentially modified during the removal process.</param>
    public override void OnRemoved(ExPlayer player, ref object? data)
    {
        base.OnRemoved(player, ref data);
        
        ApiLog.Debug($"[{Id}] OnRemoved");

        player.ResetSpeed();
        player.RemoveFakeScale();
    }

    /// <inheritdoc />
    public override void OnChangedRole(PlayerChangedRoleEventArgs args, ref object? data)
    {
        base.OnChangedRole(args, ref data);

        ApiLog.Debug($"[{Id}] OnChangedRole");
        
        if (!args.Player.CastPlayer(out var player))
            return;
        
        player.ResetSpeed();
        player.RemoveFakeScale();
    }

    private static void OnRevived(Scp049ResurrectedBodyEventArgs args)
    {
        try
        {
            if (!args.Target.CastPlayer(out var player))
                return;
            
            ApiLog.Debug($"Player revived: {player.ToLogString()}");

            var role = Roles.GetRandomWeighted(r => r.SpawnChance);

            if (role == null)
            {
                ApiLog.Debug("Null role selected");
                return;
            }

            ApiLog.Debug($"Selected zombie role: &1{role.Name}&r");

            Timing.CallDelayed(1f, () => role.Give(player));
        }
        catch (Exception ex)
        {
            ApiLog.Error(ex);
        }
    }
    
    private static void Initialize()
    {
        Scp049Events.ResurrectedBody += OnRevived;
        
        Roles.Add(zombieTank);
        Roles.Add(zombieRegular);
        Roles.Add(zombieVampire);
        Roles.Add(zombieSprinter);
        Roles.Add(zombieExplosive);
        Roles.Add(zombieJuggernaut);
        
        Roles.ForEach(r => r.Register());
    }
}