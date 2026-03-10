using LabExtended.API;

using LabExtended.Commands;
using LabExtended.Commands.Utilities;
using LabExtended.Commands.Attributes;
using LabExtended.Commands.Interfaces;

using SecretLabAPI.Features.Hats;

namespace SecretLabAPI.Commands;

/// <summary>
/// Represents a command to manage the hat system. This class is designed to handle
/// operations and functionalities related to hats within the system.
/// </summary>
[Command("hat", "Manages the hat system.")]
public class HatCommand : CommandBase, IServerSideCommand
{
    [CommandOverload("Applies a hat to specified players.", null)]
    private void Invoke(
        [CommandParameter("Players", "List of players to apply the hat to.")] List<ExPlayer> players, 
        [CommandParameter("Hat", "Name of the hat to apply.")] string hat,
        [CommandParameter("UpdateRotation", "Whether to update the schematic's rotation.")] bool updateRotation = true)
    {
        this.ForEachExecute(players, player =>
        {
            var effect = player.Effects.GetOrAddCustomEffect<HatEffect>(true);
            
            if (!effect.SetHat(hat))
                return "Could not set hat";
            
            return $"Hat set to: &1{hat}&r";
        });
    }

    [CommandOverload("disable", "Disables the hat on specified players.", null)]
    private void Disable(List<ExPlayer> players)
    {
        this.ForEachExecute(players, player =>
        {
            var effect = player.Effects.GetOrAddCustomEffect<HatEffect>(true);

            effect.RemoveHat();
            return "Hat removed";
        });
    }
}