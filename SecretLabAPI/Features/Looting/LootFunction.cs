using LabExtended.API;

namespace SecretLabAPI.Features.Looting;

/// <summary>
/// Represents a function that can be used to loot a player.
/// </summary>
public delegate void LootFunction(ExPlayer target, string[] arguments);