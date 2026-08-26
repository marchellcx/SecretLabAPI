namespace ObscurisCore.Utilities.Storage;

/// <summary>
/// Represents an attribute used to specify database storage metadata for a field.
/// </summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public class StorageAttribute : Attribute
{
    /// <summary>
    /// The name of the field in the database.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Whether the storage is shared across all servers.
    /// </summary>
    public bool IsShared { get; set; } 
    
    /// <summary>
    /// The type of the serializer.
    /// </summary>
    public Type? Serializer { get; }

    /// <summary>
    /// An attribute used to specify database storage metadata for a field.
    /// This can include the storage name and the serializer type to be used
    /// when persisting or retrieving the field's data.
    /// </summary>
    public StorageAttribute(string? name = null, bool shared = true, Type? serializer = null)
    {
        Name = name;
        IsShared = shared;
        Serializer = serializer;
    }
}