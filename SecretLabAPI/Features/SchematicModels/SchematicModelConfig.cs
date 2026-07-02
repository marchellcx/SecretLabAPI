using System.ComponentModel;

using LabExtended.Core.Configs.Objects;

using UnityEngine;

namespace SecretLabAPI.Features.SchematicModels;

/// <summary>
/// Represents the configuration for a schematic model, allowing customization
/// of its position and rotation relative to the player model. This class is
/// commonly used to define offsets for schematic models in a virtual environment.
/// </summary>
public class SchematicModelConfig
{
    /// <summary>
    /// Gets or sets the scale of the schematic model.
    /// </summary>
    [Description("Sets the scale of the schematic.")]
    public YamlVector3 Scale { get; set; } = new(0f);
    
    /// <summary>
    /// Gets or sets the offset of the schematic position relative to the player's model.
    /// </summary>
    [Description("Sets the offset of the schematic position relative to the player's model.")]
    public YamlVector3 PositionOffset { get; set; } = new(0f, 0f, 0f);

    /// <summary>
    /// Gets or sets the rotation of the schematic position relative to the player's model.
    /// </summary>
    [Description("Sets the rotation of the schematic position relative to the player's model.")]
    public YamlQuaternion RotationOffset { get; set; } = new(Quaternion.identity);
}