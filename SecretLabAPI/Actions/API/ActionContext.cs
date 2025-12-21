using LabExtended.API;

using LabExtended.Core;
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
        public int Index;

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
        public ExPlayer? Player;

        /// <summary>
        /// The list of actions to be performed.
        /// </summary>
        public List<CompiledAction> Actions;

        /// <summary>
        /// A dictionary to store temporary data between actions.
        /// </summary>
        public Dictionary<string, object> Memory;

        /// <summary>
        /// Represents the context for an action, containing relevant state
        /// information such as the list of actions, associated player, and
        /// a memory pool for temporary data storage. Used to facilitate the
        /// execution of compiled actions and manage the associated resources.
        /// </summary>
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
            if (string.IsNullOrWhiteSpace(variable))
                throw new ArgumentNullException(nameof(variable));
            
            if (!Memory.TryGetValue(variable, out var obj))
                throw new($"Memory variable '{variable}' not found.");

            if (obj is T variableValue)
                return variableValue;

            throw new($"Memory variable '{variable}' is not of type {typeof(T).FullName}.");
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
            
            ApiLog.Debug("Actions", $"Method &3{Current.Action.Id}&r saved a memory variable: &1{value}&r (&6{typeof(T).Name}&r)");
        }

        /// <summary>
        /// Retrieves the value of a parameter at the specified index within the current context, with an option to resolve memory-based variables.
        /// </summary>
        /// <param name="index">The zero-based index of the parameter to retrieve.</param>
        /// <param name="allowMemory">A boolean indicating whether to resolve memory-based variables when applicable. If true, memory variables prefixed with '$' will be resolved.</param>
        /// <returns>The value of the parameter at the specified index, optionally resolved from memory.</returns>
        /// <exception cref="IndexOutOfRangeException">Thrown when the specified index is outside the range of the available parameters.</exception>
        public string GetValue(int index, bool allowMemory = true)
        {
            if (index < 0 || index >= Current.Parameters.Length)
                throw new IndexOutOfRangeException($"Parameter index {index} is out of range.");

            var variable = Current.Parameters[index].Source;

            if (allowMemory && variable.Length > 0)
            {
                if (variable[0] == '$')
                {
                    var obj = ResolveMemoryObject(variable);

                    if (obj != null)
                    {
                        if (obj is string str)
                            return str;

                        return obj.ToString();
                    }

                    throw new($"Failed to resolve memory variable '{variable}' to string (null return)");
                }
            }

            return variable;
        }
        
        /// <summary>
        /// Retrieves the value of a parameter at the specified index within the current action context,
        /// optionally allowing access to memory-sourced data. Supports type casting to the specified type.
        /// </summary>
        /// <typeparam name="T">The expected type of the parameter's value.</typeparam>
        /// <param name="index">The zero-based index of the parameter to retrieve.</param>
        /// <param name="allowMemory">
        /// A boolean indicating whether values stored in memory should be considered
        /// when retrieving the parameter's value.
        /// </param>
        /// <returns>The value of the parameter at the specified index, cast to the specified type.</returns>
        /// <exception cref="IndexOutOfRangeException">
        /// Thrown when the specified index is out of the range of the parameter array.
        /// </exception>
        public T GetValue<T>(int index, bool allowMemory = true)
        {
            if (index < 0 || index >= Current.Parameters.Length)
                throw new IndexOutOfRangeException($"Parameter index {index} is out of range.");

            var variable = Current.Parameters[index];

            if (allowMemory && variable.Source.Length > 0)
            {
                if (variable.Source[0] == '$')
                {
                    var obj = ResolveMemoryObject(variable.Source);

                    if (obj is T memoryValue)
                        return memoryValue;

                    throw new($"Failed to resolve memory variable '{variable.Source}' to type '{typeof(T).Name}'");
                }
            }

            return variable.GetValue<T>();
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
        public bool SetMemory(object output)
        {
            if (output is null)
            {
                ApiLog.Warn("Actions", $"Method &3{Current.Action.Id}&r attempted to save a null output variable!");
                return false;
            }

            if (string.IsNullOrEmpty(Current.OutputVariableName))
            {
                ApiLog.Warn("Actions", $"Method &3{Current.Action.Id}&r attempted to save an output variable without having a name defined!");
                return false;
            }

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

        private object ResolveMemoryObject(string variableName)
        {
            if (Memory.TryGetValue(variableName, out var memoryObj))
                return memoryObj;

            var outputName = $"Comp{variableName}_AutoGen";
            var cleanName = variableName.Substring(1);
            var methodParts = cleanName.SplitOutsideQuotes(' ', '\'');
            var methodName = methodParts[0].Trim();
            var methodArgs = methodParts.Skip(1).ToArray();

            if (!ActionManager.Actions.TryGetValue(methodName, out var method))
                throw new($"Memory variable '{variableName} (method {methodName})' could not be loaded");

            var compiled = GetMetadata(outputName, () =>
            {
                var array = new CompiledParameter[method.Parameters.Length];

                for (var x = 0; x < method.Parameters.Length; x++)
                {
                    if (x >= methodArgs.Length)
                    {
                        array[x] = new() { Source = string.Empty };
                    }
                    else
                    {
                        array[x] = new() { Source = methodArgs[x].Trim() };
                    }
                }
                
                return new CompiledAction(method, array, outputName);
            });

            compiled.OutputVariableName = outputName;

            var list = ListPool<CompiledAction>.Shared.Rent();
            var ctx = new ActionContext(list, Player);
            
            list.Add(compiled);

            try
            {
                ActionManager.ExecuteContext(ref ctx, false);
            }
            catch (Exception ex)
            {
                ctx.Dispose();
                
                ListPool<CompiledAction>.Shared.Return(list);

                throw ex;
            }
            
            ListPool<CompiledAction>.Shared.Return(list);

            if (!ctx.Memory.TryGetValue(outputName, out memoryObj))
            {
                ctx.Dispose();
                
                throw new($"Method '{variableName} (method {methodName})' did not save an output variable");
            }

            return memoryObj;
        }
    }
}