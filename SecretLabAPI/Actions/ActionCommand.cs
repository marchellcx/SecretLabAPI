using LabExtended.API;

using LabExtended.Commands;
using LabExtended.Commands.Attributes;
using LabExtended.Commands.Interfaces;

using LabExtended.Extensions;

using SecretLabAPI.Actions.API;

namespace SecretLabAPI.Actions
{
    /// <summary>
    /// Represents the base command for all action-related operations on the server side.
    /// </summary>
    [Command("action", "Base command for all action related commands")]
    public class ActionCommand : CommandBase, IServerSideCommand
    {
        [CommandOverload("actions", "Lists all registered actions.", null)]
        private void Actions()
        {
            if (ActionManager.Actions.Count < 1)
            {
                Fail($"No actions were registered.");
                return;
            }

            Ok(x =>
            {
                x.AppendLine();

                foreach (var pair in ActionManager.Actions)
                {
                    x.AppendLine($"- {pair.Key}");
                }
            });
        }

        [CommandOverload("action", "Shows detailed information about an action.", null)]
        private void Action([CommandParameter("Name", "Name of the action.")] string name)
        {
            if (!ActionManager.Actions.TryGetValue(name, out var action))
            {
                Fail($"No such action ({name})");
                return;
            }

            Ok(x =>
            {
                x.AppendLine($"ID: {action.Id}");
                x.AppendLine($"Method: {action.Delegate.Method}");
                x.AppendLine($"Parameters: {action.Parameters.Length}");

                for (int i = 0; i < action.Parameters.Length; i++)
                {
                    var p = action.Parameters[i];

                    x.AppendLine($"[{i} / {p.Index}] {p.Name} ({p.Description})");
                }
            });
        }

        [CommandOverload("tables", "Lists all loaded action tables by group.", null)]
        private void Tables()
        {
            ActionManager.Table.CacheTables();

            if (ActionManager.Table.Parsed.Count < 1)
            {
                Fail($"No action tables were loaded.");
                return;
            }

            Ok(x =>
            {
                x.AppendLine();

                foreach (var table in ActionManager.Table.Parsed)
                    x.AppendLine($"- {table.Name} ({table.Weight}%, {table.Actions?.Count ?? 0} actions)");
            });
        }

        [CommandOverload("table", "Shows a detailed description of a table.", null)]
        private void Table([CommandParameter("Name", "The name of the table to show the description of.")] string name)
        {
            ActionManager.Table.CacheTables();

            if (!ActionManager.Table.Parsed.TryGetFirst(x => x.Name == name, out var table))
            {
                Fail($"No action table of name '{name}' was found.");
                return;
            }

            Ok(x =>
            {
                x.AppendLine();

                x.AppendLine($"- Name: {table.Name}");
                x.AppendLine($"- Weight: {table.Weight}");

                if (table.Multipliers != null)
                {
                    if (table.Multipliers.Multipliers?.Count > 0)
                    {
                        foreach (var pair in table.Multipliers.Multipliers)
                        {
                            x.AppendLine($"  -> {pair.Key}: * {pair.Value}");
                        }
                    }

                    if (table.Multipliers.LevelMultipliers?.Count > 0)
                    {
                        foreach (var pair in table.Multipliers.LevelMultipliers)
                        {
                            x.AppendLine($"  -> LVL {pair.Key}: * {pair.Value}");
                        }
                    }
                }

                if (table.Actions?.Count > 0)
                {
                    x.AppendLine($"- Actions: {table.Actions.Count}");

                    foreach (var action in table.Actions)
                    {
                        if (action.OutputVariableName != null)
                        {
                            x.AppendLine($" -> {action.Action?.Id ?? "(null)"}: {action.OutputVariableName}");
                        }
                        else
                        {
                            x.AppendLine($" -> {action.Action?.Id ?? "(null)"}");
                        }

                        if (action.Parameters?.Length > 0)
                        {
                            x.AppendLine($"  -> Parameters: {action.Parameters.Length} ({action.ParametersCompiled})");

                            foreach (var p in action.Parameters)
                            {
                                x.AppendLine($"   <- {p?.Source ?? "(null)"}");
                            }
                        }

                        if (action.Metadata?.Count > 0)
                        {
                            x.AppendLine($"  -> Metadata: {action.Metadata.Count}");

                            foreach (var m in action.Metadata)
                            {
                                x.AppendLine($"   <- {m.Key}: {m.Value?.GetType().Name ?? "(null)"}");
                            }
                        }
                    }
                }
            });
        }

        [CommandOverload("run", "Runs a table.", null)]
        private void Run(
            [CommandParameter("Name", "Name of the table to run.")] string name, 
            [CommandParameter("Target", "The target player to run the table on.")] ExPlayer? target = null)
        {
            ActionManager.Table.CacheTables();

            if (!ActionManager.Table.Parsed.TryGetFirst(x => x.Name == name, out var table))
            {
                Fail($"No such table.");
                return;
            }

            table.Actions.ExecuteActions(target ?? Sender);

            Ok($"Invoked table '{table.Name}'");
        }

        [CommandOverload("invoke", "Attempts to parse and invoke an action expression.", null)]
        private void Invoke(
            [CommandParameter("Expression", "The action expression to parse.")] string expression, 
            [CommandParameter("Target", "The target player to invoke the expression on.")] ExPlayer? target = null)
        {
            target ??= Sender;

            var list = new List<CompiledAction>();

            if (!ActionManager.ParseActions([expression], list))
            {
                Fail($"The expression could not be parsed - the console will contain more details.");
                return;
            }

            if (list.ExecuteActions(target))
                Ok($"Executed {list.Count} action(s) on player {target.ToCommandString()}");
            else
                Fail($"Execution failed.");
        }
    }
}