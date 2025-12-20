using LabExtended.API;
using LabExtended.Core;

using SecretLabAPI.Actions.API;
using SecretLabAPI.Actions.Attributes;
using SecretLabAPI.Actions.Enums;

namespace SecretLabAPI.Actions.Functions.Blocks
{
    /// <summary>
    /// A block that represents an "if" conditional statement.
    /// </summary>
    public static class IfBlock
    {
        internal enum ActionType
        {
            If,
            EndIf,
            ElseIf
        }

        /// <summary>
        /// Marks the end of an 'if' block during action parsing and signals successful disposal of the block.
        /// </summary>
        /// <param name="context">A reference to the current action context used for parsing and managing block state.</param>
        /// <returns>An ActionResultFlags value indicating that the block was successfully disposed.</returns>
        [Action("EndIf", "A dummy action used only for parsing if blocks.")]
        public static ActionResultFlags EndIf(ref ActionContext context)
            => ActionResultFlags.SuccessDispose;

        /// <summary>
        /// Represents a placeholder action for parsing conditional 'else if' blocks within an action sequence.
        /// </summary>
        /// <remarks>This method is intended solely for use by the parsing infrastructure and does not
        /// perform any runtime logic. It should not be invoked directly in application code.</remarks>
        /// <param name="actionContext">A reference to the current action context used during parsing. The context is updated as the action sequence
        /// progresses.</param>
        /// <returns>An ActionResultFlags value indicating that the action was successfully processed and should be disposed.</returns>
        [Action("ElseIf", "A dummy action used only for parsing if blocks.")]
        public static ActionResultFlags ElseIf(ref ActionContext actionContext)
            => ActionResultFlags.SuccessDispose;

        /// <summary>
        /// Begins an if block within an action sequence, enabling conditional execution of subsequent actions based on
        /// evaluated conditions.
        /// </summary>
        /// <remarks>This method must be followed by at least one evaluating function and one action to
        /// form a valid conditional block. The method evaluates conditions and executes the corresponding actions for
        /// 'if' and 'else if' branches. The context's metadata is used to cache the compiled conditional tree for
        /// efficient repeated execution.</remarks>
        /// <param name="context">A reference to the current action context, which provides metadata and state required for evaluating
        /// conditions and executing actions. Must not be null.</param>
        /// <returns>An ActionResultFlags value indicating the outcome of the conditional block. Returns SuccessDispose if the
        /// block completes successfully; otherwise, returns StopDispose if evaluation or execution fails.</returns>
        [Action("If", "Starts an if block - at least one evaluating function must follow (and at least one action must follow that function!)")]
        public static ActionResultFlags If(ref ActionContext context)
        {
            if (!context.Current.Metadata.TryGetValue("CompiledIfTree", out var compiledIfTreeObj)
                || compiledIfTreeObj is not List<(ActionType Type, List<CompiledAction> Conditions, List<CompiledAction> Actions)> compiledIfTree)
            {
                if (!TryParseActions(ref context, out compiledIfTree))
                    return ActionResultFlags.StopDispose;

                context.Current.Metadata["CompiledIfTree"] = compiledIfTree;
            }

            var ifTree = compiledIfTree.Find(x => x.Type == ActionType.If);
            var endIfTree = compiledIfTree.Find(x => x.Type is ActionType.EndIf);

            if (!TryInvokeTree(ifTree.Conditions, context.Player, out var conditionResult))
            {
                ApiLog.Error("ActionManager", "Could not compile if-block");
                return ActionResultFlags.StopDispose;
            }

            if (!conditionResult)
            {
                for (var i = 0; i < compiledIfTree.Count; i++)
                {
                    var tree = compiledIfTree[i];

                    if (tree.Type != ActionType.ElseIf)
                        continue;

                    if (!TryInvokeTree(tree.Conditions, context.Player, out conditionResult))
                        return ActionResultFlags.StopDispose;

                    if (!conditionResult)
                        continue;

                    tree.Actions.ExecuteActions(context.Player);
                    break;
                }
            }
            else
            {
                ifTree.Actions.ExecuteActions(context.Player);
            }

            context.Index = context.Actions.IndexOf(endIfTree.Conditions[endIfTree.Conditions.Count - 1]);
            return ActionResultFlags.SuccessDispose;
        }

        private static bool TryInvokeTree(List<CompiledAction> actions, ExPlayer player, out bool conditionResult)
        {
            conditionResult = false;

            var variable = actions[actions.Count - 1].OutputVariableName;

            if (variable == null)
            {
                ApiLog.Warn("ActionManager", $"Error while executing an if-block: The last statement must have an output variable defined.");
                return false;
            }

            var context = new ActionContext(actions, player);

            for (context.Index = 0; context.Index < actions.Count; context.Index++)
            {
                var action = actions[context.Index];

                try
                {
                    action.Action.Delegate(ref context);
                }
                catch (Exception ex)
                {
                    context.Dispose();

                    ApiLog.Error("ActionManager", $"Error while executing an if-block:\n{ex}");
                    return false;
                }
            }

            if (!context.Memory.TryGetValue(variable, out var conditionObj))
            {
                ApiLog.Warn("ActionManager", $"Error while executing an if-block: The last action has not saved an output.");
                return false;
            }

            if (conditionObj is not bool result)
            {
                ApiLog.Warn("ActionManager", $"Error while executing an if-block: The last action did not save a boolean variable.");
                return false;
            }

            conditionResult = result;
            return true;
        }

        private static bool TryParseActions(ref ActionContext context, out
            List<(ActionType Type, List<CompiledAction> Conditions, List<CompiledAction> Actions)> actions)
        {
            actions = new();

            var ifStartIndex = context.Index;
            var ifEndIndex = context.Actions.FindIndex(ifStartIndex, x => x.Action.Id == "EndIf");

            if (ifEndIndex == -1)
            {
                ApiLog.Error("ActionManager", $"Error while compiling an if-block: A block must have the &3EndIf&3 action at it's end!");
                return false;
            }

            var type = ActionType.If;

            var targets = new List<CompiledAction>();
            var conditions = new List<CompiledAction>();

            var inActions = false;
            var inConditions = true;

            for (var i = ifStartIndex + 1; i < ifEndIndex - 1; i++)
            {
                var action = context.Actions[i];

                if (action.Action.Id == "ElseIf")
                {
                    if (type is ActionType.If or ActionType.ElseIf)
                    {
                        if (conditions.Count < 1)
                        {
                            ApiLog.Error("ActionManager", $"Error while compiling an if-block: A block must have at least one condition!");
                            return false;
                        }

                        if (targets.Count < 1)
                        {
                            ApiLog.Error("ActionManager", $"Error while compiling an if-block: A block must have at least one function!");
                            return false;
                        }

                        actions.Add((type, conditions, targets));
                    }

                    type = ActionType.ElseIf;

                    conditions = new();
                    targets = new();

                    inActions = false;
                    inConditions = true;

                    continue;
                }

                if (action.Action.IsEvaluator)
                {
                    conditions.Add(action);

                    inConditions = false;
                    inActions = true;

                    continue;
                }

                if (inConditions)
                    conditions.Add(action);

                if (inActions)
                    targets.Add(action);
            }

            return true;
        }
    }
}