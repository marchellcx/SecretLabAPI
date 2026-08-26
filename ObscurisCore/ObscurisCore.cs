using System.Reflection;

using LabApi.Features.Wrappers;

using LabApi.Loader;
using LabApi.Loader.Features.Plugins;

using LabExtended.Core;
using LabExtended.Attributes;
using LabExtended.Utilities;
using NiveraAPI.Extensions;

using NiveraAPI.IO.Storage;
using NiveraAPI.IO.Serialization;

using ObscurisCore.Utilities.Storage;

namespace ObscurisCore;

/// <summary>
/// The main class of this library.
/// </summary>
[LoaderPatch]
public class ObscurisPlugin : Plugin
{
    private static Dictionary<Type, StorageSerializer> serializers = new();
    private static Dictionary<FieldInfo, StorageAttribute> storages = new();
    
    /// <summary>
    /// Gets an instance of this plugin.
    /// </summary>
    public static ObscurisPlugin Plugin { get; private set; }
    
    /// <summary>
    /// Gets the storage manager used to save data for this server only.
    /// </summary>
    public static StorageManager ServerStorage { get; private set; }
    
    /// <summary>
    /// Gets the storage manager used to save data shared between servers.
    /// </summary>
    public static StorageManager SharedStorage { get; private set; }

    /// <summary>
    /// Gets the root directory path of SecretLabAPI's global config.
    /// </summary>
    public static string RootDirectory { get; private set; }

    /// <inheritdoc/>
    public override string Name { get; } = "ObscurisCore";

    /// <inheritdoc/>
    public override string Author { get; } = "marchellcx";

    /// <inheritdoc/>
    public override string Description { get; } = "A plugin that contains many utilities and functions.";

    /// <inheritdoc/>
    public override Version Version { get; } = new(2, 1, 0);

    /// <inheritdoc/>
    public override Version RequiredApiVersion { get; } = null!;

    /// <inheritdoc/>
    public override void Enable()
    {
        try
        {
            Plugin = this;
            
            RootDirectory = Plugin.GetConfigDirectory(true).FullName;
            
            ByteSerializer<string[]>.Serialize = (writer, roles) => writer.WriteArray(roles);
            ByteSerializer<string[]>.Deserialize = reader => reader.ReadArray<string>();
            
            FindStorages();
            
            ServerStorage = new(FileUtils.CreatePath(RootDirectory, $"Storage_{Server.Port}"));
            ServerStorage.DefaultSerializer = new JsonSerializer();
            
            SharedStorage = new(FileUtils.CreatePath(RootDirectory, "Storage_Shared"));
            SharedStorage.DefaultSerializer = new JsonSerializer();
            
            AssignSerializers();
            
            ServerStorage.Initialize();
            SharedStorage.Initialize();
            
            AssignStorages();
            
            InitLoaders();
        }
        catch (Exception ex)
        {
            ApiLog.Error("SecretLab", $"Failed to enable SecretLabAPI!:\n{ex}");
        }
    }

    /// <inheritdoc/>
    public override void Disable()
    {
        
    }
    
    private static void AssignSerializers()
    {
        foreach (var kvp in storages)
        {
            try
            {
                if (kvp.Value.Serializer == null)
                    continue;
                
                if (string.IsNullOrEmpty(kvp.Value.Name))
                    continue;
                
                if (kvp.Value.Serializer != null)
                {
                    if (!serializers.TryGetValue(kvp.Value.Serializer, out var serializer))
                    {
                        if ((serializer = Activator.CreateInstance(kvp.Value.Serializer) as StorageSerializer) == null)
                        {
                            ApiLog.Error($"Failed to create serializer for &1{kvp.Value.Serializer.Name}&r");
                            
                            storages.Remove(kvp.Key);
                            continue;
                        }
                        
                        serializers.Add(kvp.Value.Serializer, serializer);
                    }
                    
                    ApiLog.Debug($"Assigned serializer &1{serializer.GetType()}&r to &1{kvp.Key.Name}&r");

                    (kvp.Value.IsShared 
                        ? SharedStorage 
                        : ServerStorage)
                        .DirectorySerializers.TryAdd(kvp.Value.Name, serializer);
                }
            }
            catch (Exception ex)
            {
                ApiLog.Error(ex);
                
                storages.Remove(kvp.Key);
            }
        }
    }
    
    private static void AssignStorages()
    {
        foreach (var kvp in storages)
        {
            try
            {
                if (string.IsNullOrEmpty(kvp.Value.Name))
                    continue;

                var dir = (kvp.Value.IsShared 
                    ? SharedStorage 
                    : ServerStorage)
                    .Add(kvp.Value.Name);

                if (dir != null)
                {
                    kvp.Key.SetValue(null, dir);
                    
                    ApiLog.Debug($"Assigned storage &1{kvp.Value.Name}&r to &1{kvp.Key.Name}&r");
                }
                else
                {
                    ApiLog.Error($"Failed to assign storage &1{kvp.Value.Name}&r to &1{kvp.Key.Name}&r");
                }
                
                if (kvp.Key.DeclaringType != null)
                {
                    var initName = string.Concat("StorageInit_", kvp.Key.Name);
                    var initMethod = kvp.Key.DeclaringType.FindMethod(m => 
                        m.Name == initName 
                        && m.IsStatic
                        && m.ReturnType == typeof(void)
                        && m.GetAllParameters().Length == 0);
                    
                    initMethod?.Invoke(null, null);
                }
            }
            catch (Exception ex)
            {
                ApiLog.Error(ex);
            }
        }
    }
    
    private static void FindStorages()
    {
        var asm = Assembly.GetExecutingAssembly();
        var types = asm.GetTypes();
        
        foreach (var type in types)
        {
            var fields = type.GetAllFields();

            foreach (var field in fields)
            {
                if (!field.HasAttribute<StorageAttribute>(out var dbStorageAttribute))
                    continue;

                if (field.IsInitOnly || field.FieldType != typeof(StorageDirectory))
                {
                    ApiLog.Warn($"Field &1{field.Name}&r is not a &1StorageDirectory&r!");
                    continue;
                }
                
                if (string.IsNullOrEmpty(dbStorageAttribute.Name))
                    dbStorageAttribute.Name = field.Name;
                
                storages.Add(field, dbStorageAttribute);
                
                ApiLog.Debug($"Found storage field &1{field.Name}&r: &3{dbStorageAttribute.Name}&r");
            }
        }
    }

    private static void InitLoaders()
    {
        foreach (var type in typeof(ObscurisPlugin).Assembly.GetTypes())
        {
            foreach (var method in type.GetAllMethods())
            {
                if (!method.IsPrivate || method.ReturnType != typeof(void) || !method.IsStatic
                    || method.Name != "Initialize" || method.GetParameters().Length != 0)
                    continue;

                ApiLog.Debug("SecretLab", $"Initializing &1{type.Name}&r ...");
                
                try
                {
                    method.Invoke(null, null);
                }
                catch (Exception ex)
                {
                    ApiLog.Error("SecretLab", $"Error while initializing &1{type.Name}&r:\n{ex}");
                }
            }
        }
    }
}