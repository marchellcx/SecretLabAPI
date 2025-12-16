using SecretLabAPI.Actions;
using SecretLabAPI.Actions.API;

namespace SecretLabAPI.RandomEvents
{
    /// <summary>
    /// Represents a custom random event that extends a custom gamemode with specific
    /// event actions that can be invoked during gameplay.
    /// </summary>
    public class CustomRandomEvent : RandomEventBase
    {
        /// <inheritdoc />
        public override string Id { get; }

        /// <summary>
        /// Gets the action method that is executed when the random event starts.
        /// </summary>
        public CompiledAction StartAction { get; }

        /// <summary>
        /// Represents an optional action method that is invoked to stop or finalize the behavior
        /// of a custom random event. This action is executed at the conclusion or deactivation
        /// of the event, where applicable.
        /// </summary>
        public CompiledAction? StopAction { get; }

        /// <summary>
        /// Gets the collection of action methods associated with the custom random event.
        /// These actions represent specific functionalities or tasks that can be invoked
        /// during the lifecycle of the event.
        /// </summary>
        public List<CompiledAction> Actions { get; }

        /// <summary>
        /// Represents a custom random event within a game environment. This class extends
        /// the base functionality of a custom gamemode, allowing the addition of unique
        /// event actions that can be triggered during gameplay.
        /// </summary>
        public CustomRandomEvent(string id, CompiledAction startAction, CompiledAction? stopAction,
            List<CompiledAction> actions)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException(nameof(id));

            if (startAction is null)
                throw new ArgumentNullException(nameof(startAction));

            if (actions is null)
                throw new ArgumentNullException(nameof(actions));
            
            Id = id;
            
            StartAction = startAction;
            StopAction = stopAction;
            
            Actions = actions;
        }

        /// <inheritdoc />
        public override void OnEnabled()
        {
            base.OnEnabled();

            StartAction.ExecuteAction();
        }

        /// <inheritdoc />
        public override void OnDisabled()
        {
            base.OnDisabled();

            StopAction?.ExecuteAction();
        }
    }
}