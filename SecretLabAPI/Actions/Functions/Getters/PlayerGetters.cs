using CustomPlayerEffects;
using LabExtended.API;
using LabExtended.API.Containers;
using LabExtended.Extensions;

using MapGeneration;

using PlayerRoles;
using ProjectMER.Commands.Utility;
using SecretLabAPI.Actions.API;
using SecretLabAPI.Actions.Enums;
using SecretLabAPI.Actions.Attributes;

using SecretLabAPI.Extensions;
using StringExtensions = SecretLabAPI.Extensions.StringExtensions;

namespace SecretLabAPI.Actions.Functions.Getters
{
    /// <summary>
    /// Provides functionality for retrieving player information and performing operations on players within the context of actions.
    /// </summary>
    public static class PlayerGetters
    {
        /// <summary>
        /// Represents a set of criteria for filtering players within specific conditions or contexts.
        /// </summary>
        public struct PlayerFilters
        {
            /// <summary>
            /// Represents the default set of player filters with no specific conditions applied.
            /// </summary>
            public static readonly PlayerFilters Default = new();
            
            /// <summary>
            /// Whether or not the player has to be alive.
            /// </summary>
            public bool? IsAlive;

            /// <summary>
            /// Whether or not the player has to be grounded.
            /// </summary>
            public bool? IsGrounded;

            /// <summary>
            /// Whether or not to ignore players with Remote Admin access.
            /// </summary>
            public bool IgnoreStaff;

            /// <summary>
            /// Whether or not to ignore players in Tutorial.
            /// </summary>
            public bool IgnoreTutorial;

            /// <summary>
            /// Whether or not to ignore Northwood staff.
            /// </summary>
            public bool IgnoreNorthwoodStaff;

            /// <summary>
            /// List of permission groups the player needs.
            /// </summary>
            public string[]? Groups;

            /// <summary>
            /// List of effects the player needs to have active.
            /// </summary>
            public string[]? Effects;

            /// <summary>
            /// A list of teams the player can be in.
            /// </summary>
            public Team[]? Teams;

            /// <summary>
            /// A list of factions the player can be in.
            /// </summary>
            public Faction[]? Factions;

            /// <summary>
            /// A list of roles the player can have.
            /// </summary>
            public RoleTypeId[]? Roles;

            /// <summary>
            /// A list of facility zones the player can be in.
            /// </summary>
            public FacilityZone[]? Zones;

            /// <summary>
            /// A list of rooms the player can be in.
            /// </summary>
            public RoomName[]? Rooms;

            /// <summary>
            /// Determines whether the specified player satisfies the filters defined within the structure.
            /// </summary>
            /// <param name="player">The player to evaluate against the defined filters.</param>
            /// <returns><c>true</c> if the player satisfies all the conditions of the filters; otherwise, <c>false</c>.</returns>
            public bool IsValid(ExPlayer player)
            {
                if (player?.ReferenceHub == null) return false;

                if (IgnoreNorthwoodStaff && player.IsNorthwoodStaff) return false;
                if (IgnoreTutorial && player.Role.IsTutorial) return false;
                if (IgnoreStaff && player.HasRemoteAdminAccess) return false;
                
                if (IsAlive.HasValue && player.Role.IsAlive != IsAlive.Value) return false;
                if (IsGrounded.HasValue && IsGrounded.Value != player.Position.IsGrounded) return false;
                
                if (Groups?.Length > 0 && (string.IsNullOrEmpty(player.PermissionsGroupName) || !Groups.Contains(player.PermissionsGroupName))) return false;
                if (Teams?.Length > 0 && !Teams.Contains(player.Role.Team)) return false;
                if (Roles?.Length > 0 && !Roles.Contains(player.Role.Type)) return false;
                if (Factions?.Length > 0 && !Factions.Contains(player.Role.Faction)) return false;
                if (Effects?.Length > 0 && !Effects.All(e => player.Effects.IsActive(e, true))) return false;
                if (Zones?.Length > 0 && !Zones.Contains(player.Position.Position.GetZone())) return false;
                if (Rooms?.Length > 0 && !Rooms.Contains(player.Position.Room?.Name ?? RoomName.Unnamed)) return false;
                
                return true;
            }
            
            // $RandomPlayer = GetRandomPlayer "IsAlive,IgnoreNwStaff,Team:SCP NTF" 
            public static bool TryParse(string str, out PlayerFilters filters)
            {
                if (string.IsNullOrWhiteSpace(str))
                {
                    filters = Default;
                    return true;
                }

                var list = str.SplitOutsideQuotes(',');

                if (list?.Length < 1)
                {
                    filters = Default;
                    return true;
                }

                filters = new();

                for (var x = 0; x < list.Length; x++)
                {
                    var part = list[x].Trim();

                    if (string.IsNullOrWhiteSpace(part))
                        continue;

                    var pair = part.SplitEscaped(':');

                    if (pair != null && pair.Length == 2)
                    {
                        var key = pair[0].Trim();
                        var value = pair[1].Trim();

                        switch (key.ToUpperInvariant())
                        {
                            case "ISALIVE":
                            {
                                if (!bool.TryParse(value, out var isAlive)) 
                                    throw new($"Could not parse IsAlive value '{value}' to BOOLEAN");
                                
                                filters.IsAlive = isAlive;
                                break;
                            }
                            
                            case "ISGROUNDED":
                            {
                                if (!bool.TryParse(value, out var isGrounded)) 
                                    throw new($"Could not parse IsGrounded value '{value}' to BOOLEAN");
                                
                                filters.IsGrounded = isGrounded;
                                break;
                            }

                            case "GROUPS":
                            {
                                filters.Groups = value.SplitEscaped(' ');
                                break;
                            }
                            
                            case "EFFECTS":
                            {
                                filters.Effects = value.SplitEscaped(' ');
                                break;
                            }

                            case "TEAMS":
                            {
                                if (!value.TryParseEnumArray<Team>(out var teams)) 
                                    throw new($"Could not parse Teams value '{value}'");
                                
                                filters.Teams = teams;
                                break;
                            }
                            
                            case "ROLES":
                            {
                                if (!value.TryParseEnumArray<RoleTypeId>(out var roles))
                                    throw new($"Could not parse Roles value '{value}'");
                                
                                filters.Roles = roles;
                                break;
                            }
                            
                            case "FACTIONS":
                            {
                                if (!value.TryParseEnumArray<Faction>(out var factions)) 
                                    throw new($"Could not parse Factions value '{value}'");
                                
                                filters.Factions = factions;
                                break;
                            }
                            
                            case "ZONES":
                            {
                                if (!value.TryParseEnumArray<FacilityZone>(out var zones)) 
                                    throw new($"Could not parse Zones value '{value}'");
                                
                                filters.Zones = zones;
                                break;
                            }

                            case "ROOMSBL" or "ROOMSWH":
                            {
                                if (!value.TryParseEnumArray<RoomName>(out var rooms)) 
                                    throw new($"Could not parse Rooms value '{value}'");
                                
                                filters.Rooms = value == "ROOMSBL"
                                    ? EnumUtils<RoomName>.Values.Except(rooms).ToArray()
                                    : rooms;
                                
                                break;
                            }
                        }
                    }
                    else
                    {
                        switch (part.ToUpperInvariant())
                        {
                            case "ISALIVE":
                                filters.IsAlive = true;
                                break;
                            
                            case "ISGROUNDED":
                                filters.IsGrounded = true;
                                break;
                            
                            case "IGNORESTAFF":
                                filters.IgnoreStaff = true;
                                break;
                            
                            case "IGNORETUTORIAL":
                                filters.IgnoreTutorial = true;
                                break;
                            
                            case "IGNORENWSTAFF":
                                filters.IgnoreNorthwoodStaff = true;
                                break;
                        }
                    }
                }

                return true;
            }
        }

        /// <summary>
        /// Selects a random player that matches the specified filter.
        /// </summary>
        /// <param name="context">The action context that contains necessary data and metadata for the operation.</param>
        /// <returns>An <see cref="ActionResultFlags"/> value indicating the result of the operation. It can reflect success, stopping, or disposal states.</returns>
        /// <exception cref="Exception">Thrown when filters cannot be parsed from the input string.</exception>
        [Action("GetRandomPlayer", "Selects a random player matching a filter.")]
        [ActionParameter("Filter", "The filter to apply to the player.")]
        public static ActionResultFlags GetRandomPlayer(ref ActionContext context)
        {
            context.EnsureCompiled((_, p) => p.EnsureCompiled(string.Empty));
            
            var input = context.GetValue(0);
            
            var filters = context.GetMetadata("Filters", () =>
            {
                if (PlayerFilters.TryParse(input, out var result))
                    return result;

                throw new Exception($"Could not parse player filters from string '{input}'");
            });

            var player = ExPlayer.Players.GetRandomItem(p => filters.IsValid(p));

            if (player?.ReferenceHub != null)
            {
                context.SetMemory(player);
                return ActionResultFlags.SuccessDispose;
            }

            return ActionResultFlags.StopDispose;
        }

        /// <summary>
        /// Selects a specified number of random players that match a given filter.
        /// </summary>
        /// <param name="context">The action context containing the parameters and state for execution.</param>
        /// <returns><c>ActionResultFlags.SuccessDispose</c> if the operation completes successfully; otherwise, <c>ActionResultFlags.StopDispose</c> if the selection fails.</returns>
        /// <exception cref="Exception">Thrown when an error occurs during the player selection or evaluation of filters.</exception>
        [Action("GetRandomPlayers", "Selects a list of random players matching a filter.")]
        [ActionParameter("Count", "The amount of players to select.")]
        [ActionParameter("Filter", "The filter to apply.")]
        public static ActionResultFlags GetRandomPlayers(ref ActionContext context)
        {
            context.EnsureCompiled((i, p) =>
            {
                return i switch
                {
                    0 => p.EnsureCompiled(int.TryParse, 1),
                    1 => p.EnsureCompiled(string.Empty),
                    
                    _ => false
                };
            });

            var count = context.GetValue<int>(0);
            var input = context.GetValue(1);
            
            var filters = context.GetMetadata("Filters", () =>
            {
                if (PlayerFilters.TryParse(input, out var result))
                    return result;

                throw new Exception($"Could not parse player filters from string '{input}'");
            });

            if (ExPlayer.Count < count)
                return ActionResultFlags.StopDispose;

            var list = new List<ExPlayer>(count);

            // kinda expensive ik
            while (list.Count < count && ExPlayer.Players.Any(p => 
                       p?.ReferenceHub != null 
                       && !list.Contains(p)
                       && filters.IsValid(p)))
            {
                var random = ExPlayer.Players.GetRandomItem(p => filters.IsValid(p));
                
                if (random?.ReferenceHub == null)
                    continue;
                
                if (list.Contains(random))
                    continue;
                
                list.Add(random);
            }

            if (list.Count < count)
            {
                list.Clear();
                return ActionResultFlags.StopDispose;
            }

            context.SetMemory(list);
            return ActionResultFlags.SuccessDispose;
        }

        /// <summary>
        /// Selects a random status effect based on the specified parameters and classifications.
        /// </summary>
        /// <param name="context">The action context containing the player and parameters for the effect selection.</param>
        /// <returns>A flag indicating the result of the action execution.</returns>
        [Action("GetRandomEffect", "Selects a random status effect.")]
        [ActionParameter("Classification", "A list of valid classifications.")]
        [ActionParameter("OnlyActive", "Whether or not to select only from effects that are currently active.")]
        [ActionParameter("OnlyInactive", "Whether or not to select only from effects that are currently inactive.")]
        public static ActionResultFlags GetRandomEffect(ref ActionContext context)
        {
            context.EnsureCompiled((i, p) =>
            {
                return i switch
                {
                    0 => p.EnsureCompiled(StringExtensions.TryParseEnumArray, new StatusEffectBase.EffectClassification[] { }),
                    1 => p.EnsureCompiled(bool.TryParse, false),
                    2 => p.EnsureCompiled(bool.TryParse, false),
                    
                    _ => false
                };
            });

            if (context.Player?.ReferenceHub == null)
                return ActionResultFlags.StopDispose;
            
            var classifications = context.GetValue<StatusEffectBase.EffectClassification[]>(0);
            
            var onlyActive = context.GetValue<bool>(1);
            var onlyInactive = context.GetValue<bool>(2);

            var effects = onlyActive
                ? context.Player.Effects.ActiveEffects
                : context.Player.Effects.Effects.Values;

            if (onlyInactive && !onlyActive)
                effects = effects.Where(e => !e.IsEnabled);
            
            effects = effects.Where(e => classifications?.Length < 1 || classifications.Contains(e.Classification));

            if (effects.Any())
            {
                context.SetMemory(effects.GetRandomItem());
                return ActionResultFlags.SuccessDispose;
            }

            return ActionResultFlags.StopDispose;
        }
        
        /// <summary>
        /// Selects a list of random status effects based on the specified parameters and classifications.
        /// </summary>
        /// <param name="context">The action context containing the player and parameters for the effect selection.</param>
        /// <returns>A flag indicating the result of the action execution.</returns>
        [Action("GetRandomEffects", "Selects a list of random status effects.")]
        [ActionParameter("Amount", "The amount of effects to select.")]
        [ActionParameter("Classification", "A list of valid classifications.")]
        [ActionParameter("OnlyActive", "Whether or not to select only from effects that are currently active.")]
        [ActionParameter("OnlyInactive", "Whether or not to select only from effects that are currently inactive.")]
        public static ActionResultFlags GetRandomEffects(ref ActionContext context)
        {
            context.EnsureCompiled((i, p) =>
            {
                return i switch
                {
                    0 => p.EnsureCompiled(int.TryParse, 1),
                    1 => p.EnsureCompiled(StringExtensions.TryParseEnumArray, new StatusEffectBase.EffectClassification[] { }),
                    2 => p.EnsureCompiled(bool.TryParse, false),
                    3 => p.EnsureCompiled(bool.TryParse, false),
                    
                    _ => false
                };
            });

            if (context.Player?.ReferenceHub == null)
                return ActionResultFlags.StopDispose;

            var amount = context.GetValue<int>(0);
            var classifications = context.GetValue<StatusEffectBase.EffectClassification[]>(1);
            
            var onlyActive = context.GetValue<bool>(2);
            var onlyInactive = context.GetValue<bool>(3);

            var effects = onlyActive
                ? context.Player.Effects.ActiveEffects
                : context.Player.Effects.Effects.Values;

            if (onlyInactive && !onlyActive)
                effects = effects.Where(e => !e.IsEnabled);
            
            effects = effects.Where(e => classifications?.Length < 1 || classifications.Contains(e.Classification));

            if (effects.Any())
            {
                var list = new List<StatusEffectBase>();

                if (amount < 0)
                {
                    list.AddRange(effects);
                }
                else
                {
                    while (list.Count < amount && effects.Any(e => !list.Contains(e)))
                    {
                        var random = effects.GetRandomItem();

                        if (list.Contains(random))
                            continue;

                        list.Add(random);
                    }

                    if (list.Count < amount)
                    {
                        list.Clear();
                        return ActionResultFlags.StopDispose;
                    }
                }

                context.SetMemory(list);
                return ActionResultFlags.SuccessDispose;
            }

            return ActionResultFlags.StopDispose;
        }

        /// <summary>
        /// Selects a list of status effects based on the specified parameters and classifications.
        /// </summary>
        /// <param name="context">The action context containing the player and parameters for the effect selection.</param>
        /// <returns>A flag indicating the result of the action execution.</returns>
        [Action("GetEffects", "Selects a list of status effects.")]
        [ActionParameter("Classification", "A list of valid classifications.")]
        [ActionParameter("OnlyActive", "Whether or not to select only from effects that are currently active.")]
        [ActionParameter("OnlyInactive", "Whether or not to select only from effects that are currently inactive.")]
        public static ActionResultFlags GetEffects(ref ActionContext context)
        {
            context.EnsureCompiled((i, p) =>
            {
                return i switch
                {
                    0 => p.EnsureCompiled(StringExtensions.TryParseEnumArray,
                        new StatusEffectBase.EffectClassification[] { }),
                    1 => p.EnsureCompiled(bool.TryParse, false),
                    2 => p.EnsureCompiled(bool.TryParse, false),

                    _ => false
                };
            });

            if (context.Player?.ReferenceHub == null)
                return ActionResultFlags.StopDispose;

            var classifications = context.GetValue<StatusEffectBase.EffectClassification[]>(0);

            var onlyActive = context.GetValue<bool>(1);
            var onlyInactive = context.GetValue<bool>(2);

            var effects = onlyActive
                ? context.Player.Effects.ActiveEffects
                : context.Player.Effects.Effects.Values;

            if (onlyInactive && !onlyActive)
                effects = effects.Where(e => !e.IsEnabled);

            effects = effects.Where(e => classifications?.Length < 1 || classifications.Contains(e.Classification));

            context.SetMemory(effects.ToList());
            return ActionResultFlags.SuccessDispose;
        }

        /// <summary>
        /// Retrieves a player's effect by its name and stores it in the provided action context.
        /// </summary>
        /// <param name="context">The action context that contains player details and where the effect will be stored.</param>
        /// <returns>An <c>ActionResultFlags</c> value indicating the outcome of the operation, such as success, stop, or disposal statuses.</returns>
        [Action("GetEffect", "Get's a player effect by name.")]
        [ActionParameter("Name", "Name of the effect to get.")]
        public static ActionResultFlags GetEffect(ref ActionContext context)
        {
            context.EnsureCompiled((_, p) => p.EnsureCompiled(string.Empty));

            if (context.Player?.ReferenceHub == null)
                return ActionResultFlags.StopDispose;

            var name = context.GetValue(0);

            if (!context.Player.Effects.TryGetEffect(name, true, out var effect))
                return ActionResultFlags.StopDispose;

            context.SetMemory(effect);
            return ActionResultFlags.SuccessDispose;
        }
    }
}