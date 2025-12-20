using LabExtended.API;
using LabExtended.Extensions;

using LabExtended.Core.Pooling.Pools;

using NorthwoodLib.Pools;

using SecretLabAPI.Extensions;

namespace SecretLabAPI.Actions.API
{
    /// <summary>
    /// Provides contextual information and state management for a sequence of actions, including tracking the current,
    /// previous, and next actions, targeted players, and temporary data.
    /// </summary>
    public struct ActionContext : IDisposable
    {
        /// <summary>
        /// The index of the iterator.
        /// </summary>
        public int IteratorIndex;

        /// <summary>
        /// The current action being processed.
        /// </summary>
        public CompiledAction Current;

        /// <summary>
        /// The next action to be performed.
        /// </summary>
        public CompiledAction? Next;

        /// <summary>
        /// The previous action that was performed.
        /// </summary>
        public CompiledAction? Previous;

        /// <summary>
        /// The player targeted by the action.
        /// </summary>
        public ExPlayer Player;

        /// <summary>
        /// The list of actions to be performed.
        /// </summary>
        public List<CompiledAction> Actions;

        /// <summary>
        /// A dictionary to store temporary data between actions.
        /// </summary>
        public Dictionary<string, object> Memory;

        /// <summary>
        /// Initializes a new instance of the ActionContext class with the specified list of actions.
        /// </summary>
        /// <param name="actions">The list of actions to be managed by this context. Cannot be null.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="actions"/> is null.</exception>
        public ActionContext(List<CompiledAction> actions, ExPlayer? player = null)
        {
            if (actions is null)
                throw new ArgumentNullException(nameof(actions));

            Player = player;
            Actions = actions;

            Next = null;
            Previous = null;

            Current = null!;

            Memory = DictionaryPool<string, object>.Shared.Rent();
        }

        /// <summary>
        /// Releases all resources used by the instance.
        /// </summary>
        public void Dispose() 
        { 
            if (Memory != null)
                DictionaryPool<string, object>.Shared.Return(Memory);

            Memory = null!;
        }

        /// <summary>
        /// Retrieves the value of a memory variable and casts it to the specified type.
        /// </summary>
        /// <typeparam name="T">The type to which the memory variable should be cast.</typeparam>
        /// <param name="variable">The name of the memory variable to retrieve. Cannot be null or empty.</param>
        /// <returns>The value of the specified memory variable cast to type <typeparamref name="T"/>.</returns>
        /// <exception cref="Exception">Thrown if the memory variable does not exist or cannot be cast to type <typeparamref name="T"/>.</exception>
        public T GetMemory<T>(string variable)
        {
            if (!Memory.TryGetValue(variable, out var obj))
                throw new Exception($"Memory variable '{variable}' not found.");

            if (obj is T variableValue)
                return variableValue;

            throw new Exception($"Memory variable '{variable}' is not of type {typeof(T).FullName}.");
        }

        /// <summary>
        /// Sets the value of a named memory variable.
        /// </summary>
        /// <typeparam name="T">The type of the value to assign to the memory variable.</typeparam>
        /// <param name="variable">The name of the memory variable to set. Cannot be null.</param>
        /// <param name="value">The value to assign to the specified memory variable.</param>
        public void SetMemory<T>(string variable, T value)
        {
            if (string.IsNullOrEmpty(variable))
                throw new ArgumentNullException(nameof(variable));

            Memory[variable] = value!;
        }

        /// <summary>
        /// Sets the specified value in memory if the variable name begins with a '$' character.
        /// </summary>
        /// <remarks>If <paramref name="variable"/> does not begin with a '$', the method does not perform
        /// any operation.</remarks>
        /// <typeparam name="T">The type of the value to store in memory.</typeparam>
        /// <param name="variable">The name of the variable. Must begin with a '$' character to be stored in memory.</param>
        /// <param name="value">The value to assign to the variable in memory.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="variable"/> is null or an empty string.</exception>
        public void SetIfMemory<T>(string variable, T value)
        {
            if (string.IsNullOrEmpty(variable))
                throw new ArgumentNullException(nameof(variable));

            if (variable[0] != '$')
                return;

            Memory[variable.Substring(1)] = value!;
        }

        /// <summary>
        /// Retrieves the source value of the parameter at the specified index within the current action.
        /// </summary>
        /// <param name="index">The zero-based index of the parameter to retrieve. Must be greater than or equal to 0 and less than the
        /// total number of actions.</param>
        /// <returns>A string containing the source value of the parameter at the specified index.</returns>
        /// <exception cref="IndexOutOfRangeException">Thrown when <paramref name="index"/> is less than 0 or greater than or equal to the number of actions.</exception>
        public string GetValue(int index)
        {
            if (index < 0 || index >= Current.Parameters.Length)
                throw new IndexOutOfRangeException($"Parameter index {index} is out of range.");

            return Current.Parameters[index].Source;
        }

        /// <summary>
        /// Retrieves the value of type <typeparamref name="T"/> from the parameter at the specified index in the
        /// current action.
        /// </summary>
        /// <typeparam name="T">The type of the value to retrieve from the parameter.</typeparam>
        /// <param name="index">The zero-based index of the parameter whose value is to be retrieved. Must be within the bounds of the
        /// current action's parameter collection.</param>
        /// <returns>The value of type <typeparamref name="T"/> stored in the parameter at the specified index.</returns>
        /// <exception cref="IndexOutOfRangeException">Thrown when <paramref name="index"/> is less than zero or greater than or equal to the number of parameters
        /// in the current action.</exception>
        public T GetValue<T>(int index)
        {
            if (index < 0 || index >= Current.Parameters.Length)
                throw new IndexOutOfRangeException($"Parameter index {index} is out of range.");

            return Current.Parameters[index].GetValue<T>();
        }

        /// <summary>
        /// Retrieves the value of the specified parameter, identified by key, and returns it as the requested type.
        /// </summary>
        /// <typeparam name="T">The type to which the parameter value will be cast and returned.</typeparam>
        /// <param name="key">The name of the parameter whose value is to be retrieved. Cannot be null or empty.</param>
        /// <returns>The value of the parameter with the specified key, cast to type <typeparamref name="T"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="key"/> is null or empty.</exception>
        /// <exception cref="Exception">Thrown if a parameter with the specified key does not exist.</exception>
        public T GetValue<T>(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            var index = Current.Action.Parameters.FindIndex(p => p.Name == key);

            if (index == -1)
                throw new($"Parameter with key '{key}' not found.");

            return GetValue<T>(index);
        }

        /// <summary>
        /// Retrieves the value associated with the specified parameter key from the current action.
        /// </summary>
        /// <param name="key">The name of the parameter whose value to retrieve. Cannot be null or empty.</param>
        /// <returns>The value of the parameter identified by the specified key.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="key"/> is null or empty.</exception>
        /// <exception cref="Exception">Thrown if a parameter with the specified key does not exist.</exception>
        public string GetValue(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            var index = Current.Action.Parameters.FindIndex(p => p.Name == key);

            if (index == -1)
                throw new($"Parameter with key '{key}' not found.");

            return GetValue(index);
        }

        /// <summary>
        /// Retrieves a value of the specified type associated with the given key, or from memory if the key is prefixed
        /// with a dollar sign ('$').
        /// </summary>
        /// <remarks>If the <paramref name="key"/> starts with a dollar sign ('$'), the method retrieves
        /// the value from memory using the substring after the '$'. Otherwise, it retrieves the value directly using
        /// the provided key.</remarks>
        /// <typeparam name="T">The type of the value to retrieve.</typeparam>
        /// <param name="key">The key that identifies the value to retrieve. If the key begins with '$', the value is retrieved from
        /// memory using the remainder of the key.</param>
        /// <returns>The value of type T associated with the specified key, or from memory if the key is prefixed with '$'.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="key"/> is null or an empty string.</exception>
        public T GetValueOrMemory<T>(string key) 
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            if (key[0] == '$')
                return GetMemory<T>(key.Substring(1));
            
            return GetValue<T>(key);
        }

        /// <summary>
        /// Retrieves a value of type <typeparamref name="T"/> from memory using the specified key, or returns a value
        /// from the provided index if the key is not found.
        /// </summary>
        /// <remarks>If the specified key does not exist in memory, the method falls back to retrieving
        /// the value from the indexed source. The method enforces type safety by throwing an exception if the memory
        /// value does not match the requested type.</remarks>
        /// <typeparam name="T">The type of the value to retrieve from memory or from the indexed source.</typeparam>
        /// <param name="key">The key used to look up the value in memory. Cannot be null or empty.</param>
        /// <param name="index">The index used to retrieve the value if the key is not present in memory.</param>
        /// <returns>The value of type <typeparamref name="T"/> associated with the specified key if found in memory; otherwise,
        /// the value obtained from the specified index.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="key"/> is null or empty.</exception>
        /// <exception cref="Exception">Thrown if the value found in memory for <paramref name="key"/> is not of type <typeparamref name="T"/>.</exception>
        public T GetMemoryOrValue<T>(string key, int index) 
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            if (Memory.TryGetValue(key, out var obj))
            {
                if (obj is T variableValue)
                    return variableValue;

                throw new($"Memory variable '{key}' is not of type {typeof(T).FullName}.");
            }
            
            return GetValue<T>(index);
        }

        /// <summary>
        /// Retrieves the string value associated with the specified key from memory, or returns a value based on the
        /// provided index if the key is not found.
        /// </summary>
        /// <param name="key">The key used to look up the value in memory. Cannot be null or empty.</param>
        /// <param name="index">The index used to retrieve a value if the key is not present in memory.</param>
        /// <returns>The string value associated with the specified key if found; otherwise, the value obtained using the
        /// specified index.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="key"/> is null or empty.</exception>
        /// <exception cref="Exception">Thrown if the memory variable associated with <paramref name="key"/> exists but is not of type <see
        /// cref="String"/>.</exception>
        public string GetMemoryOrValue(string key, int index)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            if (Memory.TryGetValue(key, out var obj))
            {
                if (obj is string variableValue)
                    return variableValue;

                throw new($"Memory variable '{key}' is not of type {typeof(string).FullName}.");
            }
            
            return GetValue(index);
        }

        /// <summary>
        /// Retrieves the value associated with the specified key, or returns a memory-stored value if the key is
        /// prefixed with a dollar sign ('$').
        /// </summary>
        /// <param name="key">The key used to identify the value to retrieve. If the key begins with '$', the method returns a value from
        /// memory using the remainder of the key; otherwise, it returns the standard value associated with the key.
        /// Cannot be null or empty.</param>
        /// <returns>A string containing the value associated with the specified key, or the memory-stored value if the key is
        /// prefixed with '$'.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="key"/> is null or empty.</exception>
        public string GetValueOrMemory(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            if (key[0] == '$')
                return GetMemory<string>(key.Substring(1));
            
            return GetValue(key);
        }

        /// <summary>
        /// Retrieves the metadata value associated with the specified key, or creates and stores a new value using the
        /// provided factory if the key does not exist.
        /// </summary>
        /// <remarks>If the metadata value for the specified key does not exist, the method invokes the
        /// factory to create a new value, stores it, and returns it. Subsequent calls with the same key will return the
        /// stored value. This method is not thread-safe; concurrent access may result in multiple factory invocations
        /// or inconsistent state.</remarks>
        /// <typeparam name="T">The type of the metadata value to retrieve or create.</typeparam>
        /// <param name="key">The key used to identify the metadata value. Cannot be null or empty.</param>
        /// <param name="factory">A function that creates a new metadata value of type T if the key does not exist.</param>
        /// <returns>The metadata value of type T associated with the specified key. If the key does not exist, a new value is
        /// created using the factory and stored.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="key"/> is null or empty.</exception>
        /// <exception cref="Exception">Thrown if the metadata value associated with <paramref name="key"/> exists but is not of type T.</exception>
        public T GetMetadata<T>(string key, Func<T> factory)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            if (Current.Metadata.TryGetValue(key, out var obj))
            {
                if (obj is T variableValue)
                    return variableValue;

                throw new($"Metadata variable '{key}' is not of type {typeof(T).FullName}.");
            }

            var value = factory();

            Current.Metadata[key] = value!;
            return value;
        }

        /// <summary>
        /// Sets a metadata entry with the specified key and value for the current context.
        /// </summary>
        /// <typeparam name="T">The type of the metadata value to associate with the specified key.</typeparam>
        /// <param name="key">The key used to identify the metadata entry. Cannot be null or empty.</param>
        /// <param name="value">The value to associate with the specified metadata key.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="key"/> is null or empty.</exception>
        public void SetMetadata<T>(string key, T value)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            Current.Metadata[key] = value!;
        }

        /// <summary>
        /// Attempts to save the specified output value to the current output variable.
        /// </summary>
        /// <remarks>The method returns false if the output is null or if the current output variable name
        /// is not set or is empty.</remarks>
        /// <param name="output">The value to be saved. Cannot be null.</param>
        /// <returns>true if the output was successfully saved; otherwise, false.</returns>
        public bool SaveOutput(object output)
        {
            if (output is null)
                return false;

            if (string.IsNullOrEmpty(Current.OutputVariableName))
                return false;

            SetMemory(Current.OutputVariableName, output);
            return true;
        }

        /// <summary>
        /// Ensures that all parameters are compiled by invoking the specified action for each parameter if compilation
        /// has not already occurred.
        /// </summary>
        /// <param name="parameter">An action to execute for each parameter, receiving the parameter index and the compiled parameter instance.
        /// Cannot be null.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="parameter"/> is null.</exception>
        public void EnsureCompiled(Func<int, CompiledParameter, bool> parameter)
        {
            if (parameter is null)
                throw new ArgumentNullException(nameof(parameter));

            if (Current.ParametersCompiled)
                return;

            Current.Parameters.For((index, p) => parameter(index, p));
        }
    }
}