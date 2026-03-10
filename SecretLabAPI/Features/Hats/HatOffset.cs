using LabExtended.API;

using PlayerRoles;

using UnityEngine;

namespace SecretLabAPI.Features.Hats;

// Offsets taken from https://github.com/creepycats/SLCosmetics/blob/767bffa9b6de24b532b4850ba4fc0d75fd2ea882/Types/Hats/HatComponent.cs#L34

/// <summary>
/// Represents the offset data for positioning and rotating a hat or accessory on a character model in relation to their head transform.
/// </summary>
public struct HatOffset
{
    /// <summary>
    /// The default hat offset applied when no specific offset is defined for a given role.
    /// </summary>
    public static readonly HatOffset DefaultOffset = new(null, new(0, 0.20f, -0.03f), null);

    /// <summary>
    /// A read-only dictionary that maps role types to their respective hat offsets.
    /// </summary>
    public static IReadOnlyDictionary<RoleTypeId, HatOffset> Offsets { get; } = new Dictionary<RoleTypeId, HatOffset>()
    {
        { RoleTypeId.Scp106, 
            new("armature/Root/HipsCTRL/Pelvis/Stomach/LowerChest/UpperChest/neck/Head", 
                new(0f, 0.20f, -0.02f), 
                new(0f, 180f, 0f)) },
      
        { RoleTypeId.Scp096, 
            new("SCP-096/root/Hips/Spine01/Spine02/Spine03/Neck01/Neck02/head",
                new(0f, 0.125f, -0.025f),
                null) },
        
        { RoleTypeId.Scp939, 
            new("Anims/939Rig/HipControl/DEF-Hips/DEF-Stomach/DEF-Chest/DEF-Neck/DEF-Head",
                new(0f, 0.1f, 0.025f),
                null) },
        
        { RoleTypeId.Scp173, new(null, new(0f, 0.55f, -0.05f), null) },
        { RoleTypeId.Scp049, new(null, new(0f, 0.125f, -0.05f), null) },
        { RoleTypeId.Scp0492, new(null, new(0f, 0f, -0.1f), null) },
    };

    /// <summary>
    /// Retrieves the hat offset associated with the specified role.
    /// </summary>
    /// <param name="role">The role for which the hat offset is being requested.</param>
    /// <returns>The corresponding hat offset for the given role. If no specific offset is defined, the default hat offset is returned.</returns>
    public static HatOffset GetOffset(RoleTypeId role)
        => Offsets.TryGetValue(role, out var offset) ? offset : DefaultOffset;
    
    /// <summary>
    /// The name of the head transform in the character model.
    /// </summary>
    public readonly string? Head;
    
    /// <summary>
    /// The position offset from the head transform.
    /// </summary>
    public readonly Vector3? Position;

    /// <summary>
    /// The rotation offset from the head transform.
    /// </summary>
    public readonly Vector3? Rotation;

    /// <summary>
    /// Creates a new HatOffset instance with the specified head, position, and rotation.
    /// </summary>
    /// <param name="head">The name of the head transform in the character model.</param>
    /// <param name="position">The position offset from the head transform.</param>
    /// <param name="rotation">The rotation offset from the head transform.</param>
    public HatOffset(string? head, Vector3? position, Vector3? rotation)
    {
        Head = head;
        Position = position;
        Rotation = rotation;
    }

    /// <summary>
    /// Retrieves the head transform of the specified player. If the head transform is not explicitly defined,
    /// it falls back to the player's camera transform.
    /// </summary>
    /// <param name="player">The player whose head transform is being retrieved. Cannot be null.</param>
    /// <returns>
    /// The transform representing the player's head, or null if the player's movement module or character model
    /// instance is unavailable.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when the provided player is null.</exception>
    public Transform? GetHead(ExPlayer player)
    {
        if (player == null)
            throw new ArgumentNullException(nameof(player));

        if (player.Role.MovementModule == null)
            return null;
        
        var model = player.Role.MovementModule.CharacterModelInstance;
        
        if (model == null)
            return null;

        if (Head != null)
        {
            var head = model.gameObject.transform.Find(Head);
            
            if (head != null)
                return head;
        }

        var mixamorigHead = model.Hitboxes.FirstOrDefault(x => x.name.ToLower().Contains("mixamorig:head"));
        return mixamorigHead?.transform ?? player.CameraTransform;
    }

    /// <summary>
    /// Calculates the position and rotation offsets for a hat relative to a player's head transform.
    /// </summary>
    /// <param name="player">The player for whom the offsets are being calculated. Cannot be null.</param>
    /// <param name="head">The head transform of the player, which serves as the reference point for the offsets. Cannot be null.</param>
    /// <param name="position">Outputs the calculated position offset relative to the head transform.</param>
    /// <param name="rotation">Outputs the calculated rotation offset relative to the head transform.</param>
    /// <exception cref="ArgumentNullException">Thrown when the provided player or head transform is null.</exception>
    public void GetOffsets(ExPlayer player, Transform head, out Vector3 position, out Quaternion rotation)
    {
        if (player == null)
            throw new ArgumentNullException(nameof(player));

        var euler = head.rotation.eulerAngles;

        if (player.Role.IsScp)
            euler.x = 0f;
        
        rotation = Rotation != null
            ? Quaternion.Euler(euler) * Quaternion.Euler(Rotation.Value)
            : Quaternion.Euler(euler);

        position = Position != null
            ? rotation * Position.Value + head.position
            : rotation * Vector3.one + head.position;
    }
}