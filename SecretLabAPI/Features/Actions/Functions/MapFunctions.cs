using LabApi.Features.Wrappers;
using LabExtended.API;

using MapGeneration;

using SecretLabAPI.Extensions;

using UnityEngine;

using Utils;

using SecretLabAPI.Features.Actions.API;
using SecretLabAPI.Features.Actions.Enums;
using SecretLabAPI.Features.Actions.Attributes;

namespace SecretLabAPI.Features.Actions.Functions
{
    /// <summary>
    /// Provides map-related utility functions for performing actions such as spawning projectiles at player positions.
    /// </summary>
    /// <remarks>This static class contains methods that operate on game map contexts, enabling scripted
    /// actions to be performed for all players or entities within the context. All methods require a properly compiled
    /// action context with valid parameters. The class is intended for use in scenarios where map-based actions need to
    /// be triggered programmatically, such as in custom game modes or automated events.</remarks>
    public static class MapFunctions
    {
        /// <summary>
        /// Spawns grenades in every room on the map based on the specified parameters.
        /// </summary>
        /// <remarks>
        /// This method iterates through all available rooms in the map and spawns a specified number of grenades
        /// with defined type and fuse time in each room. It is essential to provide valid parameters in the
        /// context to ensure desired behavior. Misconfigured parameters may result in execution errors.
        /// </remarks>
        /// <param name="context">
        /// The action context containing parameters for the operation. Expected parameters include:
        /// - Type: The type of grenade to spawn (e.g., GrenadeHE).
        /// - Amount: The number of grenades to spawn per room.
        /// - Fuse: The fuse time for each grenade before detonation.
        /// Ensure all parameters have been properly set within the context before invoking the method.
        /// </param>
        /// <returns>
        /// An ActionResultFlags value representing the result of the operation. Typically returns SuccessDispose
        /// if all grenades are successfully spawned in all rooms.
        /// </returns>
        [Action("SpawnGrenadeInEveryRoom", "Spawns a grenade in every room.")]
        [ActionParameter("Type", "The type of grenade to spawn.")]
        [ActionParameter("Amount", "The amount of grenades to spawn.")]
        [ActionParameter("Fuse", "The fuse time of the grenade.")]
        public static ActionResultFlags SpawnGrenadeInEveryRoom(ref ActionContext context)
        {
            context.EnsureCompiled((index, p) =>
            {
                return index switch
                {
                    0 => p.EnsureCompiled(Enum.TryParse, ItemType.GrenadeHE),
                    1 => p.EnsureCompiled(int.TryParse, 1),
                    2 => p.EnsureCompiled(float.TryParse, 3f),
                    
                    _ => false
                };
            });
            
            var type = context.GetValue<ItemType>(0);
            var amount = context.GetValue<int>(1);
            var fuse = context.GetValue<float>(2);
            
            foreach (var room in RoomIdentifier.AllRoomIdentifiers)
            {
                for (var i = 0; i < amount; i++)
                {
                    ExMap.SpawnProjectile(type, room.transform.position, Vector3.one, Vector3.zero,
                        room.transform.rotation,
                        0f, fuse);
                }
            }

            return ActionResultFlags.SuccessDispose;
        }
        
        /// <summary>
        /// Detonates the Alpha Warhead, initiating its explosion and applying corresponding effects to the map.
        /// </summary>
        /// <remarks>
        /// This method triggers the detonation of the Alpha Warhead, simulating its effects on the map.
        /// Once invoked, it cannot be reversed, and gameplay will proceed accordingly.
        /// Use this action responsibly to avoid unintended disruptions.
        /// </remarks>
        /// <param name="context">
        /// The action context passed by the system containing relevant parameters and execution state for the operation.
        /// Ensure the context is valid and properly initialized before calling this method.
        /// </param>
        /// <returns>
        /// An ActionResultFlags value indicating the outcome. Typically returns SuccessDispose if the warhead detonates successfully.
        /// </returns>
        [Action("DetonateWarhead", "Detonates the Alpha Warhead.")]
        public static ActionResultFlags DetonateWarhead(ref ActionContext context)
        {
            Warhead.Detonate();
            return ActionResultFlags.SuccessDispose;
        }
        
        /// <summary>
        /// Flickers the lights in specified zones or across the map, turning them off for a specified duration.
        /// </summary>
        /// <remarks>
        /// This method controls the lighting in the map and can impact gameplay based on the zones and duration
        /// parameters. If the whitelist flag is set to true, only the specified zones will be affected; otherwise,
        /// the zones will be excluded, and all other zones will flicker.
        /// </remarks>
        /// <param name="context">The action context containing parameters such as duration, zones, and whitelist flag.
        /// Ensure the context is compiled before use to access these values.</param>
        /// <returns>An ActionResultFlags value indicating the result. Returns SuccessDispose if the lights flickered successfully.</returns>
        [Action("FlickerLights", "Flickers lights on the map.")]
        [ActionParameter("Duration", "How long the lights will be turned off for (in seconds).")]
        [ActionParameter("Zones", "The list of zones to flicker lights in.")]
        [ActionParameter("Whitelist", "Whether or not the zones parameter should be used as a whitelist.")]
        public static ActionResultFlags FlickerLights(ref ActionContext context)
        {
            context.EnsureCompiled((index, p) =>
            {
                return index switch
                {
                    0 => p.EnsureCompiled(float.TryParse, 0f),
                    1 => p.EnsureCompiled(StringExtensions.TryParseEnumArray, Array.Empty<FacilityZone>()),
                    2 => p.EnsureCompiled(bool.TryParse, false),
                    
                    _ => false
                };
            });
            
            var duration = context.GetValue<float>(0);
            var zones = context.GetValue<FacilityZone[]>(1);
            var whitelist = context.GetValue<bool>(2);
            
            ExMap.FlickerLights(duration, whitelist && zones.Length > 0
                                           ? zones
                                           : EnumUtils<FacilityZone>.Values.Except(zones).ToArray());

            return ActionResultFlags.SuccessDispose;
        }
        
        /// <summary>
        /// Spawns an explosion of the specified type at the position of each player in the current action context.
        /// </summary>
        /// <remarks>The explosion type is determined by the 'Type' parameter in the context. This method
        /// affects all players present in the context and may have gameplay implications depending on the explosion
        /// type used.</remarks>
        /// <param name="context">The action context containing player information and parameters. Must be compiled and include a valid
        /// explosion type.</param>
        /// <returns>An ActionResultFlags value indicating the result of the operation. Returns SuccessDispose if explosions were
        /// spawned successfully.</returns>
        [Action("SpawnExplosion", "Spawns an explosion at each player's position.")]
        [ActionParameter("Type", "The type of explosion to spawn.")]
        public static ActionResultFlags SpawnExplosion(ref ActionContext context)
        {
            context.EnsureCompiled((index, p) =>
            {
                return index switch
                {
                    0 => p.EnsureCompiled(Enum.TryParse, ExplosionType.Grenade), // Type

                    _ => false
                };
            });

            if (context.Player?.ReferenceHub != null)
            {
                var type = context.GetValue<ExplosionType>(0);

                ExplosionUtils.ServerExplode(context.Player.ReferenceHub, type);
            }

            return ActionResultFlags.SuccessDispose;
        }

        /// <summary>
        /// Spawns the specified item type at each player's current position, creating the given number of items per
        /// player with the specified scale.
        /// </summary>
        /// <remarks>The method uses the parameters 'Type', 'Amount', and 'Scale' from the context to
        /// determine the item type, quantity, and scale for spawning. Items are spawned for each player present in the
        /// context. If the item type is None or the amount is not positive, no items are spawned and the action
        /// completes successfully.</remarks>
        /// <param name="context">A reference to the action context containing parameters for item type, amount, and scale, as well as player
        /// information.</param>
        /// <returns>An ActionResultFlags value indicating the result of the spawn operation. Returns SuccessDispose if the
        /// action completes or if the item type is None or amount is less than or equal to zero.</returns>
        [Action("SpawnItem", "Spawns an item at each player's position.")]
        [ActionParameter("Type", "The type of item to spawn.")]
        [ActionParameter("Amount", "The amount of items to spawn.")]
        [ActionParameter("Scale", "The scale of the spawned item.")]
        public static ActionResultFlags SpawnItem(ref ActionContext context)
        {
            context.EnsureCompiled((index, p) =>
            {
                return index switch
                {
                    0 => p.EnsureCompiled(Enum.TryParse, ItemType.None), // Type
                    1 => p.EnsureCompiled(int.TryParse, 1), // Amount
                    2 => p.EnsureCompiled(SecretLabAPI.Extensions.StringExtensions.TryParseVector3, Vector3.one), // Scale

                    _ => false
                };
            });

            if (context.Player?.ReferenceHub != null)
            {
                var type = context.GetValue<ItemType>(0);
                var amount = context.GetValue<int>(1);
                var scale = context.GetValue<Vector3>(2);

                if (type is ItemType.None || amount <= 0)
                    return ActionResultFlags.SuccessDispose;

                for (var i = 0; i < amount; i++)
                    ExMap.SpawnItem(type, context.Player.Position, scale, context.Player.Rotation);
            }

            return ActionResultFlags.SuccessDispose;
        }

        /// <summary>
        /// Spawns a projectile of the specified type at each player's position, applying the given force and fuse time.
        /// </summary>
        /// <remarks>The method retrieves parameters from the context: 'Type' (projectile type), 'Amount'
        /// (number of projectiles), 'Force' (applied force), and 'Fuse' (fuse time). All players in the context will
        /// receive a projectile at their current position. Ensure that the context is properly compiled and contains
        /// valid parameter values before calling this method.</remarks>
        /// <param name="context">The action context containing parameters for projectile type, amount, force, and fuse time. Must be compiled
        /// before invocation.</param>
        /// <returns>An ActionResultFlags value indicating the result of the action. Returns SuccessDispose if the projectiles
        /// were spawned successfully.</returns>
        [Action("SpawnProjectile", "Spawns a projectile at each player's position.")]
        [ActionParameter("Type", "The type of projectile to spawn.")]
        [ActionParameter("Amount", "The amount of projectiles to spawn.")]
        [ActionParameter("Force", "The force to apply to the projectile.")]
        [ActionParameter("Fuse", "The fuse time of the projectile.")]
        [ActionParameter("Velocity", "Multiplier for the player's velocity.")]
        public static ActionResultFlags SpawnProjectile(ref ActionContext context)
        {
            context.EnsureCompiled((index, p) =>
            {
                return index switch
                {
                    0 => p.EnsureCompiled(Enum.TryParse, ItemType.GrenadeHE),
                    1 => p.EnsureCompiled(int.TryParse, 1),
                    2 => p.EnsureCompiled(float.TryParse, 10f),
                    3 => p.EnsureCompiled(float.TryParse, 3f),
                    4 => p.EnsureCompiled(float.TryParse, 1f),

                    _ => false
                };
            });

            if (context.Player?.ReferenceHub != null)
            {
                var type = context.GetValue<ItemType>(0);
                var amount = context.GetValue<int>(1);
                var force = context.GetValue<float>(2);
                var fuse = context.GetValue<float>(3);
                var velocity = context.GetValue<float>(4);
                var vectorVelocity = context.Player.MultipliedVelocity(velocity);

                for (var i = 0; i < amount; i++)
                    ExMap.SpawnProjectile(type, context.Player.Position, Vector3.one, vectorVelocity,
                        context.Player.Rotation, force, fuse);
            }

            return ActionResultFlags.SuccessDispose;
        }
    }
}