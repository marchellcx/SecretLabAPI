using LabExtended.API;
using LabExtended.Core;

using MEC;

using NorthwoodLib.Pools;

using SecretLabAPI.Actions.API;
using SecretLabAPI.Actions.Enums;
using SecretLabAPI.Actions.Attributes;
using SecretLabAPI.Actions.Extensions;

using System.Collections;

using UnityEngine;

namespace SecretLabAPI.Actions.Functions
{
    /// <summary>
    /// Provides logic-based utility functions for performing repeated or conditional actions within an action sequence.
    /// </summary>
    public static class LogicFunctions
    {
        /// <summary>
        /// Halts the execution of the current action sequence if the specified variable evaluates to TRUE.
        /// </summary>
        /// <remarks>
        /// This method checks the value of a BOOLEAN variable from the provided context. If the variable is TRUE,
        /// the execution stops and the associated resources are disposed of. Otherwise, execution continues while
        /// releasing resources as needed.
        /// </remarks>
        /// <param name="context">A reference to the action context containing information about the current execution state,
        /// including the variable to evaluate.</param>
        /// <returns>A flag indicating the result of the execution:
        /// ActionResultFlags.SuccessStop | ActionResultFlags.Dispose if the variable is TRUE,
        /// or ActionResultFlags.SuccessDispose otherwise.</returns>
        [Action("StopIf", "Stops the execution if a variable is equal to TRUE.")]
        [ActionParameter("Variable", "The name of the variable (must be a BOOLEAN).")]
        public static ActionResultFlags StopIf(ref ActionContext context)
        {
            context.EnsureCompiled((_, p) => p.EnsureCompiled(bool.TryParse, false));
            
            var boolean = context.GetValue<bool>(0);

            return boolean
                ? ActionResultFlags.SuccessStop | ActionResultFlags.Dispose
                : ActionResultFlags.SuccessDispose;
        }

        [Action("StopIfAndExecute", "Stops the execution path and invokes the specified amount of following actions if $VARIABLE is TRUE.")]
        [ActionParameter("Variable", "The variable to check.")]
        [ActionParameter("Amount", "The amount of instructions to invoke or skip.")]
        public static ActionResultFlags StopIfAndExecute(ref ActionContext context)
        {
            context.EnsureCompiled((i, p) =>
            {
                return i switch
                {
                    0 => p.EnsureCompiled(bool.TryParse, false),
                    1 => p.EnsureCompiled(int.TryParse, 1),
                    
                    _ => false
                };
            });

            var boolean = context.GetValue<bool>(0);
            var amount = context.GetValue<int>(1);

            if (!boolean)
            {
                context.Index += amount;
                return ActionResultFlags.SuccessDispose;
            }

            var startIndex = context.Index + 1;
            var endIndex = startIndex + amount;

            if (startIndex >= context.Actions.Count)
            {
                ApiLog.Warn("Actions :: StopIfAndExecute", $"Start Index is out of range! (EndIndex={endIndex}; StartIndex={startIndex}; Count={context.Actions.Count}; Amount={amount})");
                return ActionResultFlags.StopDispose;
            }

            if (endIndex >= context.Actions.Count)
            {
                ApiLog.Warn("Actions :: StopIfAndExecute", $"End Index is out of range! (EndIndex={endIndex}; StartIndex={startIndex}; Count={context.Actions.Count}; Amount={amount})");
                return ActionResultFlags.StopDispose;
            }

            for (var x = startIndex; x < endIndex; x++)
            {
                context.Index = x;
                context.Current = context.Actions[x];

                if (x > 0) 
                    context.Previous = context.Actions[x - 1];
                else 
                    context.Previous = null;

                if (x + 1 < context.Actions.Count) 
                    context.Next = context.Actions[x + 1];
                else 
                    context.Next = null;

                try
                {
                    var flags = context.Current.Action.Delegate(ref context);
                    
                    if (flags.ShouldStop())
                    {
                        if (flags.ShouldDispose())
                            context.Dispose();

                        context.Index = endIndex + 1;
                        return flags;
                    }

                    if (flags.IsSuccess()) 
                        continue;
                    
                    return ActionResultFlags.StopDispose;
                }
                catch (Exception ex)
                {
                    ApiLog.Error("Actions :: StopIfAndExecute", ex);
                    return ActionResultFlags.StopDispose;
                }
            }

            return ActionResultFlags.SuccessDispose | ActionResultFlags.Stop;
        }
        
        /// <summary>
        /// Iterates through a list of players and executes a specified action for each player within the context provided.
        /// </summary>
        /// <remarks>
        /// The method ensures the action is compiled and cached before execution. If a list of players is specified,
        /// the action is executed in context of each player in the list. Errors during execution are logged, and
        /// resources are disposed of appropriately.
        /// </remarks>
        /// <param name="context">A reference to the action context containing metadata, parameters, and player information.
        /// This must include the action ID and the player list variable.</param>
        /// <returns>A value indicating the result of the action execution. Returns ActionResultFlags.SuccessDispose if the
        /// action completes successfully, or another value if specific errors occur.</returns>
        [Action("ForEach", "Executes an action.", false, true)]
        [ActionParameter("ID", "The ID of the action to execute.")]
        [ActionParameter("Players", "The name of the player list variable")]
        public static ActionResultFlags ForEach(ref ActionContext context)
        {
            context.EnsureCompiled((index, p) =>
            {
                return index switch
                {
                    0 or 1 => p.EnsureCompiled(string.Empty),

                    _ => false
                };
            });

            var actionId = context.GetValue(0);
            var players = context.GetValue<List<ExPlayer>>(0);
            var argsOverflow = context.GetMetadata<List<string>>("ArgsOverflow", () => new());

            var compiledAction = context.GetMetadata("CompiledExecute", () =>
            {
                if (ActionManager.Actions.TryGetValue(actionId, out var action))
                    return action.CompileAction(argsOverflow.ToArray())!;
                
                ApiLog.Error("ActionManager", $"Could not execute the &3Execute&r action: Action &1{actionId}&r could not be found");
                return null!;

            });

            if (compiledAction is null)
            {
                ApiLog.Error("ActionManager", $"Could not execute the &3Execute&r action: Action &1{actionId}&r could not be compiled");
                return ActionResultFlags.StopDispose;
            }

            var subList = ListPool<CompiledAction>.Shared.Rent(1);
            var subContext = new ActionContext(subList, players[0]);

            subList.Add(compiledAction);

            for (var i = 0; i < players.Count; i++)
            {
                if (i != 0)
                {
                    subContext.Player = players[i];
                    subContext.Memory.Clear();
                }

                subContext.Index = 0;
                
                try
                {
                    compiledAction.Action.Delegate(ref subContext);
                }
                catch (Exception ex)
                {
                    ApiLog.Error("ActionManager",
                        $"Error while executing action &3{compiledAction.Action.Id}&r:\n{ex}");
                }
            }

            ListPool<CompiledAction>.Shared.Return(subList);

            subContext.Dispose();
            return ActionResultFlags.SuccessDispose;
        }
        
        /// <summary>
        /// Executes the specified action within the provided action context, compiling and invoking the action as
        /// needed.
        /// </summary>
        /// <remarks>If the action has not been previously compiled, it will be compiled and cached in the
        /// context metadata. Errors during execution are logged, and the method ensures proper disposal of resources
        /// associated with the action context.</remarks>
        /// <param name="context">A reference to the action context containing parameters and metadata required for execution. Must be
        /// initialized and contain valid action identifiers and player information.</param>
        /// <returns>A value indicating the result of the action execution. Returns ActionResultFlags.SuccessDispose if the
        /// action executes successfully; otherwise, returns ActionResultFlags.StopDispose if the action cannot be found
        /// or compiled.</returns>
        [Action("For", "Executes an action.", false, true)]
        [ActionParameter("ID", "The ID of the action to execute.")]
        [ActionParameter("Player", "The name of the player variable")]
        public static ActionResultFlags For(ref ActionContext context)
        {
            context.EnsureCompiled((index, p) =>
            {
                return index switch
                {
                    0 or 1 => p.EnsureCompiled(string.Empty),

                    _ => false
                };
            });

            var actionId = context.GetValue(0);
            var playerVar = context.GetValue(1);

            var argsOverflow = context.GetMetadata<List<string>>("ArgsOverflow", () => new());

            var compiledAction = context.GetMetadata("CompiledExecute", () =>
            {
                if (ActionManager.Actions.TryGetValue(actionId, out var action))
                    return action.CompileAction(argsOverflow.ToArray())!;
                
                ApiLog.Error("ActionManager", $"Could not execute the &3Execute&r action: Action &1{actionId}&r could not be found");
                return null!;

            });

            if (compiledAction is null)
            {
                ApiLog.Error("ActionManager", $"Could not execute the &3Execute&r action: Action &1{actionId}&r could not be compiled");
                return ActionResultFlags.StopDispose;
            }

            var subList = ListPool<CompiledAction>.Shared.Rent(1);
            var subContext = new ActionContext(subList, context.GetMemory<ExPlayer>(playerVar));

            subList.Add(compiledAction);

            try
            {
                compiledAction.Action.Delegate(ref subContext);
            }
            catch (Exception ex)
            {
                ApiLog.Error("ActionManager", $"Error while executing action &3{compiledAction.Action.Id}&r:\n{ex}");
            }

            ListPool<CompiledAction>.Shared.Return(subList);

            subContext.Dispose();
            return ActionResultFlags.SuccessDispose;
        }

        /// <summary>
        /// Repeatedly invokes the next action in the sequence a specified number of times.
        /// </summary>
        /// <remarks>If the "Amount" parameter is not present or cannot be parsed as an integer, the
        /// action will default to zero repetitions. Any exceptions thrown by the repeated actions are logged and do not
        /// interrupt the repetition loop.</remarks>
        /// <param name="context">A reference to the current action context, which provides access to the action sequence and execution state.
        /// The context must contain an "Amount" parameter specifying the number of repetitions.</param>
        /// <returns>An ActionResultFlags value indicating the result of the repeated execution. Returns SuccessDispose if the
        /// actions were repeated successfully; otherwise, returns StopDispose if there are no further actions to
        /// repeat.</returns>
        [Action("Repeat", "Repeatedly calls an action for a specified amount of times.")]
        [ActionParameter("Amount", "The amount of times to repeat the action.")]
        [ActionParameter("Offset", "The offset of the end index from the current action index to repeat multiple actions.")]
        [ActionParameter("Delay", "The delay in seconds between each repetition.")]
        public static ActionResultFlags Repeat(ref ActionContext context)
        {
            if (context.Index + 1 >= context.Actions.Count)
            {
                ApiLog.Error("ActionManager", "Could not repeat action: No further actions to repeat.");
                return ActionResultFlags.StopDispose;
            }

            context.EnsureCompiled((index, p) =>
            {
                return index switch
                {
                    0 => p.EnsureCompiled(int.TryParse, 1),
                    1 => p.EnsureCompiled(int.TryParse, 0),
                    2 => p.EnsureCompiled(float.TryParse, 0f),

                    _ => false
                };
            });

            var amount = context.GetValue<int>(0);
            var offset = context.GetValue<int>(1);
            var delay = context.GetValue<float>(2);

            var startIndex = context.Index + 1;
            var endIndex = startIndex + 1 + offset;

            if (endIndex > context.Actions.Count)
                endIndex = context.Actions.Count;

            context.Index = endIndex + 1;

            var ctx = context;

            IEnumerator<float> _Coroutine()
            {
                for (var i = 0; i < amount; i++)
                {
                    for (var x = startIndex; x < endIndex; x++)
                    {
                        try
                        {
                            ctx.Actions[x].Action.Delegate(ref ctx);
                        }
                        catch (Exception ex)
                        {
                            ApiLog.Error("ActionManager", $"Could not repeat function &3{ctx.Actions[x].Action.Id}&r:\n{ex}");
                        }
                    }

                    yield return Timing.WaitForSeconds(delay);
                }

                ctx.Dispose();
            }

            if (delay > 0f)
            {
                Timing.RunCoroutine(_Coroutine());
                return ActionResultFlags.Stop | ActionResultFlags.Success;
            }

            for (var i = 0; i < amount; i++)
            {
                for (var x = startIndex; x < endIndex; x++)
                {
                    try
                    {
                        context.Actions[x].Action.Delegate(ref context);
                    }
                    catch (Exception ex)
                    {
                        ApiLog.Error("ActionManager", $"Could not repeat function &3{context.Actions[x].Action.Id}&r:\n{ex}");
                    }
                }
            }

            return ActionResultFlags.SuccessDispose;
        }

        /// <summary>
        /// Generates a random float value within a specified range and saves it to the action context output.
        /// </summary>
        /// <param name="context">The action context containing parameters for the range.</param>
        /// <returns>Returns SuccessDispose if the random value was generated and saved successfully.</returns>
        [Action("Range", "Generates a random float value within a specified range.")]
        [ActionParameter("MinValue", "The minimum value of the range (inclusive).")]
        [ActionParameter("MaxValue", "The maximum value of the range (inclusive).")]
        public static ActionResultFlags Range(ref ActionContext context)
        {
            context.EnsureCompiled((index, p) =>
            {
                return index switch
                {
                    0 => p.EnsureCompiled(float.TryParse, 0f),
                    1 => p.EnsureCompiled(float.TryParse, 1f),

                    _ => false
                };
            });

            var minValue = context.GetValue<float>(0);
            var maxValue = context.GetValue<float>(1);

            var randomValue = UnityEngine.Random.Range(minValue, maxValue);

            context.SetMemory(randomValue);
            return ActionResultFlags.SuccessDispose;
        }

        /// <summary>
        /// Selects a random item or a specified number of random items from a source collection within the provided
        /// action context.
        /// </summary>
        /// <remarks>If the amount parameter is set to one or less, a single random item is selected from
        /// the collection. If the amount is greater than one, a list containing up to the specified number of unique
        /// random items is selected. The method ensures that the number of items selected does not exceed the size of
        /// the source collection.</remarks>
        /// <param name="context">A reference to the action context containing the source collection and parameters for selection. Must not be
        /// null.</param>
        /// <returns>An ActionResultFlags value indicating the result of the selection operation. The selected item(s) are saved
        /// to the context output.</returns>
        [Action("Select", "Selects a random item or multiple items from a collection.")]
        [ActionParameter("Variable", "Name of the source collection variable.")]
        [ActionParameter("Amount", "The amount of items to select from the collection. Setting this to one (default) will return a singular object, " +
                                   "while setting it to more than one will return a list.")]
        public static ActionResultFlags Select(ref ActionContext context)
        {
            context.EnsureCompiled((index, p) =>
            {
                return index switch
                {
                    0 => p.EnsureCompiled(string.Empty),
                    1 => p.EnsureCompiled(int.TryParse, 1),

                    _ => false
                };
            });

            var source = context.GetValue(0);
            var amount = context.GetValue<int>(1);

            var enumerable = context.GetMemory<IEnumerable>(source);
            var enumerator = enumerable.GetEnumerator();

            var count = 0;

            while (enumerator.MoveNext())
                count++;

            enumerator.Reset();

            if (amount > 0)
            {
                amount = Mathf.Min(count, amount);

                var type = enumerable.GetType().GetGenericArguments()[0];

                var listType = typeof(List<>).MakeGenericType(type);
                var list = Activator.CreateInstance(listType) as IList;

                while (amount > 0 && list.Count < amount)
                {
                    while (enumerator.MoveNext())
                    {
                        if (UnityEngine.Random.Range(0, 1) == 1)
                        {
                            list.Add(enumerator.Current);
                        }
                    }

                    enumerator.Reset();
                }

                context.SetMemory(list);
            }
            else
            {
                var randomIndex = UnityEngine.Random.Range(0, count);
                var currentIndex = 0;

                while (enumerator.MoveNext())
                {
                    if (currentIndex == randomIndex)
                    {
                        context.SetMemory(enumerator.Current!);
                        break;
                    }

                    currentIndex++;
                }
            }

            enumerator.Reset();

            if (enumerator is IDisposable disposable)
                disposable.Dispose();

            return ActionResultFlags.SuccessDispose;
        }
    }
}