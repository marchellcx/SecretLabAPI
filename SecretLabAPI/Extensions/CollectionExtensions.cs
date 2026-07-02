namespace SecretLabAPI.Extensions;

/// <summary>
/// Extensions for collections.
/// </summary>
public static class CollectionExtensions
{
    /// <summary>
    /// Safely sets a value in the dictionary by applying a transformation function to the
    /// existing value if the key exists, or to the specified default value if the key does not exist.
    /// </summary>
    /// <typeparam name="TKey">The type of the dictionary keys.</typeparam>
    /// <typeparam name="TValue">The type of the dictionary values.</typeparam>
    /// <param name="dict">The dictionary to perform the operation on.</param>
    /// <param name="key">The key for the value to be set or updated.</param>
    /// <param name="defaultValue">The default value to use if the key does not exist in the dictionary.</param>
    /// <param name="newValue">A function that takes the current value (or default value) and returns the new value to set.</param>
    /// <exception cref="ArgumentNullException">Thrown if the dictionary or key is null.</exception>
    public static void SetValueSafe<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue? defaultValue,
        Func<TValue, TValue> newValue)
    {
        if (dict == null)
            throw new ArgumentNullException(nameof(dict));

        if (key == null)
            throw new ArgumentNullException(nameof(key));

        if (dict.TryGetValue(key, out var value))
        {
            dict[key] = newValue(value);
        }
        else
        {
            dict[key] = newValue(defaultValue);
        }
    }
}