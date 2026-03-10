using LabExtended.API.Custom.Items;

namespace SecretLabAPI.Features.Items;

/// <summary>
/// Represents a custom cola item that has no effect.
/// </summary>
public class NoEffectCola : CustomItem
{
    private ItemType colaType;

    /// <inheritdoc/>
    public override string Id { get; }

    /// <inheritdoc/>
    public override string Name { get; }

    /// <inheritdoc/>
    public override ItemType PickupType
    {
        get => colaType;
        set => colaType = value;
    }

    /// <inheritdoc/>
    public override ItemType InventoryType
    {
        get => colaType;
        set => colaType = value;
    }
    
    /// <summary>
    /// Creates a new NoEffectCola.
    /// </summary>
    /// <param name="colaType">The type of cola to create.</param>
    /// <exception cref="ArgumentException">Thrown if the cola type is invalid.</exception>
    public NoEffectCola(ItemType colaType)
    {
        if (colaType is not ItemType.SCP207 and not ItemType.AntiSCP207)
            throw new ArgumentException("Invalid cola type", nameof(colaType));
        
        this.colaType = colaType;

        Id = colaType == ItemType.SCP207 ? "noeffect_cola" : "noeffect_anti_cola";
        Name = colaType == ItemType.SCP207 ? "No Effect Cola" : "No Effect Anti-Cola";
    }
}