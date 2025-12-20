using LabApi.Events.Arguments.Interfaces;

using LabExtended.API;
using LabExtended.Core;
using LabExtended.Utilities;

using NorthwoodLib.Pools;

using SecretLabAPI.Actions.API;
using SecretLabAPI.Utilities;

namespace SecretLabAPI.Actions
{
    /// <summary>
    /// Provides functionality for managing and executing action-related events within the application.
    /// </summary>
    /// <remarks>
    /// ActionEvents acts as a central hub to manage triggers associated with the execution of actions.
    /// It facilitates the loading and saving of configuration files, as well as initializing event subscriptions.
    /// The class primarily interacts with YAML configuration files to define and manage triggers.
    /// </remarks>
    /// <example>
    /// This class internally handles YAML-based configuration files such as action_events.yml for defining triggers.
    /// It also subscribes to relevant events for executing associated actions when configured triggers are executed.
    /// </example>
    /// <threadsafety>
    /// The static members of this class are thread-safe.
    /// </threadsafety>
    public static class ActionEvents
    {
        private static Dictionary<string, List<string>> config = new();

        /// <summary>
        /// A dictionary that groups and maps specific event names to corresponding lists of compiled actions
        /// that are triggered when the associated event is executed.
        /// </summary>
        /// <remarks>
        /// The <c>Triggers</c> property allows registering and retrieving collections of actions tied
        /// to specific event types. These actions are executed sequentially in response to the event.
        /// </remarks>
        /// <example>
        /// The property contains event names as keys (strings) and lists of <see cref="CompiledAction"/>
        /// as values. When an event matching a key is executed, the associated actions are processed.
        /// </example>
        public static Dictionary<string, List<CompiledAction>> Triggers { get; } = new();
        
        private static void OnExecuted(Type type, object args)
        {
            if (Triggers.TryGetValue(type.Name, out var actions))
            {
                var list = ListPool<CompiledAction>.Shared.Rent();

                var context = new ActionContext(list,
                    args is IPlayerEvent { Player: ExPlayer player }
                        ? player
                        : null);
                
                try
                {
                    list.Add(actions[0]);

                    for (var x = 0; x < actions.Count; x++)
                    {
                        if (x != 0)
                        {
                            var action = actions[x];

                            list[0] = action;

                            context.Current = action;
                        }

                        context.Index = 0;

                        ActionManager.ExecuteContext(ref context, false);
                    }
                }
                catch (Exception ex)
                {
                    ApiLog.Error("ActionEvents", $"Error while handling event &3{type.Name}&r:\n{ex}");
                }

                context.Dispose();
                
                ListPool<CompiledAction>.Shared.Return(list);
            }
        }

        internal static void Initialize()
        {
            if (FileUtils.TryLoadYamlFile(SecretLab.RootDirectory, "action_events.yml", out config))
            {
                InitializeTriggers();

                if (Triggers.Count > 0)
                    LabEvents.Executed += OnExecuted;
            }
            else
            {
                FileUtils.TrySaveYamlFile(SecretLab.RootDirectory, "action_events.yml", config);
            }
        }

        private static void InitializeTriggers()
        {
            foreach (var pair in config)
            {
                var list = new List<CompiledAction>();
                
                foreach (var name in pair.Value)
                {
                    if (ActionManager.Actions.TryGetValue(name, out var method))
                    {
                        list.Add(new(method, Array.Empty<CompiledParameter>(), null));
                    }
                    else
                    {
                        ApiLog.Warn("ActionEvents", $"Unknown action (in event &6{pair.Key}&r: &1{name}&r");
                    }
                }

                if (list.Count > 0)
                {
                    Triggers[pair.Key] = list;
                }
            }
        }
    }
}