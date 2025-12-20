using LabExtended.Core;

using MEC;

using SecretLabAPI.Actions.API;
using SecretLabAPI.Actions.Attributes;
using SecretLabAPI.Actions.Enums;

namespace SecretLabAPI.Actions.Functions.Blocks
{
    /// <summary>
    /// Provides functionality to execute a block of actions repeatedly while a specified condition evaluates to <see
    /// langword="true"/>. Supports optional delay between iterations and condition polarity reversal.
    /// </summary>
    public static class WhileBlock
    {
        /// <summary>
        /// Marks the end of a while block within an action sequence and signals that the block should be disposed.
        /// </summary>
        /// <param name="context">A reference to the current action context for the sequence. Used to maintain state and control flow within
        /// the action block.</param>
        /// <returns>An ActionResultFlags value indicating that the while block has completed and should be disposed.</returns>
        [Action("EndWhile", "A dummy while action used to determine the end of a while block.")]
        public static ActionResultFlags EndWhile(ref ActionContext context)
            => ActionResultFlags.SuccessDispose;

        /// <summary>
        /// Performs a loop that repeatedly executes a set of actions while a specified condition evaluates to <see
        /// langword="true"/>. The condition can be inverted by setting the Reverse parameter.
        /// </summary>
        /// <remarks>Set the Delay parameter to introduce a delay (in seconds) between loop iterations.
        /// The Reverse parameter allows the loop to continue while the condition is <see langword="false"/> instead of
        /// <see langword="true"/>. The method updates the context's iterator index to reflect the next action after the
        /// loop.</remarks>
        /// <param name="context">A reference to the current <see cref="ActionContext"/> containing loop parameters, memory, and player
        /// information. The context is updated to reflect the loop's execution state.</param>
        /// <returns>An <see cref="ActionResultFlags"/> value indicating the result of the loop execution. Returns <see
        /// cref="ActionResultFlags.SuccessDispose"/> if the loop completes successfully, or <see
        /// cref="ActionResultFlags.StopDispose"/> if the loop is terminated early.</returns>
        [Action("WhileTrue", "Performs an action while a condition is equal to true (or false if you specify the Reverse parameter as true)")]
        [ActionParameter("Delay", "Sets the delay of the while loop (in seconds).")]
        [ActionParameter("Reverse", "Whether or not the polarity of the condition should be reversed.")]
        public static ActionResultFlags WhileTrue(ref ActionContext context)
        {
            context.EnsureCompiled((index, p) =>
            {
                return index switch
                {
                    0 => p.EnsureCompiled(float.TryParse, 0f),
                    1 => p.EnsureCompiled(bool.TryParse, false),

                    _ => false
                };
            });

            var delay = context.GetValue<float>(0);
            var reverse = context.GetValue<bool>(1);

            if (delay > 0f)
                return WhileDelayed(ref context, delay);

            if (!context.Current.Metadata.TryGetValue("CompiledWhileTree", out var compiledWhileTreeObj)
                || compiledWhileTreeObj is not ValueTuple<List<CompiledAction>, List<CompiledAction>> compiledWhileTree)
            {
                if (!TryParse(ref context, out compiledWhileTree))
                    return ActionResultFlags.StopDispose;

                context.Current.Metadata["CompiledWhileTree"] = compiledWhileTree;
            }

            var output = compiledWhileTree.Item1[compiledWhileTree.Item1.Count - 1].OutputVariableName;

            var condsCtx = new ActionContext(compiledWhileTree.Item1, context.Player);
            var actionsCtx = new ActionContext(compiledWhileTree.Item2, context.Player);

            bool Evaluate()
            {
                condsCtx.Memory.Clear();

                for (condsCtx.Index = 0; condsCtx.Index < compiledWhileTree.Item1.Count; condsCtx.Index++)
                    compiledWhileTree.Item1[condsCtx.Index].Action.Delegate(ref condsCtx);

                var result = condsCtx.GetMemory<bool>(output);

                if (reverse)
                    result = !result;

                return result;
            }

            while (Evaluate())
            {
                actionsCtx.Memory.Clear();

                for (actionsCtx.Index = 0; actionsCtx.Index < compiledWhileTree.Item2.Count; actionsCtx.Index++)
                    compiledWhileTree.Item2[actionsCtx.Index].Action.Delegate(ref actionsCtx);
            }

            var nextIndex = context.Actions.FindIndex(context.Index, x => x == compiledWhileTree.Item2[compiledWhileTree.Item2.Count - 1]);

            context.Index = nextIndex + 1;

            condsCtx.Dispose();
            actionsCtx.Dispose();

            if (context.Index >= context.Actions.Count)
                return ActionResultFlags.StopDispose | ActionResultFlags.Success;

            return ActionResultFlags.SuccessDispose;
        }

        private static ActionResultFlags WhileDelayed(ref ActionContext context, float delay)
        {
            var reverse = context.GetValue<bool>(1);

            if (!context.Current.Metadata.TryGetValue("CompiledWhileTree", out var compiledWhileTreeObj)
                || compiledWhileTreeObj is not ValueTuple<List<CompiledAction>, List<CompiledAction>> compiledWhileTree)
            {
                if (!TryParse(ref context, out compiledWhileTree))
                    return ActionResultFlags.StopDispose;

                context.Current.Metadata["CompiledWhileTree"] = compiledWhileTree;
            }

            var output = compiledWhileTree.Item1[compiledWhileTree.Item1.Count - 1].OutputVariableName;

            var condsCtx = new ActionContext(compiledWhileTree.Item1, context.Player);
            var actionsCtx = new ActionContext(compiledWhileTree.Item2, context.Player);

            bool Evaluate()
            {
                condsCtx.Memory.Clear();

                for (condsCtx.Index = 0; condsCtx.Index < compiledWhileTree.Item1.Count; condsCtx.Index++)
                    compiledWhileTree.Item1[condsCtx.Index].Action.Delegate(ref condsCtx);

                var result = condsCtx.GetMemory<bool>(output);

                if (reverse)
                    result = !result;

                return result;
            }

            var ctx = context;

            IEnumerator<float> Coroutine()
            {
                while (Evaluate())
                {
                    actionsCtx.Memory.Clear();

                    for (actionsCtx.Index = 0; actionsCtx.Index < compiledWhileTree.Item2.Count; actionsCtx.Index++)
                        compiledWhileTree.Item2[actionsCtx.Index].Action.Delegate(ref actionsCtx);

                    yield return Timing.WaitForSeconds(delay);
                }

                condsCtx.Dispose();
                actionsCtx.Dispose();

                var nextIndex = ctx.Actions.FindIndex(ctx.Index, x => x == compiledWhileTree.Item2[compiledWhileTree.Item2.Count - 1]) + 1;

                if (nextIndex >= ctx.Actions.Count)
                {
                    ctx.Dispose();
                    yield break;
                }

                ctx.Actions.RemoveRange(nextIndex, ctx.Actions.Count - nextIndex);
                ctx.Actions.ExecuteActions(ctx.Player);

                ctx.Dispose();
            }

            Timing.RunCoroutine(Coroutine());
            return ActionResultFlags.Stop;
        }

        private static bool TryParse(ref ActionContext context, out (List<CompiledAction> Condition, List<CompiledAction> Actions) compiledWhileTree)
        {
            compiledWhileTree = new(new(), new());

            var endIndex = context.Actions.FindIndex(context.Index, x => x.Action.Id == "EndWhile");

            if (endIndex == -1)
            {
                ApiLog.Error("ActionManager", $"Error while compiling while block: No EndWhile function defined");
                return false;
            }

            var inActions = false;

            for (var i = context.Index + 1; i < endIndex - 1; i++)
            {
                var action = context.Actions[i];

                if (inActions)
                {
                    compiledWhileTree.Actions.Add(action);
                }
                else
                {
                    compiledWhileTree.Condition.Add(action);

                    if (action.Action.IsEvaluator)
                        inActions = true;
                }
            }

            if (compiledWhileTree.Condition.Count < 1)
            {
                ApiLog.Error("ActionManager", $"Error while compiling while block: No conditional actions specified");
                return false;
            }

            if (compiledWhileTree.Actions.Count < 1)
            {
                ApiLog.Error("ActionManager", $"Error while compiling while block: No actions specified");
                return false;
            }

            return true;
        }
    }
}