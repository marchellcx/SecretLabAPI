using LabExtended.API;
using LabExtended.Commands;
using LabExtended.Commands.Attributes;
using LabExtended.Commands.Interfaces;

namespace ObscurisCore.Features.SchematicModels.Commands;

/// <summary>
/// Represents a command for managing replacement schematic models within the system.
/// This command facilitates interaction with schematic model configurations and behaviors.
/// </summary>
[Command("schemmodel", "Manages replacement schematic models.")]
public class SchematicModelCommand : CommandBase, IServerSideCommand
{
    [CommandOverload("set", "Sets a player's schematic model.", null)]
    private void Set(
        [CommandParameter("Player", "The player whose model should be replaced.")] ExPlayer player, 
        [CommandParameter("Name", "Name of the schematic model.")] string name)
    {
        if (!SchematicModelReplacer.Replace(player, name))
        {
            Fail($"Could not replace schematic model &1{name}&r for: {player.ToLogString()}");
            return;
        }

        Ok($"Replaced schematic model &1{name}&r for: {player.ToLogString()}");
    }

    [CommandOverload("remove", "Removes a player's schematic model.", null)]
    private void Remove(
        [CommandParameter("Player", "The player whose schematic model to remove.")] ExPlayer player)
    {
        SchematicModelReplacer.Remove(player);
        
        Ok($"Removed schematic model {player.ToLogString()}");
    }
}