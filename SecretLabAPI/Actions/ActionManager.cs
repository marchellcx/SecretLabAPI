using LabApi.Loader;
using LabApi.Loader.Features.Paths;

using LabExtended.API;

using LabExtended.Core;
using LabExtended.Utilities;
using LabExtended.Extensions;

using NorthwoodLib.Pools;

using SecretLabAPI.Actions.API;
using SecretLabAPI.Actions.Attributes;
using SecretLabAPI.Actions.Extensions;

using SecretLabAPI.Extensions;

using System.Reflection;

using SecretLabAPI.Actions.Enums;

using Utils.NonAllocLINQ;

namespace SecretLabAPI.Actions
{
    /// <summary>
    /// Helps with action executions.
    /// </summary>
    public static class ActionManager
    {
        public delegate void ContextSetupHandler(ref ActionContext ctx);
        
        /// <summary>
        /// Gets the action table.
        /// </summary>
        public static ActionTable Table { get; private set; } = new();

        /// <summary>
        /// Gets a dictionary of all compiled actions, keyed by their ID.
        /// </summary>
        public static Dictionary<string, ActionMethod> Actions { get; } = new();

        /// <summary>
        /// Registers all eligible actions defined in the specified assembly and returns the total number of actions
        /// registered.
        /// </summary>
        /// <param name="assembly">The assembly containing types whose actions will be registered. Cannot be null.</param>
        /// <returns>The total number of actions successfully registered from all types in the assembly.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="assembly"/> is null.</exception>
        public static int RegisterActions(Assembly assembly)
        {
            if (assembly is null)
                throw new ArgumentNullException(nameof(assembly));

            var count = 0;

            foreach (var type in assembly.GetTypes())
                count += RegisterActions(type);

            return count;
        }

        /// <summary>
        /// Registers all eligible action methods defined on the specified type and returns the number of actions
        /// successfully registered.
        /// </summary>
        /// <param name="type">The type whose methods will be scanned and registered as actions. Cannot be null.</param>
        /// <returns>The number of methods on the specified type that were successfully registered as actions.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="type"/> is null.</exception>
        public static int RegisterActions(Type type)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            var count = 0;

            foreach (var method in type.GetAllMethods())
            {
                if (!RegisterAction(method))
                    continue;

                count++;
            }

            return count;
        }

        /// <summary>
        /// Attempts to register a static method as an action if it meets the required signature and attribute
        /// constraints.
        /// </summary>
        /// <remarks>The method will only be registered if it matches the expected signature and is
        /// decorated with <see cref="ActionAttribute"/>. If registration fails due to signature mismatch or other
        /// constraints, the method returns false without throwing an exception.</remarks>
        /// <param name="method">The method to register as an action. Must be static, decorated with <see cref="ActionAttribute"/>, return
        /// <see cref="ActionResultFlags"/>, and accept parameters of type <see cref="ActionContext"/> by reference and
        /// <see cref="CompiledParameter"/>.</param>
        /// <returns>true if the method was successfully registered as an action; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="method"/> is null.</exception>
        public static bool RegisterAction(MethodInfo method)
        {
            if (method is null)
                throw new ArgumentNullException(nameof(method));

            if (!method.IsStatic)
                return false;

            if (!method.HasAttribute<ActionAttribute>(out var actionAttribute))
                return false;

            if (method.ReturnType != typeof(ActionResultFlags))
            {
                ApiLog.Warn("ActionManager", $"Could not register method &3{(method.DeclaringType ?? method.ReflectedType)?.Name ?? ""}.{method.Name}&r: " +
                                             $"The method's return type should be &6ActionResultFlags&r!");
                return false;
            }

            var parameters = method.GetParameters();

            if (parameters.Length != 1)
            {
                ApiLog.Warn("ActionManager", $"Could not register method &3{(method.DeclaringType ?? method.ReflectedType)?.Name ?? ""}.{method.Name}&r: " +
                                             $"The method's overload signature is incorrect! (expected one parameter)");
                return false;
            }

            if (parameters[0].ParameterType != typeof(ActionContext).MakeByRefType())
            {
                ApiLog.Warn("ActionManager", $"Could not register method &3{(method.DeclaringType ?? method.ReflectedType)?.Name ?? ""}.{method.Name}&r: " +
                                             $"The method's first parameter has to be a &3ref ActionContext&r type!");
                return false;
            }

            try
            {
                var actionDelegate = (ActionDelegate)method.CreateDelegate(typeof(ActionDelegate));

                if (actionDelegate is null)
                {
                    ApiLog.Warn("ActionManager", $"Could not register method &3{(method.DeclaringType ?? method.ReflectedType)?.Name ?? ""}.{method.Name}&r: " +
                                                 $"Delegate could not be compiled.");
                    return false;
                }

                var actionParameters = GetParameters(method);
                var actionMethod = new ActionMethod(actionAttribute.Id, actionAttribute.IsEvaluator, actionAttribute.SaveArgumentsOverflow,
                    actionDelegate, actionParameters);

                Actions[actionAttribute.Id] = actionMethod;

                if (method.DeclaringType is null || method.DeclaringType.Assembly != typeof(ActionManager).Assembly)
                    ApiLog.Info("ActionManager", $"Registered action &3{method.Name}&r with ID &6{actionAttribute.Id}&r");

                return true;
            }
            catch (Exception ex)
            {
                ApiLog.Error("ActionManager", $"Error registering action &3{method.Name}&r:\n{ex}");
                return false;
            }
        }

        /// <param name="action">The compiled action to execute. Cannot be null.</param>
        extension(CompiledAction action)
        {
            /// <summary>
            /// Executes the specified compiled action for the given player and optionally adjusts the action context using a
            /// setup handler.
            /// </summary>
            /// <param name="player">The player for whom the action is executed. Can be null for actions that don't target a specific player.</param>
            /// <param name="contextSetupHandler">An optional delegate for modifying the action context before execution. Can be null.</param>
            /// <returns>True if the action is successfully executed; otherwise, false.</returns>
            /// <exception cref="ArgumentNullException">Thrown if the compiled action associated with this execution is null.</exception>
            public bool ExecuteAction(ExPlayer? player, ContextSetupHandler? contextSetupHandler = null)
            {
                if (action is null)
                    throw new ArgumentNullException(nameof(action));

                var actions = ListPool<CompiledAction>.Shared.Rent(1);

                actions.Add(action);

                var result = actions.ExecuteActions(player, contextSetupHandler);

                ListPool<CompiledAction>.Shared.Return(actions);
                return result;
            }

            /// <summary>
            /// Executes the specified compiled action for the given player and returns a value indicating whether the
            /// execution was successful.
            /// </summary>
            /// <param name="player">The player for whom the action is executed. Cannot be null.</param>
            /// <param name="contextSetupHandler">An optional delegate to set up the action context before execution. Can be null.</param>
            /// <returns>true if the action was executed successfully; otherwise, false.</returns>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="action"/> or <paramref name="player"/> is null.</exception>
            public bool ExecuteAction(ContextSetupHandler? contextSetupHandler = null)
            {
                if (action is null)
                    throw new ArgumentNullException(nameof(action));

                return action.ExecuteAction(default(ExPlayer?), contextSetupHandler);
            }

            /// <summary>
            /// Executes the current action for the specified list of players, optionally using a context setup handler.
            /// </summary>
            /// <param name="players">The list of players on whom the action will be executed. Cannot be null or empty.</param>
            /// <param name="contextSetupHandler">
            /// An optional delegate for configuring the action's context before execution. Can be null if no custom setup is required.
            /// </param>
            /// <exception cref="ArgumentNullException">
            /// Thrown if <paramref name="players"/> is null or if the underlying action to be executed is null.
            /// </exception>
            public void ExecuteAction(List<ExPlayer> players, ContextSetupHandler? contextSetupHandler = null)
            {
                if (action is null)
                    throw new ArgumentNullException(nameof(action));

                if (players is null)
                    throw new ArgumentNullException(nameof(players));

                var list = ListPool<CompiledAction>.Shared.Rent(1);

                list.Add(action);
                list.ExecuteActions(players, contextSetupHandler);

                ListPool<CompiledAction>.Shared.Return(list);
            }
        }

        /// <param name="actions">The list of compiled actions to execute. Cannot be null.</param>
        extension(List<CompiledAction> actions)
        {
            /// <summary>
            /// Executes all compiled actions within the specified context, optionally allowing for customization of
            /// the action context using a delegate.
            /// </summary>
            /// <param name="contextSetupHandler">
            /// An optional delegate that allows modifications to the action context before the actions are executed. May be null.
            /// </param>
            /// <returns>True if all actions executed successfully; otherwise, false.</returns>
            /// <exception cref="ArgumentNullException">Thrown if the internal actions list is null.</exception>
            public bool ExecuteActions(ContextSetupHandler? contextSetupHandler = null)
            {
                if (actions is null)
                    throw new ArgumentNullException(nameof(actions));

                return actions.ExecuteActions(default(ExPlayer), contextSetupHandler);
            }

            /// <summary>
            /// Executes the specified actions for the given player within a newly created action context.
            /// Allows customization of the context through an optional context configuration handler.
            /// </summary>
            /// <param name="player">The player object that the actions will execute against. Cannot be null.</param>
            /// <param name="contextSetupHandler">
            /// An optional delegate allowing modifications to the action context before execution. May be null.
            /// </param>
            /// <returns>True if all actions executed successfully, otherwise false.</returns>
            /// <exception cref="ArgumentNullException">
            /// Thrown if <paramref name="player"/> is null or if the internal actions list is null.
            /// </exception>
            public bool ExecuteActions(ExPlayer? player, ContextSetupHandler? contextSetupHandler = null)
            {
                if (actions is null)
                    throw new ArgumentNullException(nameof(actions));

                if (player is null)
                    throw new ArgumentNullException(nameof(player));

                var context = new ActionContext(actions, player);

                if (contextSetupHandler != null)
                    contextSetupHandler(ref context);
                
                return ExecuteContext(ref context);
            }

            /// <summary>
            /// Executes a set of compiled actions for each player in the specified list, using an optional context setup handler
            /// to modify the action context before execution.
            /// </summary>
            /// <param name="players">The list of players for whom the actions will be executed. Cannot be null or empty.</param>
            /// <param name="contextSetupHandler">An optional handler for customizing the action context before execution.</param>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="players"/> or the list of actions is null.</exception>
            public void ExecuteActions(List<ExPlayer> players, ContextSetupHandler? contextSetupHandler = null)
            {
                if (actions is null)
                    throw new ArgumentNullException(nameof(actions));

                if (players is null)
                    throw new ArgumentNullException(nameof(players));

                if (actions.Count < 1)
                    return;

                if (players.Count < 1)
                    return;

                for (var i  = 0; i < players.Count; i++)
                {
                    var context = new ActionContext(actions, players[i]);

                    if (contextSetupHandler != null)
                        contextSetupHandler(ref context);

                    ExecuteContext(ref context);
                }
            }
        }

        /// <summary>
        /// Executes the actions within the given context in sequence and determines the success of the execution.
        /// </summary>
        /// <param name="context">A reference to the <see cref="ActionContext"/> containing the details of the actions to be executed. Cannot be null.</param>
        /// <param name="disposeContext">A boolean value indicating whether to dispose the context after execution. Defaults to true.</param>
        /// <returns>True if all actions in the context execute successfully; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the provided <paramref name="context"/> is null.</exception>
        /// <exception cref="Exception">Thrown if an unhandled exception occurs while executing any action in the context.</exception>
        public static bool ExecuteContext(ref ActionContext context, bool disposeContext = true)
        {
            for (context.IteratorIndex = 0; context.IteratorIndex < context.Actions.Count; context.IteratorIndex++)
            {
                if (context.IteratorIndex < 0 || context.IteratorIndex >= context.Actions.Count)
                {
                    if (disposeContext)
                        context.Dispose();

                    break;
                }

                var current = context.Actions[context.IteratorIndex];

                context.Previous = context.Current;
                context.Current = current;
                
                context.Next = context.IteratorIndex + 1 < context.Actions.Count 
                    ? context.Actions[context.IteratorIndex + 1] 
                    : null;
                
                try
                {
                    var flags = current.Action.Delegate(ref context);

                    if (flags.ShouldStop())
                    {
                        if (flags.ShouldDispose())
                            context.Dispose();

                        return flags.IsSuccess();
                    }

                    if (flags.IsSuccess()) 
                        continue;
                    
                    if (disposeContext)
                        context.Dispose();

                    ApiLog.Warn("ActionManager", $"Action &r{current.Action.Delegate.Method}&r returned unsuccessful result.");
                    return false;
                }
                catch (Exception ex)
                {
                    ApiLog.Error("ActionManager", $"Error executing compiled action &r{current.Action.Delegate.Method}&r:\n{ex}");

                    if (disposeContext)
                        context.Dispose();
                    
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Parses the provided lines into a list of action methods, assigning an optional identifier prefix
        /// to each generated action.
        /// </summary>
        /// <param name="lines">An array of strings where each line represents part of the action definition. Cannot be null or empty.</param>
        /// <param name="idPrefix">An optional identifier prefix to assign to each action. Can be null.</param>
        /// <returns>A list of <see cref="ActionMethod"/> objects created from the parsed lines. Returns an empty list if no valid actions are found.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="lines"/> is null.</exception>
        public static List<ActionMethod> ReadFromLines(string[] lines, string? idPrefix = null)
        {
            if (lines?.Length < 1)
                return new();

            var list = new List<ActionMethod>();
            
            var subList = new List<CompiledAction>();
            var subStringList = new List<string>();
            
            var id = string.Empty;
            var inMethod = false;

            void SaveAction()
            {
                if (!string.IsNullOrEmpty(id) && subStringList.Count > 0)
                {
                    if (!subStringList.ParseActions(subList))
                    {
                        ApiLog.Error("ActionManager", $"Could not parse actions of method &3{id}&r!");
                        
                        subList.Clear();
                        subStringList.Clear();

                        id = string.Empty;
                    }
                    else
                    {
                        var actions = new List<CompiledAction>(subList);
                        var last = subList.Last();

                        var del = new ActionDelegate((ref ctx) =>
                        {
                            actions.ExecuteActions(ctx.Player);
                            return ActionResultFlags.SuccessDispose;
                        });

                        var action = new ActionMethod(id, last.Action.IsEvaluator, last.Action.SaveArgumentsOverflow,
                            del,
                            Array.Empty<ActionParameter>());

                        list.Add(action);

                        ApiLog.Info("ActionManager",
                            $"Loaded action &3{id}&r with &6{actions.Count}&r instruction(s)!");
                    }
                }

                id = string.Empty;
                
                subList.Clear();
                subStringList.Clear();
            }

            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                
                if (string.IsNullOrEmpty(line))
                    continue;
                
                if (line.StartsWith("#"))
                    continue;

                if (line.StartsWith(":"))
                {
                    if (inMethod)
                        SaveAction();

                    inMethod = true;
                    
                    id = line.Substring(1);

                    if (idPrefix != null)
                        id = idPrefix + id;
                    
                    continue;
                }

                if (inMethod)
                {
                    subStringList.Add(line);
                    continue;
                }

                ApiLog.Error("ActionManager", $"Encountered a line without a parent method while parsing! (&6{line}&r)");
            }
            
            if (inMethod)
                SaveAction();
            
            subList.Clear();
            subStringList.Clear();

            return list;
        }

        /// <summary>
        /// Attempts to parse a list of string representations into compiled actions.
        /// </summary>
        /// <remarks>Parsing will fail if the input list is empty, if the actions list is null, or if no
        /// valid actions are found in the input. Each string in the input list may contain multiple actions separated
        /// by semicolons. Invalid or unknown action identifiers are skipped.</remarks>
        /// <param name="value">A list of strings containing action definitions to be parsed. Each string should follow the expected format
        /// for action specification.</param>
        /// <param name="actions">A list to which successfully parsed and compiled actions will be added. Must not be null.</param>
        /// <returns>true if at least one action was successfully parsed and added to the actions list; otherwise, false.</returns>
        public static bool ParseActions(this List<string> value, List<CompiledAction>? actions)
        {
            if (Actions.Count < 1)
                return false;

            if (actions is null)
                return false;

            if (value.Count < 1)
                return false;

            for (var i = 0; i < value.Count; i++)
            {
                var trimmed = value[i].Trim();

                if (string.IsNullOrEmpty(trimmed))
                    continue;

                // Format of actions:
                // Single action:
                // - ActionID "Arg" "Arg" "Arg"
                // Multiple actions:
                // - ActionID "Arg" "Arg"; ActionID "Arg"; ActionID "Arg" "Arg" "Arg"

                // ActionArg; ActionAndArgs; ActionAndArgs;
                var parts = trimmed.SplitEscaped(';');

                if (parts.Length < 1)
                    continue;

                for (var x = 0; x < parts.Length; x++)
                {
                    var part = parts[x].Trim();

                    if (string.IsNullOrEmpty(part))
                        continue;

                    var spaceIndex = part.IndexOf(' ');

                    string actionId = string.Empty;
                    string[] actionArgs = Array.Empty<string>();
                    
                    if (spaceIndex == -1)
                    {
                        actionId = part;
                        actionArgs = Array.Empty<string>();
                    }
                    else
                    {
                        actionId = part.Substring(0, spaceIndex).Trim();
                        actionArgs = part.Substring(spaceIndex + 1).Trim().SplitOutsideQuotes(' ', true, true, true);
                    }

                    if (!Actions.TryGetValue(actionId, out var action))
                    {
                        ApiLog.Error("ActionManager", $"Failed to compile action &3{part}&r: unknown action ID (&6{actionId}&r)");
                        continue;
                    }

                    var resultAction = action.CompileAction(actionArgs);

                    if (resultAction is null)
                    {
                        ApiLog.Error("ActionManager", $"Failed to compile action: &3{part}&r");
                        continue;
                    }

                    actions.Add(resultAction);
                }
            }

            return actions.Count > 0;
        }

        // Arguments can either be assigned by their position or by their name (using a key=value format).
        /// <summary>
        /// Compiles the specified action method and its arguments into a structured representation for execution.
        /// </summary>
        /// <remarks>If more arguments are provided than expected and the action method allows overflow,
        /// excess arguments are stored in the metadata. The first argument may specify an output variable if it begins
        /// with '$'. Compilation fails and returns null if required arguments are missing, empty, or
        /// duplicated.</remarks>
        /// <param name="method">The action method definition to compile. Must not be null and should contain parameter metadata describing
        /// expected arguments.</param>
        /// <param name="args">An array of argument strings to assign to the action method's parameters. Arguments may be provided by
        /// position or by name using the key=value format.</param>
        /// <returns>A CompiledAction instance representing the compiled action and its arguments, or null if compilation fails
        /// due to invalid or missing arguments.</returns>
        public static CompiledAction? CompileAction(this ActionMethod method, string[] args)
        {
            var array = new CompiledParameter[method.Parameters.Length];
            var output = default(string);
            var comp = new CompiledAction(method, array, string.Empty);

            for (var i = 0; i < args.Length; i++)
            {
                if (i >= method.Parameters.Length)
                {
                    if (method.SaveArgumentsOverflow)
                    {
                        if (!comp.Metadata.TryGetValue("ArgsOverflow", out var argsOverflowObj)
                            || argsOverflowObj is not List<string> argsOverflow)
                            comp.Metadata["ArgsOverflow"] = argsOverflow = new();

                        argsOverflow.Add(args[i].Trim());
                        continue;
                    }

                    ApiLog.Warn("ActionManager", $"Error while compiling action (&3{method.Id}&r): Too many arguments were provided.");
                    return null;
                }

                var compiledArg = new CompiledParameter();
                var arg = args[i].Trim();

                if (string.IsNullOrEmpty(arg))
                {
                    ApiLog.Error("ActionManager", $"Error while compiling action (&3{method.Id}&r): Argument &6{method.Parameters[i].Name}&r was provided as empty.");
                    return null;
                }

                if (output is null && i == 0 && arg[0] == '$')
                {
                    output = arg.Substring(1).Trim();
                    args = args.Skip(1).ToArray();

                    i--;
                    continue;
                }

                var splitArg = arg.SplitEscaped('=');

                if (splitArg.Length == 2)
                {
                    var argKey = splitArg[0].Trim();
                    var argValue = splitArg[1].Trim();
                    var argIndex = method.Parameters.FindIndex(x => string.Equals(x.Name, argKey, StringComparison.InvariantCultureIgnoreCase));

                    if (argIndex == -1)
                    {
                        ApiLog.Error("ActionManager", $"Error while compiling action (&3{method.Id}&r): No argument labeled &6{argKey}&r was found.");
                        return null;
                    }

                    if (array[argIndex] != null)
                    {
                        ApiLog.Error("ActionManager", $"Error while compiling action (&3{method.Id}&r): Duplicate argument &6{argKey}&r was provided.");
                        return null;
                    }

                    compiledArg.Source = argValue;

                    array[argIndex] = compiledArg;
                }
                else
                {
                    compiledArg.Source = arg;

                    if (array[i] != null)
                    {
                        ApiLog.Error("ActionManager", $"Error while compiling action (&3{method.Id}&r): Duplicate argument &6{method.Parameters[i].Name}&r was provided.");
                        return null;
                    }

                    array[i] = compiledArg;
                }
            }

            if (args.Length < method.Parameters.Length)
            {
                for (var i = 0; i < method.Parameters.Length; i++)
                {
                    if (array[i] != null)
                        continue;

                    array[i] = new() { Source = string.Empty };
                }
            }

            comp.OutputVariableName = output;
            return comp;
        }

        private static ActionParameter[] GetParameters(MethodInfo method)
        {
            var attributes = method.GetCustomAttributes<ActionParameterAttribute>();
            var count = attributes.Count();

            if (count < 1)
                return Array.Empty<ActionParameter>();

            var parameters = new ActionParameter[count];
            var index = 0;

            foreach (var attribute in attributes)
            {
                parameters[index] = new(index, attribute.Name) { Description = attribute.Description };

                index++;
            }

            return parameters;
        }

        internal static void Initialize()
        {
            PluginLoader.Plugins.ForEachValue(asm => RegisterActions(asm));

            var path = Path.Combine(PathManager.SecretLab.FullName, "actions");
            var examplePath = Path.Combine(path, "example.txt");

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            File.WriteAllText(examplePath,
                $"# This file serves as an example for custom action definitions\n" +
                $"# Each file can contain multiple action definitions, each definition is separated by a comma before it's name, ex. :ID\n\n" +
                $"# Actions are defined per-line like this:" +
                $"# ActionID \"ArgumentOne\" \"ArgumentTwo\" and so on\n\n" +
                $"# Can also define multiple actions on one line:\n" +
                $"# ActionID \"ArgumentOne\" \"ArgumentTwo\"; ActionID \"ArgumentOne\" \"ArgumentTwo\"\n\n" +
                $"# Both of these lines will spawn an item on all players and log a message to the console:\n" +
                $":ExampleAction\n" +
                $"GetPlayers \"$Players\"\n" +
                $"Execute \"SpawnItem\" \"$Players\" \"Medkit\" \"2\"\n" +
                $"Log \"Spawned two medkits at all players\"\n\n" +
                $":ExampleActionOneLine\n" +
                $"GetPlayers \"$Players\"; Execute \"SpawnItem\" \"$Players\" \"Medkit\" \"2\"\n\n" +
                $"# And as for parsing, lines starting with a # and empty lines are ignored\n" +
                $"# Good luck!");

            foreach (var file in Directory.GetFiles(path, "*.txt"))
            {
                if (file == examplePath)
                    continue;
                
                var actions = ReadFromLines(File.ReadAllLines(file), null);
                
                actions.ForEach(act => Actions[act.Id] = act);
            }
            
            foreach (var dir in Directory.GetDirectories(path))
            {
                var dirName = Path.GetDirectoryName(dir) + "/";

                foreach (var subFile in Directory.GetFiles(dir, "*.txt"))
                {
                    var actions = ReadFromLines(File.ReadAllLines(subFile), dirName);
                    
                    actions.ForEach(act => Actions[act.Id] = act);
                }
            }

            if (FileUtils.TryLoadYamlFile<ActionTable>(SecretLab.RootDirectory, "action_table.yml", out var actionTable))
            {
                Table = actionTable;
                Table.CacheTables();
            }
            else
            {
                FileUtils.TrySaveYamlFile(SecretLab.RootDirectory, "action_table.yml", Table);
            }
            
            ActionEvents.Initialize();
            CoinActions.Initialize();
        }
    }
}