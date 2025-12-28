using LabExtended.API;

using LabExtended.Commands;
using LabExtended.Commands.Attributes;
using LabExtended.Commands.Interfaces;

using SecretLabAPI.Utilities;

using UnityEngine;

namespace SecretLabAPI.Commands
{
    /// <summary>
    /// Provides server-side commands to start or stop an item rain effect on specified players.
    /// </summary>
    /// <remarks>This command is intended for administrative use to control item rain events for one or more
    /// players. The command supports starting and stopping item rain with customizable parameters such as item type,
    /// amount, delay, and scale. Only users with appropriate permissions should execute these commands, as they affect
    /// gameplay for targeted players.</remarks>
    [Command("rain", "Starts an item rain on the specified players.")]
    public class ItemRainCommand : CommandBase, IServerSideCommand
    {
        [CommandOverload("start", "Starts an item rain on the specified players.", null)]
        private void Start(
            [CommandParameter("Targets", "List of players to start the rain on.")] List<ExPlayer> targets, 
            [CommandParameter("Type", "The item type that should rain.")] ItemType item,
            [CommandParameter("Amount", "The amount of items that should be spawned per drop.")] int amount,
            [CommandParameter("Delay", "The delay between each drop (in milliseconds).")] int delay, 
            [CommandParameter("Scale", "Scale of dropped items (defaults to one)")] Vector3 scale = default)
        {
            if (scale == default)
                scale = Vector3.one;

            foreach (ExPlayer target in targets)
                ItemRain.StartItemRain(target, item, amount, delay, scale);

            Ok("Started item rain on " + targets.Count + " players.");
        }

        [CommandOverload("stop", "Stops an item rain on the specified players.", null)]
        private void Stop(
            [CommandParameter("Targets", "The list of players to stop the rain on.")] List<ExPlayer> targets)
        {
            var amount = 0;

            foreach (var target in targets)
            {
                if (ItemRain.StopItemRain(target))
                {
                    amount++;
                }
            }

            Ok("Stopped item rain on " + amount + " players.");
        }
    }
}
