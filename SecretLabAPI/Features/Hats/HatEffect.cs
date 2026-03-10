using LabExtended.API;
using LabExtended.API.Custom.Effects;

using LabExtended.Core;
using Mirror;
using PlayerRoles;

using ProjectMER.Features;
using ProjectMER.Features.Objects;

using UnityEngine;

namespace SecretLabAPI.Features.Hats;

/// <summary>
/// Represents a custom ticking effect that applies a hat schematic to a player.
/// </summary>
public class HatEffect : CustomTickingEffect
{
    /// <summary>
    /// Gets the currently spawned hat schematic.
    /// </summary>
    public SchematicObject? Hat { get; private set; }

    /// <summary>
    /// Sets a hat schematic on the targeted player.
    /// </summary>
    /// <param name="schematicName">The name of the schematic to be set as the hat.</param>
    /// <returns>True if the hat is successfully set, otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="schematicName"/> is null or empty.</exception>
    public bool SetHat(string schematicName)
    {
        if (string.IsNullOrEmpty(schematicName))
            throw new ArgumentNullException(nameof(schematicName));

        if (!ObjectSpawner.TrySpawnSchematic(schematicName, Vector3.zero, Quaternion.identity, Vector3.one,
                out var hat))
        {
            ApiLog.Error($"Failed to spawn hat schematic '{schematicName}'");
            return false;
        }

        return SetHat(hat);
    }
    
    /// <summary>
    /// Sets a custom hat schematic on the targeted player.
    /// </summary>
    /// <param name="hat">The schematic object representing the custom hat.</param>
    /// <returns>True if the hat is successfully set, otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="hat"/> is null.</exception>
    public bool SetHat(SchematicObject hat)
    {
        if (hat == null)
            throw new ArgumentNullException(nameof(hat));

        if (Player?.ReferenceHub == null)
        {
            ApiLog.Error("Cannot set hat: null player");
            return false;
        }
        
        if (Hat != null)
            Hat.Destroy();
        
        foreach (var identity in hat.NetworkIdentities)
            Player.Connection.Send(new ObjectHideMessage() { netId = identity.netId });
        
        Hat = hat;
        return true;
    }

    /// <summary>
    /// Removes the currently applied hat schematic from the targeted player.
    /// </summary>
    public void RemoveHat()
        => RemoveEffects();
    
#region Overrides of CustomTickingEffect
    /// <summary>
    /// Removes the currently applied hat schematic from the targeted player.
    /// </summary>
    public override void RemoveEffects()
    {
        if (Hat != null)
            Hat.Destroy();
        
        Hat = null;
        
        base.RemoveEffects();
    }

    /// <summary>
    /// Updates the position and rotation of the hat schematic based on the player's role and position.'
    /// </summary>
    public override void Tick()
    {
        base.Tick();

        if (Hat != null && Player?.ReferenceHub != null)
        {
            if (!Player.IsAlive)
            {
                RemoveEffects();
                return;
            }

            Hat.Position = Player.CameraTransform.position;
            Hat.Rotation = Player.CameraTransform.rotation;
        }
    }
#endregion
}